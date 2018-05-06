using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    public class ImageServiceClientHandler : IClientHandler
    {
        private NetworkStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        public void HandleClient(TcpClient client)
        {
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
            new Task(() =>
            {
                System.Diagnostics.Debugger.Launch();
                string commandLine = reader.ReadString();
                Console.WriteLine("Got command: {0}", commandLine);
                //string result = ExecuteCommand(commandLine, client);
                writer.Write("what a wonderful day");

               // client.Close();
            }).Start();
        }
    }
}
