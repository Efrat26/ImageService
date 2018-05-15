using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    
    //currently not in use
    class CloseCommand : ICommand
    {
        /// <summary>
        /// The image service modal
        /// </summary>
        private IImageServiceModal modal;
        /// <summary>
        /// Initializes a new instance of the <see cref="CloseCommand"/> class.
        /// </summary>
        /// <param name="m">The modal.</param>
        public CloseCommand(IImageServiceModal m)
        {
            modal = m;            // Storing the Modal
        }
        /// <summary>
        /// Executes the command - currently not in use.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="result">if set to <c>true</c> [result].</param>
        /// <returns></returns>
        public string Execute(string[] args, out bool result)
        {
            // The String Will Return the New Path if result = true, and will return the error message
            result = true;
            return null;
        }
        public string[] getArgs()
        {
            return null;
        }
    }
}
