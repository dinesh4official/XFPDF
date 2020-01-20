using System;
using System.Collections.Generic;
using System.Text;

namespace XFPDF
{
    public interface IFileHelper
    {
        string DocumentFilePath { get; }

        string ResourcesBaseUrl { get; }
    }
}
