using ImageService.Controller;
using ImageService.Controller.Handlers;
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
using ImageService.ImageService.Logging.Modal;

namespace ImageService.Server
{
    public class ImageServiceClientHandler : IClientHandler
    {
        private NetworkStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private IImageController controller;
        private IManagerOfHandlers handler;
        private ILoggingService log;
        private List<LogMessage> logMessages;
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        private List<TcpClient> clients;

        public ImageServiceClientHandler(ILoggingService l, IImageController c)
        {
            this.controller = c;
            this.log = l;
            this.handler = new ImageServerManagerOfHandlers(l, c);
            this.CloseCommand += this.handler.OnCloseDirectory;
            this.CommandRecieved += this.handler.OnCommandRecieved;
            this.logMessages = new List<LogMessage>();
            this.clients = new List<TcpClient>();
        }
        public void HandleClient(TcpClient client)
        {
            clients.Add(client);
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
            bool stop = false;
            new Task(() =>
            {
                while (!stop)
                {
                    int commandNum;
                    bool res;
                    String result = null;
                    // System.Diagnostics.Debugger.Launch();
                    string commandLine = reader.ReadString();
                    Console.WriteLine("Got command: {0}", commandLine);
                    this.log.Log("command recieved: " + commandLine, MessageTypeEnum.INFO);
                    try
                    {
                        commandNum = Int32.Parse(commandLine);
                        if (commandNum == (int)CommandEnum.CloseHandler)
                        {
                            // System.Diagnostics.Debugger.Launch();
                            this.log.Log("close specific handler command recieved", MessageTypeEnum.INFO);
                            string handlerJObject = reader.ReadString();
                            this.log.Log("recieved: " + handlerJObject, MessageTypeEnum.INFO);
                            HandlerToClose h = HandlerToClose.FromJSON(handlerJObject);
                            this.CloseCommand?.Invoke(this, new DirectoryCloseEventArgs(h.Path, null));
                            res = true;
                            result = ResultMessgeEnum.Success.ToString();

                        }
                        else
                        {
                            this.log.Log("command number " + commandNum.ToString() + " recieved", MessageTypeEnum.INFO);
                            result = this.controller.ExecuteCommand(Int32.Parse(commandLine), null, out res);
                        }
                    }
                    catch (Exception)
                    {
                        res = false;
                        result = null;
                    }


                    if (res)
                    {
                        writer.Write(result);
                    }
                    else
                    {
                        writer.Write("failed");
                    }
                }
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
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, null));
        }
        //close specific directory
        public void OnCloseDirectory(object sender, DirectoryCloseEventArgs e)
        {
            this.CloseCommand(this, e);
        }

        public void OnLogMessageRecieved(object sender, MessageRecievedEventArgs e)
        {
            this.logMessages.Add(new LogMessage(e.Message, e.Status));
            //write to the client
        }
    }
}
