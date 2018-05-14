﻿using ImageService.Controller;
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

        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;

        public ImageServiceClientHandler(ILoggingService l, IImageController c)
        {
            this.controller = c;
            this.log = l;
            this.handler = new ImageServerManagerOfHandlers(l, c);
            this.CloseCommand += this.handler.OnClose;
            this.CommandRecieved += this.handler.OnCommandRecieved;
        }
        public void HandleClient(TcpClient client)
        {
            stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
            new Task(() =>
            {
                int commandNum;
                bool res;
                String result = null;
                System.Diagnostics.Debugger.Launch();
                string commandLine = reader.ReadString();
                Console.WriteLine("Got command: {0}", commandLine);
                try
                {
                    commandNum = Int32.Parse(commandLine);
                    if (commandNum == (int)CommandEnum.CloseHandler)
                    {
                        this.log.Log("close specific handler command recieved", MessageTypeEnum.INFO);
                        HandlerToClose h = HandlerToClose.FromJSON(commandLine);
                        this.CloseCommand?.Invoke(this, new DirectoryCloseEventArgs(h.Path, null));
                        res = true;

                    }
                    else
                    {
                        this.log.Log("command number " + commandNum.ToString() +" recieved", MessageTypeEnum.INFO);
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
    }
}
