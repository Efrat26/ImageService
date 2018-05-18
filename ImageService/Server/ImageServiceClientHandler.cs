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
        private List<TcpClient> logClients;
        private List<TcpClient> settingsClients;
        private static Mutex mut = new Mutex();
        public ImageServiceClientHandler(ILoggingService l, IImageController c)
        {
            this.controller = c;
            this.log = l;
            this.handler = new ImageServerManagerOfHandlers(l, c);
            this.CloseCommand += this.handler.OnCloseDirectory;
            this.CommandRecieved += this.handler.OnCommandRecieved;
            this.logMessages = new List<LogMessage>();
            this.logClients = new List<TcpClient>();
            this.settingsClients = new List<TcpClient>();
        }
        public void HandleClient(TcpClient client)
        {
            NetworkStream stream;
            BinaryReader reader;
            bool stop = false;
            new Task(() =>
            {
                while (!stop)
                {
                    stream = client.GetStream();
                    reader = new BinaryReader(stream);
                   // writer = new BinaryWriter(stream);
                    int commandNum;
                    bool res;
                    String result = null;
                    // System.Diagnostics.Debugger.Launch();
                    string commandLine = reader.ReadString();
                    Console.WriteLine("Got command: {0}", commandLine);
                    Char c = commandLine[0];
                    // this.log.Log("command recieved: " + c, MessageTypeEnum.INFO);
                    try
                    {
                        commandNum = Int32.Parse(c.ToString());
                        if (commandNum == (int)CommandEnum.GetConfigCommand)
                        {
                            if (!this.settingsClients.Contains(client))
                            {
                                settingsClients.Add(client);
                            }
                            if (this.logClients.Contains(client))
                            {
                                this.logClients.Remove(client);
                            }
                        }
                        if (commandNum == (int)CommandEnum.CloseHandler)
                        {
                            //System.Diagnostics.Debugger.Launch();
                            this.log.Log("close specific handler command recieved", MessageTypeEnum.INFO);

                            String handlerJObject = commandLine.Substring(1, commandLine.Length - 1); ;

                            this.log.Log("recieved: " + handlerJObject, MessageTypeEnum.INFO);
                            HandlerToClose h = HandlerToClose.FromJSON(handlerJObject);
                            this.CloseCommand?.Invoke(this, new DirectoryCloseEventArgs(h.Path, null));
                            //System.Diagnostics.Debugger.Launch();
                            res = true;
                            result = ResultMessgeEnum.Success.ToString();
                        }
                        else if (commandNum == (int)CommandEnum.LogCommand)
                        {
                            // System.Diagnostics.Debugger.Launch();
                            if (!this.logClients.Contains(client))
                            {
                                logClients.Add(client);
                            }
                            if (this.settingsClients.Contains(client))
                            {
                                this.settingsClients.Remove(client);
                            }
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
                    Task.Delay(2000);
                    this.WriteToClient(result, 1);

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
            this.WriteToClient(logObj, 2);
        }

        private void WriteToClient(string message, int clientType)
        {
            Task task = new Task(() =>
            {
                Console.WriteLine(message);
               // this.log.Log("in task, writing message: "+message, MessageTypeEnum.INFO);
                NetworkStream stream;
                BinaryWriter writer;
                //setting clients

                if (clientType == 1)
                {
                    
                    mut.WaitOne();
                    //this.log.Log("sending message to seetings client," + message, MessageTypeEnum.INFO);
                    foreach (TcpClient client in this.settingsClients)
                    {
                        
                            System.Diagnostics.Debugger.Launch();
                            stream = client.GetStream();
                        try
                        {
                            writer = new BinaryWriter(stream);
                            writer.Write(message);
                           // writer.Flush();
                            //writer.Close();
                            //Task.Delay(1000);
                        }
                        catch (Exception e)
                        {
                            this.log.Log("trying to send message to settings client," +
                                " got exception\n message is: " + message + " exception is: " + e.ToString()
                                +" stream is: " + stream.ToString(),
                                MessageTypeEnum.FAIL);
                        }
                    }
                    mut.ReleaseMutex();
                }
                else
                {
                    
                    mut.WaitOne();
                    //this.log.Log("sending message to logs client," + message, MessageTypeEnum.INFO);
                    foreach (TcpClient client in this.logClients)
                    {
                       
                            //System.Diagnostics.Debugger.Launch();
                            stream = client.GetStream();
                        try
                        {
                            writer = new BinaryWriter(stream);
                            writer.Write(message);
                            //writer.Flush();
                            //writer.Close();
                            // Task.Delay(1000);
                        }
                        catch (Exception e)
                        {
                            this.log.Log("trying to send message to log client," +
                                " got exception\n message is: " + message + " exception is: " + e.ToString()
                                 + " stream is: " + stream.ToString(),
                                MessageTypeEnum.FAIL);
                        }
                    }
                    mut.ReleaseMutex();
                }
            });
            task.Start();
         //  task.Wait();
        }
    }
}
