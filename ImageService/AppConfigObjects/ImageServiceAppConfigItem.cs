using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.AppConfigObjects
{
    public class ImageServiceAppConfigItem
    {
        private String outputFolder;
        public String OutputFolder { get { return this.outputFolder; } set { this.outputFolder = value; } }
        private String handlers;
        public String Handlers { get { return this.handlers; } set { this.handlers = value; } }
        private String sourceName;
        public String SourceName { get { return this.sourceName; } set { this.sourceName = value; } }
        private String logName;
        public String LogName { get { return this.logName; } set { this.logName = value; } }
        private int thumbnailSize;
        public int ThumbnailSize{ get { return this.thumbnailSize; } set { this.thumbnailSize = value; } }
        public ImageServiceAppConfigItem(String output, String handlers, String source,
            String log, int size)
        {
            this.outputFolder = output;
            this.sourceName = source;
            this.logName = log;
            this.handlers = handlers;
            this.thumbnailSize = size;
        }
        public string ToJSON()
        {
            JObject appConfigItem = new JObject();
            appConfigItem["output"] = this.outputFolder;
            appConfigItem["source"] = this.sourceName;
            appConfigItem["log"] = this.logName;
            appConfigItem["handlers"] = this.handlers;
            appConfigItem["size"] = this.thumbnailSize;
            return appConfigItem.ToString();
        }
        public static ImageServiceAppConfigItem FromJSON(string str)
        {
            ImageServiceAppConfigItem appconf = new ImageServiceAppConfigItem(null,null,null,null,0);
            JObject appconfItem = JObject.Parse(str);
            appconf.outputFolder = (string)appconfItem["output"];
            appconf.sourceName = (string)appconfItem["source"];
            appconf.logName = (string)appconfItem["log"];
            appconf.handlers = (string)appconfItem["handlers"];
            appconf.thumbnailSize = (int)appconfItem["size"];
            return appconf;
        }
    }
}
