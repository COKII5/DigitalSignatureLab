using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureLab.Networking
{
    public class TcpPacketSender
    {
        public async Task SendAsync(string host, int port, string payload)
        {
            byte[] body = Encoding.UTF8.GetBytes(payload);
            byte[] lengthPrefix = BitConverter.GetBytes(body.Length);
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(body, 0, body.Length);
            }
        }
    }
}
