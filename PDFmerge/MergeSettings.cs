using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFmerge
{
    public static class MergeSettings
    {

        public static readonly string INSTALL_DIR = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string APP_DATA_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\PDFmerge"); 




    }
}
