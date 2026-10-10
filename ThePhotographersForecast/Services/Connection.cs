using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ThePhotographersForecast.Services
{
    // Minimal Connection helper for controlled async reads and dispose ordering
    public class Connection : IAsyncDisposable
    {
        readonly TcpClient _tcpClient;
        readonly SslStream _ssl;
        readonly CancellationTokenSource _shutdownCts = new();
        Task? _receiveTask;
        int _activeReads;
        TaskCompletionSource<object?>? _noActiveReadsTcs;
        readonly object _sync = new();
        bool _disposed;

        public Connection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _ssl = new SslStream(_tcpClient.GetStream(), leaveInnerStreamOpen: true);
        }

        public Task AuthenticateAsClientAsync(string targetHost, CancellationToken cancellationToken = default)
        {
            var options = new SslClientAuthenticationOptions
            {
                TargetHost = targetHost,
                EnabledSslProtocols = SslProtocols.None, // let OS choose, or set SslProtocols.Tls12 | SslProtocols.Tls13
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck // corresponds to checkCertificateRevocation: false
            };

            return _ssl.AuthenticateAsClientAsync(options, cancellationToken);
        }
        public void StartReceiving(Func<ReadOnlyMemory<byte>, Task> onMessage, int bufferSize = 4096)
        {
            if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));
            if (_disposed) throw new ObjectDisposedException(nameof(Connection));
            _receiveTask ??= Task.Run(() => ReceiveLoopAsync(onMessage, bufferSize, _shutdownCts.Token));
        }

        async Task ReceiveLoopAsync(Func<ReadOnlyMemory<byte>, Task> onMessage, int bufferSize, CancellationToken token)
        {
            var buffer = new byte[bufferSize];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Volatile.Read(ref _disposed) != false) break;

                    Interlocked.Increment(ref _activeReads);
                    int bytesRead;
                    try
                    {
                        bytesRead = await _ssl.ReadAsync(buffer.AsMemory(0, buffer.Length), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref _activeReads) == 0)
                        {
                            lock (_sync)
                            {
                                _noActiveReadsTcs?.TrySetResult(null);
                            }
                        }
                    }

                    if (bytesRead == 0) break; // remote closed
                    await onMessage(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception) { /* log if needed */ }
        }

        public async Task StopAsync()
        {
            if (_disposed) return;
            _shutdownCts.Cancel();
            if (_receiveTask != null) await _receiveTask.ConfigureAwait(false);
            await DisposeAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            TaskCompletionSource<object?>? tcs;
            lock (_sync)
            {
                if (_noActiveReadsTcs == null)
                    _noActiveReadsTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                tcs = _noActiveReadsTcs;
            }

            if (Volatile.Read(ref _activeReads) != 0)
                await tcs.Task.ConfigureAwait(false);

            _ssl.Dispose();
            _tcpClient.Dispose();
            _shutdownCts.Dispose();
        }
    }
}
