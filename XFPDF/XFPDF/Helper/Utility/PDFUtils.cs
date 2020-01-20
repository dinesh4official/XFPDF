using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XFPDF
{
    public static class PDFUtils
    {
        private static string GetBaseUrl()
        {
            var fileHelper = DependencyService.Get<IFileHelper>();
            return fileHelper.ResourcesBaseUrl + "pdfjs/";
        }

        public static string PdfJsViewerUri => GetBaseUrl() + "web/viewer.html";
    }
}
