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
using ImageService.ImageService.Infrastructure.Enums;
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
        /// a list with handlers
        /// </summary>
        private List<IDirectoryHandler> handler;

        private List<TcpClient> clients;
        #endregion
        #region Properties     
        /// <summary>
        /// Occurs when a command recieved.
        /// </summary>
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
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
            this.ch = new ImageServiceClientHandler();
            this.port = p;
            this.IP = ip;
            //thr logger of the service
            this.logging = l;
            //create image model
            this.controller = new ImageController(new ImageServiceModal(l));
            //initialize handlers list
            this.handler = new List<IDirectoryHandler>();
            //create the handler and sign the onClose method to the event
            string folderToListen = ConfigurationManager.AppSettings.Get("Handler");
            string[] folders = folderToListen.Split(';');
            //create handlers for each folder:
            foreach (string folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    this.CreateHandlerForFolder(folder);
                }
            }
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
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseCommand,null,null));
            this.logging.Log("after closing handlers", MessageTypeEnum.INFO);
            //remove the methods that signed to the events
            foreach (IDirectoryHandler handler in this.handler)
            {
                this.CommandRecieved -= handler.OnCommand;
                handler.DirectoryClose -= this.OnClose;
            }
            
        }
        /// <summary>
        /// Creates the handlers for folders defined in the app config.
        /// </summary>
        /// <param name="path">The path.</param>
        public void CreateHandlerForFolder(string path)
        {
           
            //create the handler and sign the onClose method to the event
            this.handler.Add( new DirectoyHandler(path, this.controller, this.logging));
            this.handler.Last().DirectoryClose += this.OnClose;
            //register the handler to the event of command recieved 
            this.CommandRecieved += this.handler.Last().OnCommand;
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
            { while (!stop) {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Console.WriteLine("Got new connection");
                        this.logging.Log("Got new connection", MessageTypeEnum.INFO);
                        //System.Diagnostics.Debugger.Launch();
                       // if (!client.Connected) { client.Connect(ep); }
                        ch.HandleClient(client);
                           
                    } catch (SocketException)
                    { break; }
                }
                this.logging.Log("Server stopped",MessageTypeEnum.INFO); });
            task.Start();
            this.logging.Log("after start listening", MessageTypeEnum.INFO);
        }

        public void Stop() { listener.Stop(); }
    }
}