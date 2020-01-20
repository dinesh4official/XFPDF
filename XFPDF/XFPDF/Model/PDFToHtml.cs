using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFPDF
{
    public class PDFToHtml : INotifyPropertyChanged, IDisposable 
    {
        private bool ispdfloading;
        private PDFEnum pDFEnum;

        public PDFToHtml() 
        {
            FileName = "Example";
        }

        public bool IsPDFGenerating
        {
            get { return ispdfloading; }
            set
            {
                ispdfloading = value;
                OnPropertyChanged("IsPDFGenerating");
            }
        }
         
        public PDFEnum Status
        {
            get { return pDFEnum; }
            set
            {
                pDFEnum = value;
                this.UpdatePDFStatus(value);
                OnPropertyChanged("Status");
            }
        }

        public string HTMLString { get; set; }

        public string FileName { get; set; }

        public double PageHeight { get; set; } = 1024;

        public double PageWidth { get; set; } = 512;

        public double PageDPI { get; set; } = 300;

        public string FilePath { get; set; }

        public FileStream FileStream { get; set; }

        public byte[] PDFStreamArray { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Dispose();
                FileStream = null;
            }
         
            PDFStreamArray = null;
        }

        public async void GeneratePDF()
        {
            try
            {
                this.Status = PDFEnum.Started;
                FilePath = CreateTempPath(FileName);
                FileStream = File.Create(FilePath);
                DependencyService.Get<IPDFConverter>().ConvertHTMLtoPDF(this);
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("ERROR!", "PDF is not generated", "Ok");
            }
        }

        public static string CreateTempPath(string fileName)
        {
            string tempPath = Path.Combine(DependencyService.Get<IFileHelper>().DocumentFilePath, "temp");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            string path = Path.Combine(tempPath, fileName + ".pdf");
            while (File.Exists(path))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Path.GetExtension(fileName);
                path = Path.Combine(tempPath, fileName + ".pdf");
            }

            return path;
        }

        private async void UpdatePDFStatus(PDFEnum newValue)
        {
            if (newValue == PDFEnum.Started)
                IsPDFGenerating = true;
            else if (newValue == PDFEnum.Failed)
            {
                IsPDFGenerating = false;
                await App.Current.MainPage.DisplayAlert("ERROR!", "PDF is not generated", "Ok");
            }
            else if (newValue == PDFEnum.Completed)
            {
                try
                {
                    PDFStreamArray = Device.RuntimePlatform == Device.iOS ? File.ReadAllBytes(FilePath + ".pdf") : new byte[FileStream.Length];

                    if (Device.RuntimePlatform == Device.Android)
                        FileStream.Read(PDFStreamArray, 0, (int)FileStream.Length);

                    await FileStream.WriteAsync(PDFStreamArray, 0, PDFStreamArray.Length);
                    FileStream.Close();
                    IsPDFGenerating = false;
                    await App.Current.MainPage.Navigation.PushAsync(new PDFViewer() { Title = FileName, BindingContext = this });
                }
                catch
                {
                    await App.Current.MainPage.DisplayAlert("ERROR!", "PDF is not generated", "Ok");
                }
            }
        }

        public void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
