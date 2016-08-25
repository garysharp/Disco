using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;

namespace Disco.BI.Extensions
{
    public static class UtilityExtensions
    {

        public static string StreamToString(this System.IO.Stream stream)
        {
            if (stream.Position != 0 && stream.CanSeek)
            {
                stream.Position = 0;
            }
            using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
        
    }
}
