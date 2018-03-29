using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ImageService.Infrastructure.Enums
{
    /// <summary>
    /// enum for the result message (mainly for the success message that returns from the 
    /// execute command function because otherwise the error message is returned)
    /// </summary>
    public enum ResultMessgeEnum : int
    {
        Success,
        Fail
    }
}
