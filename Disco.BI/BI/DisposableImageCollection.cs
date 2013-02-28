using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Disco.BI
{
    public class DisposableImageCollection : List<Bitmap>, IDisposable
    {
        public void Dispose()
        {
            foreach (Image i in this)
            {
                if (i != null)
                    i.Dispose();
            }
        }

    }
}
