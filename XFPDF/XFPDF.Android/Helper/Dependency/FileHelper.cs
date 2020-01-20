using Xamarin.Forms;
using XFPDF.Droid;

[assembly: Dependency(typeof(FileHelper))]
namespace XFPDF.Droid
{
    public class FileHelper : IFileHelper
    {
        public string DocumentFilePath => GetLocalFilePath();

        private string GetLocalFilePath()
        {
            //For dummy file path creation.
            //return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
        }

        public string ResourcesBaseUrl => "file:///android_asset/";
    }
}