using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace XFPDF
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private PDFToHtml PDFToHtml { get; set; }
        public MainPage()
        {
            InitializeComponent();
            this.CreatePDF();
        }

        private async void ConvertHTMLToPDF_Clicked(object sender, EventArgs e)
        {
            if (PDFToHtml.IsPDFGenerating)
                await DisplayAlert("Alert", "PDF is generating...", "Ok");
            else if (PDFToHtml.Status == PDFEnum.Completed)
            {
                try
                {
                    bool result = await DisplayAlert("PDF", "Would you like to generate new PDF?", "Yes", "No");
                    if (result)
                    {
                        this.CreatePDF();
                        PDFToHtml.GeneratePDF();
                    }
                }
                catch
                {
                    await DisplayAlert("ERROR!", "PDF is not generated", "Ok");
                }
            }
            else if (!PDFToHtml.IsPDFGenerating)
                PDFToHtml.GeneratePDF();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (PDFToHtml != null)
            {
                PDFToHtml.Dispose();
                PDFToHtml = null;
            }
        }

        private async void CreatePDF() 
        {
            PDFToHtml = new PDFToHtml();
            this.BindingContext = PDFToHtml;
            PDFToHtml.HTMLString = await HTMLUtils.GetHTMLFromURL("https://www.google.com/");
        }
    }
}
