using ImageService.Controller;
using ImageService.Modal.Event;
using System;
using System.Net.Sockets;

namespace ImageService.Server
{ 
    internal interface IClientHandler
    {
        void HandleClient(TcpClient c);
        void SetController(IImageController c);
        void OnCommandRecieved(object sender, CommandRecievedEventArgs e);
        void OnCloseDirectory(object sender, DirectoryCloseEventArgs e);
        event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        event EventHandler<DirectoryCloseEventArgs> CloseCommand;
    }
}
