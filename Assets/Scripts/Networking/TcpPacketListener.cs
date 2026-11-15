using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureLab.Networking
{
    public class TcpPacketListener
    {
        private const int LengthPrefixBytes = 4;
        private const int MaxPayloadBytes = 1048576;

        private readonly ConcurrentQueue<string> receivedPayloads = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> receiveErrors = new ConcurrentQueue<string>();

        private TcpListener listener;
        private volatile bool isListening;

        public void Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            isListening = true;
            _ = AcceptClientsAsync();
        }

        public void Stop()
        {
            isListening = false;
            listener?.Stop();
        }

        public bool TryDequeuePayload(out string payload)
        {
            return receivedPayloads.TryDequeue(out payload);
        }

        public bool TryDequeueError(out string error)
        {
            return receiveErrors.TryDequeue(out error);
        }

        private async Task AcceptClientsAsync()
        {
            while (isListening)
            {
                try
                {
                    using (TcpClient client = await listener.AcceptTcpClientAsync())
                    {
                        receivedPayloads.Enqueue(await ReadPayloadAsync(client.GetStream()));
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (SocketException exception)
                {
                    if (isListening)
                    {
                        receiveErrors.Enqueue(exception.Message);
                    }
                }
                catch (Exception exception) when (exception is IOException || exception is InvalidDataException)
                {
                    receiveErrors.Enqueue(exception.Message);
                }
            }
        }

        private static async Task<string> ReadPayloadAsync(NetworkStream stream)
        {
            byte[] lengthPrefix = await ReadExactlyAsync(stream, LengthPrefixBytes);
            int payloadLength = BitConverter.ToInt32(lengthPrefix, 0);
            if (payloadLength <= 0 || payloadLength > MaxPayloadBytes)
            {
                throw new InvalidDataException("The packet length is out of range.");
            }

            byte[] body = await ReadExactlyAsync(stream, payloadLength);
            return Encoding.UTF8.GetString(body);
        }

        private static async Task<byte[]> ReadExactlyAsync(NetworkStream stream, int count)
        {
            byte[] buffer = new byte[count];
            int totalRead = 0;
            while (totalRead < count)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalRead, count - totalRead);
                if (bytesRead == 0)
                {
                    throw new IOException("The connection closed before the packet was complete.");
                }

                totalRead += bytesRead;
            }

            return buffer;
        }
    }
}
