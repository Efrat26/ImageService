using Logs.Controller;
using Logs.ImageService.Logging.Modal;
using Logs.Modal.Event;
using Logs.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    class ClientHandlerForImages : IClientHandler
    {
        private String start;
        private String end;
        private List<String> handlers;
        private List<TcpClient> clients;
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        public ClientHandlerForImages()
        {
            this.start = "BEGIN";
            this.end = "END";
            this.clients = new List<TcpClient>();
            this.handlers = new List<string>();
            string folderToListen = ConfigurationManager.AppSettings.Get("Handler");
            string[] folders = folderToListen.Split(';');
            //create handlers for each folder:
            foreach (string folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    this.handlers.Add(folder);
                }
            }
        }
        public void HandleClient(TcpClient c)
        {
            if (clients != null && !this.clients.Contains(c))
            {
                this.clients.Add(c);
            }
            BinaryReader reader;
            NetworkStream stream;
            String numOfBytesAsString;
            int numOfBytes;
            Byte byteRead;
            bool stop = false;
            new Task(() =>
            {
                stream = c.GetStream();
                reader = new BinaryReader(stream);
                while (!stop)
                {
                    byte[] img = null;
                    //int counter = 0;
                    try
                    {
                        numOfBytesAsString = reader.ReadString();
                        if (numOfBytesAsString != null && numOfBytesAsString != "")
                        {
                            System.Diagnostics.Debugger.Launch();
                            numOfBytes = Int32.Parse(numOfBytesAsString);
                            img = reader.ReadBytes(numOfBytes);
                           // System.Diagnostics.Debugger.Launch();

                            Image x = (Bitmap)((new ImageConverter()).ConvertFrom(img));
                        }
                    }
                    catch (Exception e)

                    {

                    }

                }
            }).Start();

        }

        public void OnCloseDirectory(object sender, DirectoryCloseEventArgs e)
        {
            
        }

        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            
        }

        public void OnLogMessageRecieved(object sender, MessageRecievedEventArgs e)
        {
            
        }

        public void SetController(IImageController c)
        {
           
        }
        public void MoveFile(Image i)
        {
            if(this.handlers != null && this.handlers.Count > 0)
            {
                System.IO.Directory.Move(Directory.GetCurrentDirectory(),handlers[0]);
            }
        }
    }
}
