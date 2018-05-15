using ImageService.ImageService.Logging.Modal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ImageService.Logging
{
    public class LogMessage
    {
        private String message;
        private MessageTypeEnum type;
        public String Message
        {
            get { return this.message; }
            set { this.message = value; }
        }
        public MessageTypeEnum Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        public LogMessage(String m, MessageTypeEnum t)
        {
            this.message = m;
            this.type = t;
        }
        public String ToJSON()
        {
            JObject logMessageItm = new JObject();
            logMessageItm["message"] = this.message;
            logMessageItm["type"] = this.type.ToString();
            return logMessageItm.ToString();
        }
        
    }
}
