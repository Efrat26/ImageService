using Logs.Controller;
using Logs.Controller.Handlers;
using Infrastructure.Enums;
using Logs.ImageService.Logging;
using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Logs.ImageService.Logging.Modal;
using System.Threading;

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
            BinaryWriter writer;
            bool stop = false;
            new Task(() =>
            {
                while (!stop)
                {
                    stream = client.GetStream();
                    reader = new BinaryReader(stream);
                    writer = new BinaryWriter(stream);
                    int commandNum;
                    bool res;
                    String result = null;
                    // System.Diagnostics.Debugger.Launch();
                    string commandLine = reader.ReadString();
                    Console.WriteLine("Got command: {0}", commandLine);
                    Char c = commandLine[0];
                    this.log.Log("command recieved: " + c, MessageTypeEnum.INFO);
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
                    mut.WaitOne();

                    foreach (TcpClient cln in this.settingsClients)
                    {

                        if (res)
                        {
                            if (!result.Equals(ResultMessgeEnum.Success.ToString()))
                            {
                                writer.Write("\r\n" + result + "\r\n");
                                writer.Flush();
                            }
                            else
                            {
                                writer.Write(result);
                                writer.Flush();

                            }
                        }
                        else
                        {
                            if (!result.Equals(ResultMessgeEnum.Fail.ToString()))
                            {
                                writer.Write("\r\n" + result + "\r\n");
                                writer.Flush();
                            }
                            else
                            {
                                writer.Write(result);
                                writer.Flush();
                            }
                        }
                    }
                    mut.ReleaseMutex();



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
            NetworkStream stream;
            BinaryReader reader;
            BinaryWriter writer = null;
            LogMessage l = new LogMessage(e.Message, e.Status);
            String logObj = l.ToJSON();

            {
                mut.WaitOne();
                foreach (TcpClient client in logClients)
                {
                    //System.Diagnostics.Debugger.Launch();
                    stream = client.GetStream();
                    reader = new BinaryReader(stream);
                    writer = new BinaryWriter(stream);

                    writer.Write("\r\n" + logObj + "\r\n");


                }
                mut.ReleaseMutex();

            }


        }
    }
}
