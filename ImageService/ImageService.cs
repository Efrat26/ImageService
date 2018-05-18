using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Logs.Server;
using Logs.ImageService.Logging;
using Logs.ImageService.Logging.Modal;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using Logs.Modal.Event;
using ImageService.Server;

namespace ImageService1
{
    public partial class ImageService : ServiceBase
    {
        private int eventId = 1;
        private ILoggingService log;
        private IServer server;
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
        public event EventHandler<DirectoryCloseEventArgs> ServiceClose;
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
        /// <summary>
        /// Initializes a new instance of the class.
        /// sets the log and the source names
        /// </summary>
        public ImageService()
        {
            InitializeComponent();
            string eventSourceName = ConfigurationManager.AppSettings["SourceName"];
            //"MySource";
            string logName = ConfigurationManager.AppSettings["LogName"];
            //"MyNewLog";
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }
        /// <summary>
        /// Called when [timer]. from the tutorial
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.  
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds  
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }
        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the 
        /// service by the Service Control Manager (SCM) or when the operating system starts
        /// (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            //create logger and server
            this.log = new LoggingService();
            //System.Diagnostics.Debugger.Launch();
            this.log.MessageRecieved += OnMessage;
            this.server = new ImageServer("127.0.0.1", 8000);
            this.server.SetLoggerService(log);
            this.ServiceClose += this.server.OnClose;
            this.log.MessageRecieved += this.server.OnMessage;
            this.log.Log("Hello from service", MessageTypeEnum.INFO);
        }
        /// <summary>
        /// Called when a message was recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MessageRecievedEventArgs"/>
        /// instance containing the event data.</param>
        public void OnMessage(object sender, MessageRecievedEventArgs e)
        {
            Console.WriteLine(e.Message);
            string msg = e.Message + " " + e.Status;
            this.eventLog1.WriteEntry(msg); 
        }
        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service 
        /// by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// calles the OnStop that raises the event to notify the server that it's about to close
        /// </summary>
        protected override void OnStop()
        {
            this.OnStop(null, null);
            this.eventLog1.WriteEntry("service is closed!");
        }
        /// <summary>
        /// Called when stopping the server. raises the event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DirectoryCloseEventArgs"/>
        /// instance containing the event data.</param>
        protected void OnStop(object sender, DirectoryCloseEventArgs e)
        {
            
            //ServiceClose?.Invoke(this, null);
            eventLog1.WriteEntry("In onStop.");
        }
        /// <summary>
        /// When implemented in a derived class,
        /// <see cref="M:System.ServiceProcess.ServiceBase.OnContinue" /> runs when a Continue command
        /// is sent to the service by the Service Control Manager (SCM). Specifies actions
        /// to take when a service resumes normal functioning after being paused.
        /// </summary>
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }
    }
}
