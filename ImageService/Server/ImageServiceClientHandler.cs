using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    public class ImageServiceClientHandler : IClientHandler
    {
        public void HandleClient(TcpClient c)
        {
            NetworkStream netStream = c.GetStream();
            if (netStream.CanRead)
            {
                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[c.ReceiveBufferSize];

                // Read can return anything from 0 to numBytesToRead. 
                // This method blocks until at least one byte is read.
                netStream.Read(bytes, 0, (int)c.ReceiveBufferSize);

                // Returns the data received from the host to the console.
                string returndata = Encoding.UTF8.GetString(bytes);

                Console.WriteLine("This is what the host returned to you: " + returndata);

            }
            else
            {
                Console.WriteLine("You cannot read data from this stream.");
                c.Close();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return;
            }
            netStream.Close();
        }
    }
}
