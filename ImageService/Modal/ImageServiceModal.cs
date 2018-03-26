﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members
        private string m_OutputFolder;            // The Output Folder
        private int m_thumbnailSize;              // The Size Of The Thumbnail Size
        public ImageServiceModal()
        {
            this.m_OutputFolder = ConfigurationManager.AppSettings.Get("OutputDir");
            this.m_thumbnailSize = Int32.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));
        }
        public string AddFile(string path, out bool result)
        {
            result = true;
            return null;
        }
        public string MoveFile(string path, out bool result)
        {
            result = true;
            return null;
        }

        #endregion
    }
}
