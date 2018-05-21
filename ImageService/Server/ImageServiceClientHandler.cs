using Logs.Controller;
using Logs.Controller.Handlers;
using Infrastructure.Enums;
using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Logs.ImageService.Logging;
using Logs.ImageService.Logging.Modal;

namespace Logs.Server
{
    //an image service client handler
    public class ImageServiceClientHandler : IClientHandler
    {
        #region members
        /// <summary>
        /// The controller
        /// </summary>
        private IImageController controller;
        /// <summary>
        /// The handler
        /// </summary>
        private IManagerOfHandlers handler;
        /// <summary>
        /// The log service
        /// </summary>
        private ILoggingService log;
        /// <summary>
        /// The log messages
        /// </summary>
        private List<LogMessage> logMessages;
        /// <summary>
        /// Occurs when command recieved.
        /// </summary>
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        /// <summary>
        /// Occurs when got a close command.
        /// </summary>
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        /// <summary>
        /// list of clients
        /// </summary>
        private List<TcpClient> Clients;
        /// <summary>
        /// mutex for the writing command
        /// </summary>
        private static Mutex mut = new Mutex();
        /// <summary>
        /// a dictionary for client and the stream (i had to save it because sometimes i had problems getting
        /// the stream from the client)
        /// </summary>
        private Dictionary<TcpClient, NetworkStream> ClientStreamDictionary = new Dictionary<TcpClient, NetworkStream>();
        /// <summary>
        /// client-writer dictionary
        /// </summary>
        private Dictionary<TcpClient, BinaryWriter> writerDictionary = new Dictionary<TcpClient, BinaryWriter>();
        /// <summary>
        /// client-reader dictionary
        /// </summary>
        private Dictionary<TcpClient, BinaryReader> readerDictionary = new Dictionary<TcpClient, BinaryReader>();
        #endregion members        
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageServiceClientHandler"/> class.
        /// </summary>
        /// <param name="l">The log service.</param>
        /// <param name="c">The controller.</param>
        public ImageServiceClientHandler(ILoggingService l, IImageController c)
        {
            this.controller = c;
            this.log = l;
            this.handler = new ImageServerManagerOfHandlers(l, c);
            this.CloseCommand += this.handler.OnCloseDirectory;
            this.CommandRecieved += this.handler.OnCommandRecieved;
            this.logMessages = new List<LogMessage>();
            this.Clients = new List<TcpClient>();
            //this.settingsClients = new List<TcpClient>();
        }
        /// <summary>
        /// Handles the client: adds it to the list of clients, sends it the log messages so far, 
        /// reads command, send it to execute and sends it to writing the results.
        /// </summary>
        /// <param name="client">The client.</param>
        public void HandleClient(TcpClient client)
        {
            this.Clients.Add(client);
            //bool settingClient = true;
            if (!ClientStreamDictionary.ContainsKey(client))
            {
                //System.Diagnostics.Debugger.Launch();
                ClientStreamDictionary.Add(client, client.GetStream());
                for (int i = 0; i < this.logMessages.Count; ++i)
                {
                    this.WriteToClient(this.logMessages[i].ToJSON());
                }
            }
            BinaryReader reader;
            bool stop = false;
            new Task(() =>
            {
                while (!stop)
                {


                    // writer = new BinaryWriter(stream);
                    int commandNum;
                    bool res;
                    String result = null;

                    // System.Diagnostics.Debugger.Launch();
                    bool hasValue = ClientStreamDictionary.TryGetValue(client, out NetworkStream stream);
                    if (hasValue)
                    {
                        hasValue = readerDictionary.TryGetValue(client, out BinaryReader r);
                        if (!hasValue)
                        {
                            reader = new BinaryReader(stream);
                            readerDictionary.Add(client, reader);
                        }
                        else
                        {
                            reader = r;
                        }
                        string commandLine = reader.ReadString();

                        Task.Delay(1000);
                        // Task.Delay(4000);
                        Console.WriteLine("Got command: {0}", commandLine);
                        //System.Diagnostics.Debugger.Launch();
                        Char c = commandLine[0];
                        // this.log.Log("command recieved: " + c, MessageTypeEnum.INFO);
                        try
                        {
                            commandNum = Int32.Parse(c.ToString());
                            if (commandNum == (int)CommandEnum.CloseHandler)
                            {
                                //System.Diagnostics.Debugger.Launch();
                                this.log.Log("close specific handler command recieved", MessageTypeEnum.INFO);
                                String handlerJObject = commandLine.Substring(1, commandLine.Length - 1);
                                this.log.Log("recieved: " + handlerJObject, MessageTypeEnum.INFO);
                                HandlerToClose h = HandlerToClose.FromJSON(handlerJObject);
                                this.CloseCommand?.Invoke(this, new DirectoryCloseEventArgs(h.Path, null));
                                //System.Diagnostics.Debugger.Launch();
                                res = true;
                                result = ResultMessgeEnum.Success.ToString();

                            }
                            else if (commandNum == (int)CommandEnum.LogCommand)
                            {
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
                            result = ResultMessgeEnum.Fail.ToString();
                        }



                        this.WriteToClient(result);
                        //this.WriteToClient(result, 1);
                    }
                }
            }).Start();
        }
        /// <summary>
        /// Sets a controller to execute the commands.
        /// </summary>
        /// <param name="c">the controller</param>
        public void SetController(IImageController c)
        {
            if (this.controller == null)
            {
                this.controller = c;
            }
        }
        // the command the server can recieve is close command        
        /// <summary>
        /// method to handle when command recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:Logs.Modal.Event.CommandRecievedEventArgs" /> instance containing the event data.</param>
        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseCommand, null, null));
        }
        //close specific directory        
        /// <summary>
        /// Called when directory closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:Logs.Modal.Event.DirectoryCloseEventArgs" /> instance containing the event data.</param>
        public void OnCloseDirectory(object sender, DirectoryCloseEventArgs e)
        {
            this.CloseCommand(this, e);
        }
        /// <summary>
        /// Called when a log message recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:Logs.ImageService.Logging.Modal.MessageRecievedEventArgs" /> instance containing the event data.</param>
        public void OnLogMessageRecieved(object sender, MessageRecievedEventArgs e)
        {
            this.logMessages.Add(new LogMessage(e.Message, e.Status));
            //write to the client
            //System.Diagnostics.Debugger.Launch();

            LogMessage l = new LogMessage(e.Message, e.Status);
            String logObj = l.ToJSON();
            this.WriteToClient(logObj);
        }
        /// <summary>
        /// Writes to client a message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteToClient(string message)
        {
            bool hasValue;
            BinaryWriter writer;
            Console.WriteLine(message);
            foreach (TcpClient c in this.Clients)
            {
                hasValue = ClientStreamDictionary.TryGetValue(c, out NetworkStream s);
                if (hasValue && s != null)
                {
                    hasValue = writerDictionary.TryGetValue(c, out BinaryWriter w);
                    if (!hasValue)
                    {

                        writer = new BinaryWriter(c.GetStream());
                        writerDictionary.Add(c, writer);
                    }
                    else
                    {
                        writer = w;
                    }
                    mut.WaitOne();
                    writer.Write(message);
                    mut.ReleaseMutex();
                }
            }
        }
    }
}