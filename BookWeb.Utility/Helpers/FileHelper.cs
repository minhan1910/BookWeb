using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookWeb.Utility.Helpers
{
    public static class FileHelper
    {
        public static string NormalizeFilePathToWebRootPath(string path)
        {
            string url = path.Replace("\\", "/");

            return "~/"  + url;
        }
    }
}
