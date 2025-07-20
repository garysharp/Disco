using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace Disco.Services.Web.Bundles
{
    public class FileBundle : IBundle
    {
        private DateTime? _FileLastModified { get; set; }
        private string _FileHash { get; set; }
        private string _VersionUrl { get; set; }

        public bool RemapRequest { get { return true; } }
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
                return $"{Url}?v={FileHash}";
#else
                return _VersionUrl;
#endif
            }
        }

        public FileBundle(string Url, string File)
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
                throw new FileNotFoundException($"Not Found: {File}", File);

            this.Url = Url;
            this.File = File;

            switch (fileInfo.Extension.ToLower())
            {
                case ".css":
                    ContentType = "text/css";
                    break;
                case ".js":
                    ContentType = "text/javascript";
                    break;
                default:
                    throw new ArgumentException("Unsupported Bundle File Extension");
            }

            // Write File Hash
            if (fileInfo.Length > 0)
                UpdateFileHash();
            else
                _FileHash = string.Empty;

            //this.Version = fileInfo.LastWriteTimeUtc.Ticks;

            _VersionUrl = $"{this.Url}?v={FileHash}";
        }

        private void UpdateFileHash()
        {
            if (System.IO.File.Exists(File))
            {
                var fileLastModified = System.IO.File.GetLastWriteTimeUtc(File);
                if (!_FileLastModified.HasValue || _FileLastModified.Value != fileLastModified)
                {
                    _FileLastModified = fileLastModified;
                    var fileBytes = System.IO.File.ReadAllBytes(File);
                    if (fileBytes.Length > 0)
                    {
                        using (SHA256 sha = SHA256.Create())
                        {
                            byte[] hash = sha.ComputeHash(fileBytes);
                            _FileHash = HttpServerUtility.UrlTokenEncode(hash);
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

            _FileHash = string.Empty;
        }

        public void ProcessRequest(HttpContext context)
        {
            // Write Content Type
            context.Response.ContentType = ContentType;

            // Write Headers
            var cache = context.Response.Cache;
            cache.SetOmitVaryStar(true);
            cache.SetExpires(DateTime.Now.AddYears(1));
            cache.SetValidUntilExpires(true);
            cache.SetMaxAge(TimeSpan.FromDays(365));
            cache.SetCacheability(HttpCacheability.Public);

            // Write File
            context.Response.WriteFile(File);
        }
    }
}
