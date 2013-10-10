using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Services.Web.Bundles
{
    public class Bundle
    {
        private DateTime? _FileLastModified { get; set; }
        private string _FileHash { get; set; }
        private string _VersionUrl { get; set; }

        public string Url { get; private set; }
        public string File { get; private set; }
        public string FileHash
        {
            get
            {
#if DEBUG
                UpdateFileHash();
#endif
                return _FileHash;
            }
        }
        public string ContentType { get; private set; }
        public string VersionUrl
        {
            get
            {
#if DEBUG
                return string.Format("{0}?v={1}", this.Url, this.FileHash);
#else
                return _VersionUrl;
#endif
            }
        }

        public Bundle(string Url, string File)
        {
            if (string.IsNullOrWhiteSpace(Url))
                throw new ArgumentNullException("Url");
            if (string.IsNullOrWhiteSpace(File))
                throw new ArgumentNullException("File");

            Uri fileUri;
            if (!Uri.TryCreate(File, UriKind.Absolute, out fileUri))
            {
                File = HttpContext.Current.Server.MapPath(File);
            }

            var fileInfo = new FileInfo(File);

            if (!fileInfo.Exists)
                throw new FileNotFoundException(string.Format("Not Found: {0}", File), File);

            this.Url = Url;
            this.File = File;

            switch (fileInfo.Extension.ToLower())
            {
                case ".css":
                    this.ContentType = "text/css";
                    break;
                case ".js":
                    this.ContentType = "text/javascript";
                    break;
                default:
                    throw new ArgumentException("Unsupported Bundle File Extension");
            }

            // Write File Hash
            if (fileInfo.Length > 0)
                UpdateFileHash();
            else
                this._FileHash = string.Empty;

            //this.Version = fileInfo.LastWriteTimeUtc.Ticks;

            this._VersionUrl = string.Format("{0}?v={1}", this.Url, this.FileHash);
        }

        private void UpdateFileHash()
        {
            if (System.IO.File.Exists(this.File))
            {
                var fileLastModified = System.IO.File.GetLastWriteTimeUtc(this.File);
                if (!this._FileLastModified.HasValue || this._FileLastModified.Value != fileLastModified)
                {
                    this._FileLastModified = fileLastModified;
                    var fileBytes = System.IO.File.ReadAllBytes(this.File);
                    if (fileBytes.Length > 0)
                    {
                        using (SHA256 sha = SHA256.Create())
                        {
                            byte[] hash = sha.ComputeHash(fileBytes);
                            this._FileHash = HttpServerUtility.UrlTokenEncode(hash);
                            return;
                        }
                    }
                }
                else
                {
                    // Already Updated
                    return;
                }
            }

            this._FileHash = string.Empty;
        }

        internal void ProcessRequest(HttpContext context)
        {
            // Write Content Type
            context.Response.ContentType = this.ContentType;

            // Write Headers
            var cache = context.Response.Cache;
            cache.SetOmitVaryStar(true);
            cache.SetExpires(DateTime.Now.AddYears(1));
            cache.SetValidUntilExpires(true);
            cache.SetMaxAge(TimeSpan.FromDays(365));
            cache.SetCacheability(HttpCacheability.Public);

            // Write File
            context.Response.WriteFile(this.File);
        }
    }
}
