using ImageService.Controller;
using System.Net.Sockets;

namespace ImageService.Server
{ 
    internal interface IClientHandler
    {
        void HandleClient(TcpClient c);
        void SetController(IImageController c);
    }
}
