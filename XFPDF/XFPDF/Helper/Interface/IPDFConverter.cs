using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XFPDF
{
    public interface IPDFConverter
    {
        void ConvertHTMLtoPDF(PDFToHtml _PDFToHtml);    
    }
}
