using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    class ClientHandler : IClientHandler

    {
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        public void HandleClient(TcpClient client)
        {
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            new Task(() =>
            {
                System.Diagnostics.Debugger.Launch();
                string commandLine = reader.ReadLine();
                Console.WriteLine("Got command: {0}", commandLine);
                //string result = ExecuteCommand(commandLine, client);
                writer.Write("what a wonderful day");

                client.Close();
            }).Start();
        }

    }
}
