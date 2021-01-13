using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Disco.Services
{
    public static class DocumentTemplateExtensions
    {

        public static Bitmap GenerateTemplatePreview(this DocumentTemplate DocumentTemplate, DiscoDataContext Database, int Width, int PageGapHeight, bool DrawPageBorder)
        {
            string filename = DocumentTemplate.RepositoryFilename(Database);

            if (File.Exists(filename))
            {

                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var pdfDocument = PdfiumViewer.PdfDocument.Load(fileStream))
                    {
                        var pageMaxWidth = (int)pdfDocument.PageSizes.Max(s => s.Width);
                        var pageScale = (float)(Width + (DrawPageBorder ? -2 : 0)) / pageMaxWidth;

                        var previewTotalHeight = pdfDocument.PageSizes
                            .Take(40)
                            .Select(s => (int)(pageScale * s.Height))
                            .Sum() +
                                (DrawPageBorder ? (Math.Min(40, pdfDocument.PageCount) * 2) : 0) +
                                ((Math.Min(40, pdfDocument.PageCount) - 1) * PageGapHeight);

                        var result = new Bitmap(Width, previewTotalHeight);
                        result.SetResolution(72, 72);

                        using (var graphics = Graphics.FromImage(result))
                        {
                            var yPosition = 0;

                            for (int pageIndex = 0; pageIndex < Math.Min(40, pdfDocument.PageCount); pageIndex++)
                            {
                                var pageSize = pdfDocument.PageSizes[pageIndex];
                                var previewWidth = Math.Floor(pageScale * pageSize.Width);
                                var previewHeight = Math.Floor(pageScale * pageSize.Height);

                                // Calculate box
                                var destination = new Rectangle(
                                    x: (int)((Width - previewWidth) / 2),
                                    y: yPosition + (DrawPageBorder ? 1 : 0),
                                    width: (int)previewWidth,
                                    height: (int)previewHeight
                                    );

                                // Fill white background
                                graphics.FillRectangle(Brushes.White, destination);

                                using (var image = pdfDocument.Render(pageIndex, (int)previewWidth, (int)previewHeight, 72F, 72F, false))
                                {
                                    graphics.DrawImage(image, destination.X, destination.Y);
                                }

                                if (DrawPageBorder)
                                {
                                    destination.X -= 1;
                                    destination.Y -= 1;
                                    destination.Width += 1;
                                    destination.Height += 1;
                                    graphics.DrawRectangle(Pens.LightGray, destination);
                                }

                                yPosition += destination.Height + PageGapHeight;
                            }
                        }

                        return result;
                    }
                }

            }

            return null;
        }

        public static IAttachmentTarget ResolveScopeTarget(this AttachmentTypes scope, DiscoDataContext database, string targetId)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrWhiteSpace(targetId))
                throw new ArgumentNullException(nameof(targetId));

            switch (scope)
            {
                case AttachmentTypes.Device:
                    return database.Devices.Find(targetId);
                case AttachmentTypes.Job:
                    if (!int.TryParse(targetId, out var targetIdInt))
                        throw new ArgumentOutOfRangeException(nameof(targetId));
                    return database.Jobs.Find(targetIdInt);
                case AttachmentTypes.User:
                    // special usecase in resolving users (they may not exist in the database yet)
                    targetId = ActiveDirectory.ParseDomainAccountId(targetId);
                    var target = database.Users.Find(targetId);
                    if (target == null)
                    {
                        // try importing user
                        target = UserService.GetUser(targetId, database, true);
                    }
                    return target;
                default:
                    throw new InvalidOperationException("Unexpected DocumentType Scope");
            }
        }

        public static IAttachmentTarget ResolveScopeTarget(this DocumentTemplate template, DiscoDataContext database, string targetId)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            return ResolveScopeTarget(template.AttachmentType, database, targetId);
        }

    }
}
