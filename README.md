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

Showing a PDF file seems a very easy task, and depending on what platform you are targeting; it is. Through native support, one can view the PDF file with the help of [pdfjs](https://mozilla.github.io/pdf.js/) library.

## Android

The first thing we need to do is download the [pdfjs](https://mozilla.github.io/pdf.js/) library and paste the **pdfjs** folder in **Android Assets**. Make sure the build action for all files is set to `AndroidAsset`. 

```
[assembly: ExportRenderer(typeof(PdfWebView), typeof(PdfWebViewRenderer))]
namespace XFPDF.Droid
{
    public class PdfWebViewRenderer : WebViewRenderer
    {
        private PdfWebView PdfWebView { get { return this.Element as PdfWebView; } }

        private string PdfJsViewerUri => PDFUtils.PdfJsViewerUri;

        public PdfWebViewRenderer(Android.Content.Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                Control.Settings.AllowFileAccess = true;
                Control.Settings.AllowFileAccessFromFileURLs = true;
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
                Control.Settings.UseWideViewPort = true;
                Control.Settings.LoadWithOverviewMode = true;
                this.UpdateDisplayZoomControls();
                this.UpdateEnableZoomControls();
                this.Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
                this.LoadPdfFile(this.PdfWebView.Uri);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == PdfWebView.UriProperty.PropertyName)
                this.LoadPdfFile(this.PdfWebView.Uri);
        }

        void UpdateEnableZoomControls()
        {
            // BuiltInZoomControls supported as of API level 3
            if (Control != null && ((int)Build.VERSION.SdkInt >= 3))
            {
                var value = Element.OnThisPlatform().ZoomControlsEnabled();
                Control.Settings.SetSupportZoom(value);
                Control.Settings.BuiltInZoomControls = value;
            }
        }

        void UpdateDisplayZoomControls()
        {
            // DisplayZoomControls supported as of API level 11
            if (Control != null && ((int)Build.VERSION.SdkInt >= 11))
            {
                var value = Element.OnThisPlatform().ZoomControlsDisplayed();
                Control.Settings.DisplayZoomControls = value;
            }
        }

        void LoadPdfFile(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return;

            string url = $"?file={WebUtility.UrlEncode(uri)}";
            Control.LoadUrl(PdfJsViewerUri + url);
        }
    }
}
```

## iOS

For iOS platform, just copy the downloaded **pdfjs** folder and paste it into the **Resources** folder. Make sure the build action for all files is set to **BundleResource**.

```
[assembly: ExportRenderer(typeof(PdfWebView), typeof(PdfWebViewRenderer))]
namespace XFPDF.iOS
{
    class PdfWebViewRenderer : ViewRenderer<PdfWebView, UIWebView>
    {
        private string PdfJsViewerUri => PDFUtils.PdfJsViewerUri;

        public PdfWebViewRenderer() : base()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<PdfWebView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.SetNativeControl(new UIWebView());
                this.Control.ScrollView.Bounces = false;
                this.Control.ScrollView.BouncesZoom = false;
                this.Control.ScrollView.AlwaysBounceHorizontal = false;
                this.Control.ScrollView.AlwaysBounceVertical = false;
                this.LoadPdfFile(this.Element?.Uri);
                this.Control.BackgroundColor = UIColor.Clear;
                //Control.ScrollView.ScrollEnabled = false;
                //Control.ScalesPageToFit = false;
                //Control.MultipleTouchEnabled = false;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == PdfWebView.UriProperty.PropertyName)
                this.LoadPdfFile(this.Element.Uri);
        }

        private void LoadPdfFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            Control.LoadRequest(new NSUrlRequest(new NSUrl(PdfJsViewerUri, false)));
            Control.LoadFinished += Control_LoadFinished;
        }

        private void Control_LoadFinished(object sender, System.EventArgs e)
        {
            Control.LoadFinished -= Control_LoadFinished;
            Control.EvaluateJavascript($"DEFAULT_URL='{Element?.Uri}'; window.location.href='{PdfJsViewerUri}?file=file://{Element?.Uri}'; ");
        }
    }
}
```

## Display the PDF file

All set, now its time to use the `pdfjs` to show the PDF file. Write the custom renderers for each platform for your custom **WebView** as shared in above code snippet and in  `Uri` bindable property set the path of PDF file.

Additionally, to give a connection between the `pdfjs` and this project, you need to bind them as like below code example.

**Android**
```
string url = $"?file={WebUtility.UrlEncode(uri)}";
Control.LoadUrl(PdfJsViewerUri + url);
```

**iOS**
```
Control.EvaluateJavascript($"DEFAULT_URL='{Element?.Uri}'; window.location.href='{PdfJsViewerUri}?file=file://{Element?.Uri}'; ");
```

Where `PdfJsViewerUri` is nothing but the path of `pdfjs` folder.

```
public static class PDFUtils
{
  private static string GetBaseUrl()
  {
     //fileHelper returns the path of pdfjs folder for both Android and iOS project.
     var fileHelper = DependencyService.Get<IFileHelper>();
     return fileHelper.ResourcesBaseUrl + "pdfjs/";
  }

  public static string PdfJsViewerUri => GetBaseUrl() + "web/viewer.html";
}
```

