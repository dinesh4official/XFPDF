using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;
using WebKit;
using Xamarin.Forms;
using XFPDF.iOS;

[assembly: Dependency(typeof(PDFConverter))]
namespace XFPDF.iOS
{
    public class PDFConverter : IPDFConverter
    {
        public void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml)
        {
            try
            {
                WKWebView webView = new WKWebView(new CGRect(0, 0, (int)_PDFToHtml.PageWidth, (int)_PDFToHtml.PageHeight), new WKWebViewConfiguration());
                webView.UserInteractionEnabled = false;
                webView.BackgroundColor = UIColor.White;
                webView.NavigationDelegate = new WebViewCallBack(_PDFToHtml);
                webView.LoadHtmlString(_PDFToHtml.HTMLString, null);
            }
            catch
            {
                _PDFToHtml.Status = PDFEnum.Failed;
            }
        }
    }
}