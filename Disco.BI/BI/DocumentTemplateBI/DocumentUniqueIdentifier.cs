using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
namespace Disco.BI.DocumentTemplateBI
{
    public class DocumentUniqueIdentifier
    {
        private bool? _loadedComponentsOk;
        private DocumentTemplate _documentTemplate;
        private object _data;
        private string _dataDescription;
        public string TemplateTypeId { get; private set; }
        public string DataId { get; private set; }
        public string DocumentUniqueId
        {
            get
            {
                return string.Format("{0}|{1}", this.TemplateTypeId, this.DataId);
            }
        }
        public string CreatorId { get; private set; }
        public System.DateTime TimeStamp { get; private set; }
        public int Page { get; private set; }
        public string Tag { get; private set; }
        public DocumentTemplate DocumentTemplate
        {
            get
            {
                bool flag = this._loadedComponentsOk.HasValue && this._loadedComponentsOk.Value;
                if (flag)
                {
                    return this._documentTemplate;
                }
                throw new System.Exception("Document Unique Identifier Components not loaded or invalid");
            }
        }
        public object Data
        {
            get
            {
                bool flag = this._loadedComponentsOk.HasValue && this._loadedComponentsOk.Value;
                if (flag)
                {
                    return this._data;
                }
                throw new System.Exception("Document Unique Identifier Components not loaded or invalid");
            }
        }
        public string DataDescription
        {
            get
            {
                bool flag = this._loadedComponentsOk.HasValue && this._loadedComponentsOk.Value;
                if (flag)
                {
                    return this._dataDescription;
                }
                throw new System.Exception("Document Unique Identifier Components not loaded or invalid");
            }
        }
        public string DataScope { get; private set; }
        public static bool IsDocumentUniqueIdentifier(string UniqueIdentifier)
        {
            return UniqueIdentifier.StartsWith("Disco|", System.StringComparison.InvariantCultureIgnoreCase);
        }
        public DocumentUniqueIdentifier(string TemplateTypeId, string DataId, string CreatorId, DateTime TimeStamp, int? Page = null, string Tag = null)
        {
            this.Tag = Tag;
            this.TemplateTypeId = TemplateTypeId;
            this.DataId = DataId;
            this.CreatorId = CreatorId;
            this.TimeStamp = TimeStamp;
            this.Page = Page ?? 0;
        }
        public DocumentUniqueIdentifier(string UniqueIdentifier, string Tag)
        {
            if (!DocumentUniqueIdentifier.IsDocumentUniqueIdentifier(UniqueIdentifier))
            {
                throw new System.ArgumentException("Invalid Document Unique Identifier", "UniqueIdentifier");
            }
            this.Tag = Tag;
            string[] s = UniqueIdentifier.Split(new char[] { '|' });
            string left = s[1].ToUpper();
            if (left == "AT" || left == "1")
            {
                if (s.Length >= 3)
                {
                    this.TemplateTypeId = s[2];
                }
                if (s.Length >= 4)
                {
                    this.DataId = s[3];
                }
                if (s.Length >= 5)
                {
                    this.CreatorId = s[4];
                }
                if (s.Length >= 6)
                {
                    System.DateTime timeStamp;
                    if (System.DateTime.TryParse(s[5], out timeStamp))
                    {
                        this.TimeStamp = timeStamp;
                    }
                }
                if (s.Length >= 7)
                {
                    int page = 0;
                    if (int.TryParse(s[6], out page))
                    {
                        this.Page = page;
                    }
                }
                return;
            }
            throw new System.ArgumentException(string.Format("Invalid Document Unique Identifier Version ({0})", s[1]), "UniqueIdentifier");
        }
        public bool LoadComponents(DiscoDataContext Context)
        {
            bool LoadComponents;
            if (!this._loadedComponentsOk.HasValue)
            {
                string scopeType;
                if (this.TemplateTypeId.StartsWith("--"))
                {
                    string templateTypeId = this.TemplateTypeId;
                    switch (this.TemplateTypeId)
                    {
                        case "--DEVICE":
                            scopeType = DocumentTemplate.DocumentTemplateScopes.Device;
                            break;
                        case "--JOB":
                            scopeType = DocumentTemplate.DocumentTemplateScopes.Job;
                            break;
                        case "--USER":
                            scopeType = DocumentTemplate.DocumentTemplateScopes.User;
                            break;
                        default:
                            scopeType = null;
                            break;
                    }
                }
                else
                {
                    this._documentTemplate = Context.DocumentTemplates.Find(this.TemplateTypeId);
                    if (this._documentTemplate != null)
                    {
                        scopeType = this._documentTemplate.Scope;
                    }
                    else
                    {
                        scopeType = null;
                    }
                }
                if (scopeType != null)
                {
                    this.DataScope = scopeType;
                    switch (scopeType)
                    {
                        case DocumentTemplate.DocumentTemplateScopes.Device:
                            Device d = Context.Devices.Find(this.DataId);
                            if (d != null)
                            {
                                this._data = d;
                                this._dataDescription = d.SerialNumber;
                                this._loadedComponentsOk = true;
                                LoadComponents = true;
                                return LoadComponents;
                            }
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.Job:
                            Job i = Context.Jobs.Find(int.Parse(this.DataId));
                            if (i != null)
                            {
                                this._data = i;
                                this._dataDescription = i.Id.ToString();
                                this._loadedComponentsOk = true;
                                LoadComponents = true;
                                return LoadComponents;
                            }
                            break;
                        case DocumentTemplate.DocumentTemplateScopes.User:
                            User u = Context.Users.Find(this.DataId);
                            if (u != null)
                            {
                                this._data = u;
                                this._dataDescription = u.DisplayName;
                                this._loadedComponentsOk = true;
                                LoadComponents = true;
                                return LoadComponents;
                            }
                            break;
                        default:
                            break;
                    }
                }
                this._loadedComponentsOk = false;
            }
            LoadComponents = this._loadedComponentsOk.Value;
            return LoadComponents;
        }
    }
}
