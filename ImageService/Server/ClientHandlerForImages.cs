using Logs.Controller;
using Logs.ImageService.Logging.Modal;
using Logs.Modal.Event;
using Logs.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
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

                    //int counter = 0;
                    try
                    {
                        string b = reader.ReadString();

                        if (b.Equals("") || b == null)
                        {
                            continue;
                        }
                        else if (b.StartsWith("begin"))
                        {
                            numOfBytes = Int32.Parse(b.Substring(5, b.Length - 5));

                            sbyte[] img = new sbyte[numOfBytes];
                            for (int i = 0; i < numOfBytes; ++i)
                            {
                                img[i] = reader.ReadSByte();
                            }
                            string name = reader.ReadString();
                            while (name.Equals("") || name == null) { name = reader.ReadString(); }
                            string temp;
                            if (name.StartsWith("end"))
                            {
                                temp = name.Substring(3, name.Length - 3);
                                if (this.handlers != null && this.handlers.Count > 0)
                                {
                                    using (Image image = Image.FromStream(new MemoryStream((byte[])(Array)img)))
                                    {
                                        String path = handlers[0]+ "\\\\"+temp;
                                        image.Save(path, ImageFormat.Jpeg);  // Or Png
                                        //System.Diagnostics.Debugger.Launch();
                                    }
                                }
                                
                            }
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
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
