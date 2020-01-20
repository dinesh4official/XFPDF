using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using XFPDF.iOS;

[assembly: Dependency(typeof(FileHelper))]
namespace XFPDF.iOS
{
    public class FileHelper : IFileHelper
    {
        public string DocumentFilePath => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public string ResourcesBaseUrl
        {
            get
            {
                string path = NSBundle.MainBundle.BundlePath;
                if (!path.EndsWith("/")) path += "/";
                return path;
            }
        }
    }
}