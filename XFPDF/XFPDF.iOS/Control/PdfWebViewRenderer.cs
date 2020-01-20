using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XFPDF;
using XFPDF.iOS;

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