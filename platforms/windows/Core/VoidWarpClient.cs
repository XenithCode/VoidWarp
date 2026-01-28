using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VoidWarp.Windows.Native;

namespace VoidWarp.Windows.Core
{
    /// <summary>
    /// High-level wrapper around VoidWarp native bindings.
    /// Provides simplified async API for file sending and receiving.
    /// </summary>
    public class VoidWarpClient : IDisposable
    {
        private bool _disposed = false;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Event fired when progress changes during send/receive operations
        /// </summary>
        public event Action<float>? ProgressChanged;

        /// <summary>
        /// Event fired when a log message is generated
        /// </summary>
        public event Action<string>? LogMessage;

        public VoidWarpClient()
        {
            Log("VoidWarpClient initialized");
        }

        /// <summary>
        /// Send a file to the specified IP address and port.
        /// Runs on a background thread to avoid blocking the UI.
        /// </summary>
        /// <param name="ip">Target IP address</param>
        /// <param name="port">Target port</param>
        /// <param name="filePath">Path to the file to send</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> SendFileAsync(string ip, ushort port, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                Log("Error: IP address is required");
                return false;
            }

            if (!File.Exists(filePath))
            {
                Log($"Error: File not found: {filePath}");
                return false;
            }

            Log($"Starting file send: {Path.GetFileName(filePath)} to {ip}:{port}");

            return await Task.Run(() =>
            {
                IntPtr senderHandle = IntPtr.Zero;
                try
                {
                    // Create TCP sender
                    senderHandle = NativeBindings.voidwarp_tcp_sender_create(filePath);
                    if (senderHandle == IntPtr.Zero)
                    {
                        Log("Error: Failed to create TCP sender");
                        return false;
                    }

                    var fileSize = NativeBindings.voidwarp_tcp_sender_get_file_size(senderHandle);
                    Log($"File size: {fileSize / 1024.0 / 1024.0:F2} MB");

                    // Start progress monitoring task
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    var progressTask = MonitorSendProgressAsync(senderHandle, _cts.Token);

                    // Start the transfer (blocking call)
                    var senderName = Environment.MachineName;
                    int result = NativeBindings.voidwarp_tcp_sender_start(senderHandle, ip, port, senderName);

                    // Stop progress monitoring
                    _cts.Cancel();
                    try { progressTask.Wait(1000); } catch { }

                    // Handle result
                    switch (result)
                    {
                        case 0:
                            Log("✓ File sent successfully");
                            ProgressChanged?.Invoke(100f);
                            return true;
                        case 1:
                            Log("✗ Transfer rejected by receiver");
                            return false;
                        case 2:
                            Log("✗ Checksum mismatch");
                            return false;
                        case 3:
                            Log($"✗ Connection failed to {ip}:{port}");
                            return false;
                        case 4:
                            Log("✗ Transfer timeout");
                            return false;
                        case 5:
                            Log("✗ Transfer cancelled");
                            return false;
                        default:
                            Log($"✗ Unknown error: {result}");
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Exception during send: {ex.Message}");
                    return false;
                }
                finally
                {
                    if (senderHandle != IntPtr.Zero)
                    {
                        NativeBindings.voidwarp_tcp_sender_destroy(senderHandle);
                    }
                    _cts?.Dispose();
                    _cts = null;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Start a file receiver that listens for incoming transfers.
        /// Runs on a background thread and accepts transfers automatically.
        /// </summary>
        /// <param name="savePath">Directory where received files will be saved</param>
        /// <param name="cancellationToken">Cancellation token to stop the receiver</param>
        /// <returns>Task that completes when receiver is stopped</returns>
        public async Task StartReceiverAsync(string savePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(savePath))
            {
                Log("Error: Save path is required");
                return;
            }

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Log($"Created save directory: {savePath}");
            }

            Log($"Starting receiver, save path: {savePath}");

            await Task.Run(async () =>
            {
                IntPtr receiverHandle = IntPtr.Zero;
                try
                {
                    // Create receiver
                    receiverHandle = NativeBindings.voidwarp_create_receiver();
                    if (receiverHandle == IntPtr.Zero)
                    {
                        Log("Error: Failed to create receiver");
                        return;
                    }

                    var port = NativeBindings.voidwarp_receiver_get_port(receiverHandle);
                    Log($"Receiver listening on port {port}");

                    // Start receiver (blocking call on background thread)
                    var startTask = Task.Run(() =>
                    {
                        NativeBindings.voidwarp_receiver_start(receiverHandle);
                    });

                    // Poll for incoming transfers
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var state = (ReceiverState)NativeBindings.voidwarp_receiver_get_state(receiverHandle);

                        switch (state)
                        {
                            case ReceiverState.AwaitingAccept:
                                // Get pending transfer info
                                var pending = NativeBindings.voidwarp_receiver_get_pending(receiverHandle);
                                if (pending.IsValid)
                                {
                                    var fileName = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(pending.FileName) ?? "unknown";
                                    var fileSize = pending.FileSize;
                                    Log($"📥 Incoming: {fileName} ({fileSize / 1024.0 / 1024.0:F2} MB)");
                                    NativeBindings.voidwarp_free_pending_transfer(pending);

                                    // Auto-accept and save to specified path
                                    var fullSavePath = Path.Combine(savePath, fileName);
                                    int acceptResult = NativeBindings.voidwarp_receiver_accept(receiverHandle, fullSavePath);
                                    
                                    if (acceptResult == 0)
                                    {
                                        Log($"✓ File received: {fullSavePath}");
                                    }
                                    else
                                    {
                                        Log($"✗ Receive failed with result: {acceptResult}");
                                    }
                                }
                                break;

                            case ReceiverState.Receiving:
                                var progress = NativeBindings.voidwarp_receiver_get_progress(receiverHandle);
                                ProgressChanged?.Invoke(progress);
                                break;

                            case ReceiverState.Error:
                                Log("Receiver encountered an error");
                                break;
                        }

                        await Task.Delay(200, cancellationToken);
                    }

                    Log("Receiver stopped");
                }
                catch (OperationCanceledException)
                {
                    Log("Receiver cancelled");
                }
                catch (Exception ex)
                {
                    Log($"Receiver exception: {ex.Message}");
                }
                finally
                {
                    if (receiverHandle != IntPtr.Zero)
                    {
                        NativeBindings.voidwarp_receiver_stop(receiverHandle);
                        NativeBindings.voidwarp_destroy_receiver(receiverHandle);
                    }
                }
            }, cancellationToken);
        }

        private async Task MonitorSendProgressAsync(IntPtr senderHandle, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var progress = NativeBindings.voidwarp_tcp_sender_get_progress(senderHandle);
                    ProgressChanged?.Invoke(progress);

                    if (progress >= 100f)
                    {
                        break;
                    }

                    await Task.Delay(200, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when transfer completes
            }
            catch (Exception ex)
            {
                Log($"Progress monitoring error: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            Debug.WriteLine($"[VoidWarpClient] {message}");
            LogMessage?.Invoke(message);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
