using Infrastructure.Enums;
using Logs.Controller;
using Logs.ImageService.Logging;
using Logs.ImageService.Logging.Modal;
using Logs.Modal;
using Logs.Modal.Event;
using Logs.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ImageService.Server
{
    public sealed class ImageServer : IServer
    {
        /// <summary>
        /// The instance, server is singleton
        /// </summary>
        private static volatile ImageServer instance;
        private static object syncRoot = new Object();
        public static ImageServer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ImageServer(Communication.CommunicationDetails.IP
                                , Communication.CommunicationDetails.port);
                    }
                }

                return instance;
            }
        }
        #region Members        
        /// <summary>
        /// The controller - holds the object to threat the command (dictionary with commands)
        /// </summary>
        private IImageController controller;
        /// <summary>
        /// The logging service
        /// </summary>
        private ILoggingService logging;
        /// <summary>
        private int port;
        private String ip;
        private TcpListener listener;
        private IClientHandler ch;
        #endregion
        #region Properties 
        public int Port { get { return this.port; } set { this.port = value; } }
        public String IP { get { return this.ip; } set { this.ip = value; } }
        public ILoggingService Log { get { return this.logging;} set {this.logging = value; } }
        #endregion
        /// <summary>
        /// Occurs when a command recieved.
        /// </summary>
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        /// <summary>
        /// Occurs when got close directory command.
        /// </summary>
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        /// <summary>
        /// Occurs when a log message recieved.
        /// </summary>
        public event EventHandler<MessageRecievedEventArgs> LogMessageRecieved;
       
        
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="l">The logging service</param>
        public ImageServer(String ip, int p)
        {
            this.Port = p;
            this.IP = ip;
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        event EventHandler<CommandRecievedEventArgs> IServer.CommandRecieved
        {
            add
            {
              //  throw new NotImplementedException();
            }

            remove
            {
               // throw new NotImplementedException();
            }
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
        //starts a thread that listens to connections, when got a connection it 
        //gives it to the client handler
        public void Start()
        {
            List<TcpClient> clients = new List<TcpClient>();
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
                        if (!clients.Contains(client))
                        {
                            Console.WriteLine("Got new connection");
                            this.logging.Log("Got new connection", MessageTypeEnum.INFO);
                            //System.Diagnostics.Debugger.Launch();
                            // if (!client.Connected) { client.Connect(ep); }
                            Task t = new Task(() =>
                            {
                                ch.HandleClient(client);
                            }); t.Start();
                        }
                    }
                    catch (SocketException)
                    { break; }
                }
                this.logging.Log("Server stopped", MessageTypeEnum.INFO);
            });
            task.Start();
            this.logging.Log("after start listening", MessageTypeEnum.INFO);
        }
        /// <summary>
        /// Stops the listening.
        /// </summary>
        public void Stop() { listener.Stop(); }
        /// <summary>
        /// invokes the event when got a log message.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:Logs.ImageService.Logging.Modal.MessageRecievedEventArgs" /> instance containing the event data.</param>
        public void OnMessage(object sender, MessageRecievedEventArgs e)
        {
            this.LogMessageRecieved?.Invoke(this, e);
        }
        /// <summary>
        /// Sets the logger service.
        /// </summary>
        /// <param name="l">The logger service instance.</param>
        public void SetLoggerService(ILoggingService l)
        {
            this.logging = l;
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
    }
}
