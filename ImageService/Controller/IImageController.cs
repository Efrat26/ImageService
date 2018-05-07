using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller
{
    /// <summary>
    /// interface for an image controller object
    /// </summary>
    public interface IImageController
    {
        /// <summary>
        /// Executes the command specified by ID.
        /// </summary>
        /// <param name="commandID">The command identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="result">if set to <c>true</c> [result].</param>
        /// <returns>a string contains error message (if error occured) or a
        /// success message if it done without errors</returns>
        string ExecuteCommand(int commandID, string[] args, out bool result);
        /// <summary>
        /// Called when service is closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void OnCloseOfService(object sender, EventArgs e);
    }
}
