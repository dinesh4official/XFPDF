using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XFPDF
{
    public class PdfWebView : WebView
    {
        public static readonly BindableProperty UriProperty = BindableProperty.Create(nameof(Uri), typeof(string), typeof(PdfWebView));

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public PdfWebView()
        {

        }
    }
}
