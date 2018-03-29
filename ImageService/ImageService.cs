﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ImageService.Server;
using ImageService.ImageService.Logging;
using ImageService.ImageService.Logging.Modal;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using ImageService.Modal.Event;

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
        public ImageService(string[] args)
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
            //this.m_dirWatcher = new FileSystemWatcher();
        }
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
        protected override void OnStart(string[] args)
        {
            // System.Diagnostics.Debugger.Launch();
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
            this.log.MessageRecieved += OnMessage;
            this.server = new ImageServer(this.log);
            this.ServiceClose += this.server.OnClose;
            this.log.Log("Hello frm service", MessageTypeEnum.INFO);
        }
        public void OnMessage(object sender, MessageRecievedEventArgs e)
        {
            Console.WriteLine(e.Message);
            string msg = e.Message + " " + e.Status;
            this.eventLog1.WriteEntry(msg); 
            // System.Diagnostics.Debugger.Launch();
        }
        protected override void OnStop()
        {
            this.OnStop(null, null);
        }
        protected void OnStop(object sender, DirectoryCloseEventArgs e)
        {
            
            ServiceClose?.Invoke(this, null);
            eventLog1.WriteEntry("In onStop.");
        }
        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }
    }
}
