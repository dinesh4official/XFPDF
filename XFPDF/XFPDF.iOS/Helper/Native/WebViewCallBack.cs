using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;
using WebKit;

namespace XFPDF.iOS
{
    class WebViewCallBack : WKNavigationDelegate
    {
        private PDFToHtml PDFToHtml { get; set; }

        public WebViewCallBack(PDFToHtml _pDFToHtml)
        {
            PDFToHtml = _pDFToHtml;
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            try
            {
                int padding = 10;
                UIEdgeInsets pageMargins = new UIEdgeInsets(padding, padding, padding, padding);
                webView.ViewPrintFormatter.ContentInsets = pageMargins;
                UIPrintPageRenderer renderer = new UIPrintPageRenderer();
                renderer.AddPrintFormatter(webView.ViewPrintFormatter, 0);
                CGSize pageSize = new CGSize(PDFToHtml.PageWidth, PDFToHtml.PageHeight);
                CGRect printableRect = new CGRect(padding, padding, pageSize.Width - (padding * 2), pageSize.Height - (padding * 2));
                CGRect paperRect = new CGRect(0, 0, PDFToHtml.PageWidth, PDFToHtml.PageHeight);

                var nSString = new NSString("PaperRect");
                var printableRectstring = new NSString("PrintableRect");

                renderer.SetValueForKey(NSValue.FromObject(paperRect), nSString);
                renderer.SetValueForKey(NSValue.FromObject(printableRect), printableRectstring);

                NSData file = PrintToPDFWithRenderer(renderer, paperRect);
                File.WriteAllBytes(PDFToHtml.FilePath + ".pdf", file.ToArray());
                PDFToHtml.Status = PDFEnum.Completed;
            }
            catch
            {
                PDFToHtml.Status = PDFEnum.Failed;
            }
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            base.DidFailNavigation(webView, navigation, error);
            PDFToHtml.Status = PDFEnum.Failed;
        }

        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            base.DidFailProvisionalNavigation(webView, navigation, error);
            PDFToHtml.Status = PDFEnum.Failed;
        }

        private NSData PrintToPDFWithRenderer(UIPrintPageRenderer renderer, CGRect paperRect)
        {
            NSMutableData pdfData = new NSMutableData();
            try
            {
                UIGraphics.BeginPDFContext(pdfData, paperRect, null);
                renderer.PrepareForDrawingPages(new NSRange(0, renderer.NumberOfPages));
                for (int i = 0; i < renderer.NumberOfPages; i++)
                {
                    UIGraphics.BeginPDFPage();
                    renderer.DrawPage(i, paperRect);
                }
                UIGraphics.EndPDFContent();
            }
            catch
            {
                PDFToHtml.Status = PDFEnum.Failed;
            }

            return pdfData;
        }
    }
}