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
    public class ImageServiceClientHandler : IClientHandler
    {

        private IImageController controller;
        private IManagerOfHandlers handler;
        private ILoggingService log;
        private List<LogMessage> logMessages;
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        private List<TcpClient> Clients;
        //private List<TcpClient> settingsClients;
        private static Mutex mut = new Mutex();
        private Dictionary<TcpClient, NetworkStream> myClients = new Dictionary<TcpClient, NetworkStream>();
        private Dictionary<TcpClient, BinaryWriter> writerDictionary = new Dictionary<TcpClient, BinaryWriter>();
        private Dictionary<TcpClient, BinaryReader> readerDictionary = new Dictionary<TcpClient, BinaryReader>();
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
        public void HandleClient(TcpClient client)
        {
            this.Clients.Add(client);
            //bool settingClient = true;
            if (!myClients.ContainsKey(client))
            {
                System.Diagnostics.Debugger.Launch();
                myClients.Add(client, client.GetStream());
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
                    bool hasValue = myClients.TryGetValue(client, out NetworkStream stream);
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
            //System.Diagnostics.Debugger.Launch();

            LogMessage l = new LogMessage(e.Message, e.Status);
            String logObj = l.ToJSON();
            this.WriteToClient(logObj);
        }

        private void WriteToClient(string message)
        {
          //  System.Diagnostics.Debugger.Launch();
         //   Task task = new Task(() =>
          //  {
                bool hasValue;
                BinaryWriter writer;
                Console.WriteLine(message);
                foreach(TcpClient c in this.Clients)
                {
                    hasValue = myClients.TryGetValue(c, out NetworkStream s);
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
                
           // });
           // task.Start();
            //task.Wait();
        }
    }
}


