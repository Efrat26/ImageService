using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.ImageService.Infrastructure.Enums;
using ImageService.ImageService.Logging;
using ImageService.ImageService.Logging.Modal;
using ImageService.Modal;
using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ImageService.Commands;

namespace ImageService.Server
{
    class ImageServer : IServer
    {
        #region Members        
        /// <summary>
        /// The controller - holds the object to threat the command (dictionary with commands)
        /// </summary>
        private IImageController controller;
        /// <summary>
        /// The logging service
        /// </summary>
        private ImageService.Logging.ILoggingService logging;
        /// <summary>
        private List<TcpClient> clients;
        #endregion
        #region Properties     
        /// <summary>
        /// Occurs when a command recieved.
        /// </summary>
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        public event EventHandler<MessageRecievedEventArgs> LogMessageRecieved;
        private int port;
        private String IP;
        private TcpListener listener;
        private IClientHandler ch;
        #endregion
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="l">The logging service</param>
        public ImageServer(ILoggingService l, String ip, int p)
        {
            this.port = p;
            this.IP = ip;
            //thr logger of the service
            this.logging = l;
            //create image model
            this.controller = new ImageController(new ImageServiceModal(l));
            //set the client handler
            this.ch = new ImageServiceClientHandler(logging, controller);
            //this.ch.SetController(this.controller);
            this.CommandRecieved += this.ch.OnCommandRecieved;
            this.LogMessageRecieved += this.ch.OnLogMessageRecieved;
            //start listening
            //System.Diagnostics.Debugger.Launch();
            this.Start();
            this.logging.Log("after start command ctor", MessageTypeEnum.INFO);
            //  this.logging.Log("Hello frm server", ImageService.Logging.Modal.MessageTypeEnum.INFO);
        }
        /// <summary>
        /// Raises the Close event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DirectoryCloseEventArgs" /> instance containing the event data.</param>
        public void OnClose(object sender, DirectoryCloseEventArgs e)
        {
            this.logging.Log("in on close of server", MessageTypeEnum.INFO);
            // 
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, null));
            this.logging.Log("after closing handlers", MessageTypeEnum.INFO);
        }

        public void Start()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(this.IP), this.port);
            listener = new TcpListener(ep);
            listener.Start();
            this.logging.Log("before start listening", MessageTypeEnum.INFO);
            Console.WriteLine("Waiting for connections...");
            Boolean stop = false;
            Task task = new Task(() =>
            {
                while (!stop)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Console.WriteLine("Got new connection");
                        this.logging.Log("Got new connection", MessageTypeEnum.INFO);
                        //System.Diagnostics.Debugger.Launch();
                        // if (!client.Connected) { client.Connect(ep); }
                        ch.HandleClient(client);

                    }
                    catch (SocketException)
                    { break; }
                }
                this.logging.Log("Server stopped", MessageTypeEnum.INFO);
            });
            task.Start();
            this.logging.Log("after start listening", MessageTypeEnum.INFO);
        }

        public void Stop() { listener.Stop(); }
        public void OnMessage(object sender, MessageRecievedEventArgs e)
        {
            this.LogMessageRecieved?.Invoke(this, e);
        }
    }
}