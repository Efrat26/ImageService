using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    /// <summary>
    /// interface for commands
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the specified command (by command ID).
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <param name="result">if set to true if the execution was successful
        /// and false otherwise.</param>
        /// <returns>string that contains the error message or a success message</returns>
        string Execute(string[] args, out bool result);          // The Function That will Execute The 
    }
}
