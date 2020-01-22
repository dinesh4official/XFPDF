# XFPDF
This repository demonstrates you to generate the PDF file from HTML string in Xamarin Forms without any third party package.

In Xamarin Forms, there is no default support either to generate the PDF or to view the PDF file. But you can achieve these requirements with native support through renderers for both Android and iOS platform.

The demo application in the repository is divided into three segments.

1.	Create HTML from URL.
2.	Convert the HTML string to PDF file.
3.	View the PDF file. 

# Create HTML from URL
                        
You can easily get the HTML string from an URL either through HTTPClient or WebClient class. Please refer the below code snippet.

```
  var client = new HttpClient();
  htmlstring = await client.GetStringAsync(url);
```
```
  using (WebClient client = new WebClient())
  {
     htmlstring = client.DownloadString(url);
  }

```

# Convert the HTML string to PDF file

In recent days, I tried to convert the HTML string to PDF file in Xamarin Forms. But, unfortunately, there is no free library or default support in Xamarin platform. With the help of [WebView](https://docs.microsoft.com/en-us/dotnet/api/xamarin.forms.webview) and Xamarin Forums, I got ideas from colleagues and finally achieved the requirement through renderers. 

**For Android**

With the support of Native WebView and WebViewClient, you can create your own print document adapter where you can customize your PDF file as required. Instead of printing the PDF file using device printer, create your own callbacks to write into the file and save the file in custom location. 

```
   public class PdfLayoutResultCallback : PrintDocumentAdapter.LayoutResultCallback
   {
        public PrintDocumentAdapter Adapter { get; set; }

        public PDFToHtml PDFToHtml { get; set; }

        public PdfLayoutResultCallback(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public PdfLayoutResultCallback() : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (!(Handle != IntPtr.Zero))
            {
                unsafe
                {
                    JniObjectReference val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }

        }

        public override void OnLayoutFinished(PrintDocumentInfo info, bool changed)
        {
            try
            {
                var file = new Java.IO.File(PDFToHtml.FilePath);
                var fileDescriptor = ParcelFileDescriptor.Open(file, ParcelFileMode.ReadWrite);
                var writeResultCallback = new PdfWriteResultCallback(PDFToHtml);
                Adapter.OnWrite(new PageRange[] { PageRange.AllPages }, fileDescriptor, new CancellationSignal(), writeResultCallback);
            }
            catch
            {
                PDFToHtml.Status = PDFEnum.Failed;
            }

            base.OnLayoutFinished(info, changed);
        }
        
        public override void OnLayoutCancelled()
        {
            base.OnLayoutCancelled();
            PDFToHtml.Status = PDFEnum.Failed;
        }

        public override void OnLayoutFailed(ICharSequence error)
        {
            base.OnLayoutFailed(error);
            PDFToHtml.Status = PDFEnum.Failed;
        }
    }

    [Register("android/print/PdfWriteResult")]
    public class PdfWriteResultCallback : PrintDocumentAdapter.WriteResultCallback
    {
        readonly PDFToHtml pDFToHtml;

        public PdfWriteResultCallback(PDFToHtml _pDFToHtml, IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            pDFToHtml = _pDFToHtml;
        }

        public PdfWriteResultCallback(PDFToHtml _pDFToHtml) : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (!(Handle != IntPtr.Zero))
            {
                unsafe
                {
                    JniObjectReference val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }

            pDFToHtml = _pDFToHtml;
        }


        public override void OnWriteFinished(PageRange[] pages)
        {
            base.OnWriteFinished(pages);
            pDFToHtml.Status = PDFEnum.Completed;
        }

        public override void OnWriteCancelled()
        {
            base.OnWriteCancelled();
            pDFToHtml.Status = PDFEnum.Failed;
        }

        public override void OnWriteFailed(ICharSequence error)
        {
            base.OnWriteFailed(error);
            pDFToHtml.Status = PDFEnum.Failed;
        }
    }
```

**For iOS**

With the support of WKWebView and WKNavigationDelegate, you can easily write the data in the file using UIPrintPageRenderer. 

```
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
```

# View the PDF file

Through native support, one can view the PDF file with the help of [pdfjs](https://mozilla.github.io/pdf.js/) library. For more reference, please refer the below article.

https://www.c-sharpcorner.com/article/pdf-viewer-in-android-using-xamarin-forms/ 
