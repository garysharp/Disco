using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Disco.Services.Plugins.Features.DocumentHandlerProvider
{
    [PluginFeatureCategory(DisplayName = "Document Handler")]
    public abstract class DocumentHandlerProviderFeature : PluginFeature
    {
        public abstract string HandlerTitle { get; }
        public abstract string HandlerDescription { get; }

        public abstract bool CanHandle(DocumentTemplate template);
        public abstract bool CanHandle(DocumentTemplatePackage templatePackage);
        public abstract bool CanHandle(DocumentTemplate template, IAttachmentTarget target);
        public abstract bool CanHandle(DocumentTemplatePackage templatePackage, IAttachmentTarget target);
        public abstract bool CanHandleBulk(DocumentTemplate template);
        public abstract bool CanHandleBulk(DocumentTemplatePackage templatePackage);

        public abstract Type GenerationOptionsUi { get; }
        public virtual string GenerationOptionsIcon => "file-text-o";
        public abstract object GetGenerationOptionsUiModel(DocumentTemplate template, IAttachmentTarget target, User targetUser, User techUser);
        public abstract object GetGenerationOptionsUiModel(DocumentTemplatePackage templatePackage, IAttachmentTarget target, User targetUser, User techUser);

        public abstract ActionResult Handle(DocumentTemplate template, IAttachmentTarget target, User targetUser, User techUser);
        public abstract ActionResult Handle(DocumentTemplatePackage templatePackage, IAttachmentTarget target, User targetUser, User techUser);
        public abstract ActionResult HandleBulk(DocumentTemplate template, IList<IAttachmentTarget> targets, User targetUser, User techUser);
        public abstract ActionResult HandleBulk(DocumentTemplatePackage templatePackage, IList<IAttachmentTarget> targets, User targetUser, User techUser);
    }
}
