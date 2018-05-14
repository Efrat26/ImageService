using ImageService.Controller;
using ImageService.ImageService.Infrastructure.Enums;
using ImageService.ImageService.Logging;
using ImageService.Modal.Event;
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
        private IImageController controller;
        private IHandlerOfHandler handler;
        private ILoggingService log;

        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;

        public ImageServiceClientHandler(ILoggingService l, IImageController c)
        {
            this.controller = c;
            this.log = l;
            this.handler = new ImageServerHandlerOfHandler(l, c);
        }
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
                
                String result = this.controller.ExecuteCommand(Int32.Parse(commandLine), null, out bool res);
                if (res)
                {
                    writer.Write(result);
                }
                else
                {
                    writer.Write("failed");
                }
                
                //string result = ExecuteCommand(commandLine, client);
                //writer.Write("what a wonderful day");

               // client.Close();
            }).Start();
        }

        public void SetController(IImageController c)
        {
            if (this.controller == null)
            {
                this.controller = c;
            }
        }
        // the command the server can recieve is close command
        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null,null));
        }
        public void OnCommandRecieved(object sender, DirectoryCloseEventArgs e)
        {

        }

        public void OnCloseDirectory(object sender, DirectoryCloseEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
