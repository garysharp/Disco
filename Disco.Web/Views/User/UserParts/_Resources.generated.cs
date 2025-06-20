﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Disco.Web.Views.User.UserParts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using Disco;
    using Disco.Models.Repository;
    using Disco.Services;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/User/UserParts/_Resources.cshtml")]
    public partial class _Resources : Disco.Services.Web.WebViewPage<Disco.Web.Models.User.ShowModel>
    {
        public _Resources()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Views\User\UserParts\_Resources.cshtml"
  
    Authorization.Require(Claims.User.ShowAttachments);

    var canAddAttachments = Authorization.Has(Claims.User.Actions.AddAttachments);
    var canRemoveAnyAttachments = Authorization.Has(Claims.User.Actions.RemoveAnyAttachments);
    var canRemoveOwnAttachments = Authorization.Has(Claims.User.Actions.RemoveOwnAttachments);

    Html.BundleDeferred("~/Style/Shadowbox");
    Html.BundleDeferred("~/ClientScripts/Modules/Shadowbox");
    Html.BundleDeferred("~/ClientScripts/Modules/jQuery-SignalR");

    if (canAddAttachments)
    {
        Html.BundleDeferred("~/ClientScripts/Modules/Disco-AttachmentUploader");
    }

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" id=\"UserDetailTab-Resources\"");

WriteLiteral(" class=\"UserPart\"");

WriteLiteral(">\r\n    <table");

WriteLiteral(" id=\"userShowResources\"");

WriteLiteral(">\r\n        <tr>\r\n            <td");

WriteLiteral(" id=\"AttachmentsContainer\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" id=\"Attachments\"");

WriteAttribute("class", Tuple.Create(" class=\"", 872), Tuple.Create("\"", 947)
            
            #line 22 "..\..\Views\User\UserParts\_Resources.cshtml"
, Tuple.Create(Tuple.Create("", 880), Tuple.Create<System.Object, System.Int32>(canAddAttachments ? "canAddAttachments" : "cannotAddAttachments"
            
            #line default
            #line hidden
, 880), false)
);

WriteLiteral(" data-uploadurl=\"");

            
            #line 22 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                              Write(Url.Action(MVC.API.User.AttachmentUpload(Model.User.UserId, null)));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-onlineuploadurl=\"");

            
            #line 22 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                                                                                                                           Write(Url.Action(MVC.API.User.AttachmentOnlineUploadSession(Model.User.UserId)));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-qrcodeurl=\"");

            
            #line 22 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                                                                                                                                                                                                                        Write(Url.Content("~/ClientSource/Scripts/Modules/qrcode.min.js"));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 23 "..\..\Views\User\UserParts\_Resources.cshtml"
               Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n                    <div");

WriteLiteral(" class=\"Disco-AttachmentUpload-DropTarget\"");

WriteLiteral(">\r\n                        <h2>Drop Attachments Here</h2>\r\n                    </" +
"div>\r\n                    <div");

WriteLiteral(" class=\"attachmentOutput\"");

WriteLiteral(">\r\n");

            
            #line 28 "..\..\Views\User\UserParts\_Resources.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 28 "..\..\Views\User\UserParts\_Resources.cshtml"
                         if (Model.User.UserAttachments != null)
                        {
                            foreach (var ua in Model.User.UserAttachments.OrderByDescending(a => a.Id))
                            {

            
            #line default
            #line hidden
WriteLiteral("                                <a");

WriteAttribute("href", Tuple.Create(" href=\"", 1730), Tuple.Create("\"", 1788)
            
            #line 32 "..\..\Views\User\UserParts\_Resources.cshtml"
, Tuple.Create(Tuple.Create("", 1737), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.User.AttachmentDownload(ua.Id))
            
            #line default
            #line hidden
, 1737), false)
);

WriteLiteral(" data-attachmentid=\"");

            
            #line 32 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                            Write(ua.Id);

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-mimetype=\"");

            
            #line 32 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                                   Write(ua.MimeType);

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(">\r\n                                    <span");

WriteLiteral(" class=\"icon\"");

WriteAttribute("title", Tuple.Create(" title=\"", 1902), Tuple.Create("\"", 1922)
            
            #line 33 "..\..\Views\User\UserParts\_Resources.cshtml"
, Tuple.Create(Tuple.Create("", 1910), Tuple.Create<System.Object, System.Int32>(ua.Filename
            
            #line default
            #line hidden
, 1910), false)
);

WriteLiteral(">\r\n                                        <img");

WriteLiteral(" alt=\"Attachment Thumbnail\"");

WriteAttribute("src", Tuple.Create(" src=\"", 1997), Tuple.Create("\"", 2057)
            
            #line 34 "..\..\Views\User\UserParts\_Resources.cshtml"
, Tuple.Create(Tuple.Create("", 2003), Tuple.Create<System.Object, System.Int32>(Url.Action(MVC.API.User.AttachmentThumbnail(ua.Id))
            
            #line default
            #line hidden
, 2003), false)
);

WriteLiteral(" />\r\n                                    </span>\r\n                               " +
"     <span");

WriteLiteral(" class=\"comments\"");

WriteAttribute("title", Tuple.Create(" title=\"", 2166), Tuple.Create("\"", 2186)
            
            #line 36 "..\..\Views\User\UserParts\_Resources.cshtml"
, Tuple.Create(Tuple.Create("", 2174), Tuple.Create<System.Object, System.Int32>(ua.Comments
            
            #line default
            #line hidden
, 2174), false)
);

WriteLiteral(">\r\n");

            
            #line 37 "..\..\Views\User\UserParts\_Resources.cshtml"
                                        
            
            #line default
            #line hidden
            
            #line 37 "..\..\Views\User\UserParts\_Resources.cshtml"
                                          if (!string.IsNullOrEmpty(ua.DocumentTemplateId))
                                            { 
            
            #line default
            #line hidden
            
            #line 38 "..\..\Views\User\UserParts\_Resources.cshtml"
                                         Write(ua.DocumentTemplate.Description);

            
            #line default
            #line hidden
            
            #line 38 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                              }
                                        else
                                        { 
            
            #line default
            #line hidden
            
            #line 40 "..\..\Views\User\UserParts\_Resources.cshtml"
                                      Write(ua.Comments ?? ua.Filename);

            
            #line default
            #line hidden
            
            #line 40 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                        }
            
            #line default
            #line hidden
WriteLiteral("\r\n                                    </span><span");

WriteLiteral(" class=\"author\"");

WriteLiteral(">");

            
            #line 41 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                           Write(ua.TechUser.ToStringFriendly());

            
            #line default
            #line hidden
WriteLiteral("</span>");

            
            #line 41 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                       if (canRemoveAnyAttachments || (canRemoveOwnAttachments && ua.TechUserId.Equals(CurrentUser.UserId, StringComparison.OrdinalIgnoreCase)))
                                    {
            
            #line default
            #line hidden
WriteLiteral("<span");

WriteLiteral(" class=\"remove fa fa-times-circle\"");

WriteLiteral("></span>");

            
            #line 42 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                 }
            
            #line default
            #line hidden
WriteLiteral("<span");

WriteLiteral(" class=\"timestamp\"");

WriteLiteral(" data-livestamp=\"");

            
            #line 42 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                                      Write(ua.Timestamp.ToUnixEpoc());

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteAttribute("title", Tuple.Create(" title=\"", 2895), Tuple.Create("\"", 2933)
            
            #line 42 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                               , Tuple.Create(Tuple.Create("", 2903), Tuple.Create<System.Object, System.Int32>(ua.Timestamp.ToFullDateTime()
            
            #line default
            #line hidden
, 2903), false)
);

WriteLiteral(">");

            
            #line 42 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                                                                                                          Write(ua.Timestamp.ToFullDateTime());

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n                                </a>\r\n");

            
            #line 44 "..\..\Views\User\UserParts\_Resources.cshtml"
                            }
                        }

            
            #line default
            #line hidden
WriteLiteral("                    </div>\r\n");

            
            #line 47 "..\..\Views\User\UserParts\_Resources.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 47 "..\..\Views\User\UserParts\_Resources.cshtml"
                     if (canAddAttachments)
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <div");

WriteLiteral(" class=\"Disco-AttachmentUpload-Progress\"");

WriteLiteral("></div>\r\n");

WriteLiteral("                        <div");

WriteLiteral(" class=\"attachmentInput clearfix\"");

WriteLiteral(">\r\n                            <span");

WriteLiteral(" class=\"action enabled upload fa fa-upload disabled\"");

WriteLiteral(" title=\"Attach File\"");

WriteLiteral("></span><span");

WriteLiteral(" class=\"action enabled photo fa fa-camera disabled\"");

WriteLiteral(" title=\"Capture Image\"");

WriteLiteral("></span><span");

WriteLiteral(" class=\"action enabled online-upload fa fa-qrcode disabled\"");

WriteLiteral(" title=\"Upload with Online Services\"");

WriteLiteral("></span>\r\n                        </div>\r\n");

            
            #line 53 "..\..\Views\User\UserParts\_Resources.cshtml"
                    }

            
            #line default
            #line hidden
WriteLiteral("                    <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
                        Shadowbox.init({
                            skipSetup: true,
                            modal: true
                        });
                        $(function () {
                            var $Attachments = $('#Attachments');
                            var $attachmentOutput = $Attachments.find('.attachmentOutput');
                            var $dialogRemoveAttachment = null;

                            // Connect to Hub
                            var hub = $.connection.userUpdates;

                            // Map Functions
                            hub.client.addAttachment = onAddAttachment;
                            hub.client.removeAttachment = onRemoveAttachment;

                            $.connection.hub.qs = { UserId: '");

            
            #line 71 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                         Write(Model.User.UserId.Replace(@"\", @"\\"));

            
            #line default
            #line hidden
WriteLiteral("\' };\r\n                            $.connection.hub.error(onHubFailed);\r\n         " +
"                   $.connection.hub.disconnected(onHubFailed);\r\n\r\n              " +
"              $.connection.hub.reconnecting(function () {\r\n                     " +
"           $(\'#AttachmentsContainer\').find(\'span.action.enabled\').addClass(\'disa" +
"bled\');\r\n                            });\r\n                            $.connecti" +
"on.hub.reconnected(function () {\r\n                                $(\'#Attachment" +
"sContainer\').find(\'span.action.enabled\').removeClass(\'disabled\');\r\n             " +
"               });\r\n\r\n                            // Start Connection\r\n         " +
"                   $.connection.hub.start(function () {\r\n                       " +
"         $(\'#AttachmentsContainer\').find(\'span.action.enabled\').removeClass(\'dis" +
"abled\');\r\n                            }).fail(onHubFailed);\r\n\r\n                 " +
"           function onHubFailed(error) {\r\n                                // Dis" +
"able UI\r\n                                $(\'#AttachmentsContainer\').find(\'span.a" +
"ction.enabled\').addClass(\'disabled\');\r\n\r\n                                // Show" +
" Dialog Message\r\n                                if ($(\'.disconnected-dialog\').l" +
"ength == 0) {\r\n                                    $(\'<div>\')\r\n                 " +
"                       .addClass(\'dialog disconnected-dialog\')\r\n                " +
"                        .html(\'<h3><span class=\"fa-stack fa-lg\"><i class=\"fa fa-" +
"wifi fa-stack-1x\"></i><i class=\"fa fa-ban fa-stack-2x error\"></i></span>Disconne" +
"cted from the Disco ICT Server</h3><div>This page is not receiving live updates." +
" Please ensure you are connected to the server, then refresh this page to enable" +
" features.</div>\')\r\n                                        .dialog({\r\n         " +
"                                   resizable: false,\r\n                          " +
"                  title: \'Disconnected\',\r\n                                      " +
"      width: 400,\r\n                                            modal: true,\r\n   " +
"                                         buttons: {\r\n                           " +
"                     \'Refresh Now\': function () {\r\n                             " +
"                       $(this).dialog(\'option\', \'buttons\', null);\r\n             " +
"                                       window.location.reload(true);\r\n          " +
"                                      },\r\n                                      " +
"          \'Close\': function () {\r\n                                              " +
"      $(this).dialog(\'destroy\');\r\n                                              " +
"  }\r\n                                            }\r\n                            " +
"            });\r\n                                }\r\n                            " +
"}\r\n\r\n                            function onAddAttachment(id, quick) {\r\n        " +
"                        var data = { id: id };\r\n                                " +
"$.ajax({\r\n                                    url: \'");

            
            #line 117 "..\..\Views\User\UserParts\_Resources.cshtml"
                                     Write(Url.Action(MVC.API.User.Attachment()));

            
            #line default
            #line hidden
WriteLiteral(@"',
                                    dataType: 'json',
                                    data: data,
                                    success: function (d) {
                                        if (d.Result == 'OK') {
                                            var a = d.Attachment;
");

            
            #line 123 "..\..\Views\User\UserParts\_Resources.cshtml"
                                        
            
            #line default
            #line hidden
            
            #line 123 "..\..\Views\User\UserParts\_Resources.cshtml"
                                         if (canRemoveAnyAttachments)
                                        {

            
            #line default
            #line hidden
WriteLiteral("                                        ");

WriteLiteral("buildAttachment(a, true, quick);");

WriteLiteral("\r\n");

            
            #line 126 "..\..\Views\User\UserParts\_Resources.cshtml"
                                        }
                                        else if (canRemoveOwnAttachments)
                                        {

            
            #line default
            #line hidden
WriteLiteral("                                        ");

WriteLiteral("buildAttachment(a, (a.AuthorId === \'");

            
            #line 129 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                              Write(CurrentUser.UserId);

            
            #line default
            #line hidden
WriteLiteral("\'), quick);");

WriteLiteral("\r\n");

            
            #line 130 "..\..\Views\User\UserParts\_Resources.cshtml"
                                        }
                                        else
                                        {

            
            #line default
            #line hidden
WriteLiteral("                                        ");

WriteLiteral("buildAttachment(a, false, quick);");

WriteLiteral("\r\n");

            
            #line 134 "..\..\Views\User\UserParts\_Resources.cshtml"
                                        }

            
            #line default
            #line hidden
WriteLiteral(@"                                        } else {
                                            alert('Unable to add attachment: ' + d.Result);
                                        }
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        alert('Unable to add attachment: ' + textStatus);
                                    }
                                });
                            }
                            function buildAttachment(a, canRemove, quick) {
                                var t = '<a><span class=""icon""><img alt=""Attachment Thumbnail"" /></span><span class=""comments""></span><span class=""author""></span>';
                                if (canRemove)
                                    t += '<span class=""remove fa fa-times-circle""></span>';
                                t += '<span class=""timestamp""></span></a>';

                                var e = $(t);

                                e.attr('data-attachmentid', a.Id).attr('data-mimetype', a.MimeType).attr('href', '");

            
            #line 152 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                              Write(Url.Action(MVC.API.User.AttachmentDownload()));

            
            #line default
            #line hidden
WriteLiteral(@"/' + a.Id);
                                e.find('.comments').text(a.Description);
                                e.find('.author').text(a.Author);
                                e.find('.timestamp').text(a.TimestampFull).attr('title', a.TimestampFull).livestamp(a.TimestampUnixEpoc);
                                if (canRemove)
                                    e.find('.remove').click(removeAttachment);
                                if (!quick)
                                    e.hide();
                                $attachmentOutput.prepend(e);
                                onUpdate();
                                if (!quick)
                                    e.show('slow');
                                if (a.MimeType.toLowerCase().indexOf('image/') == 0)
                                    e.shadowbox({ gallery: 'attachments', player: 'img', title: a.Description });
                                else
                                    e.click(onDownload);

                                // Add Thumbnail
                                var buildThumbnail = function () {
                                    var retryCount = 0;
                                    var img = e.find('.icon img');

                                    var setThumbnailUrl = function () {
                                        img.attr('src', '");

            
            #line 175 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                     Write(Url.Action(MVC.API.User.AttachmentThumbnail()));

            
            #line default
            #line hidden
WriteLiteral("/\' + a.Id + \'?v=\' + retryCount);\r\n                                    };\r\n       " +
"                             img.on(\'error\', function () {\r\n                    " +
"                    img.addClass(\'loading\');\r\n                                  " +
"      retryCount++;\r\n                                        if (retryCount < 6)" +
"\r\n                                            window.setTimeout(setThumbnailUrl," +
" retryCount * 250);\r\n                                    });\r\n                  " +
"                  img.on(\'load\', function () {\r\n                                " +
"        img.removeClass(\'loading\');\r\n                                    });\r\n  " +
"                                  window.setTimeout(setThumbnailUrl, 100);\r\n    " +
"                            };\r\n                                buildThumbnail()" +
";\r\n                            }\r\n\r\n                            function onDownl" +
"oad() {\r\n                                var $this = $(this);\r\n                 " +
"               var url = $this.attr(\'href\');\r\n\r\n                                " +
"if ($.connection && $.connection.hub && $.connection.hub.transport &&\r\n         " +
"                                           $.connection.hub.transport.name == \'f" +
"oreverFrame\') {\r\n                                    // SignalR active with fore" +
"verFrame transport - use popup window\r\n                                    windo" +
"w.open(url, \'_blank\', \'height=150,width=250,location=no,menubar=no,resizable=no," +
"scrollbars=no,status=no,toolbar=no\');\r\n                                } else {\r" +
"\n                                    // use iFrame\r\n                            " +
"        if (!$attachmentDownloadHost) {\r\n                                       " +
" $attachmentDownloadHost = $(\'<iframe>\')\r\n                                      " +
"      .attr({ \'src\': url, \'title\': \'Attachment Download Host\' })\r\n              " +
"                              .addClass(\'hidden\')\r\n                             " +
"               .appendTo(\'body\')\r\n                                            .c" +
"ontents();\r\n                                    } else {\r\n                      " +
"                  $attachmentDownloadHost[0].location.href = url;\r\n             " +
"                       }\r\n                                }\r\n\r\n                 " +
"               return false;\r\n                            }\r\n\r\n                 " +
"           function onRemoveAttachment(id) {\r\n                                va" +
"r a = $attachmentOutput.find(\'a[data-attachmentid=\' + id + \']\');\r\n\r\n            " +
"                    a.hide(300).delay(300).queue(function () {\r\n                " +
"                    var $this = $(this);\r\n                                    if" +
" ($this.attr(\'data-mimetype\').toLowerCase().indexOf(\'image/\') == 0)\r\n           " +
"                             Shadowbox.removeCache(this);\r\n                     " +
"               $this.find(\'.timestamp\').livestamp(\'destroy\');\r\n                 " +
"                   $this.remove();\r\n                                    onUpdate" +
"();\r\n                                });\r\n                            }\r\n\r\n     " +
"                       function onUpdate() {\r\n                                va" +
"r attachmentCount = $attachmentOutput.children(\'a\').length;\r\n                   " +
"             var tabHeading = \'Attachments [\' + attachmentCount + \']\';\r\n        " +
"                        $(\'#UserDetailTab-ResourcesLink\').text(tabHeading);\r\n   " +
"                         }\r\n\r\n");

            
            #line 234 "..\..\Views\User\UserParts\_Resources.cshtml"
                        
            
            #line default
            #line hidden
            
            #line 234 "..\..\Views\User\UserParts\_Resources.cshtml"
                         if (canAddAttachments)
                        {
            
            #line default
            #line hidden
WriteLiteral("\r\n                            //#region Add Attachments\r\n                        " +
"    var attachmentUploader = new document.Disco.AttachmentUploader($Attachments)" +
";\r\n\r\n                            var $attachmentInput = $Attachments.find(\'.atta" +
"chmentInput\');\r\n                            $attachmentInput.find(\'.online-uploa" +
"d\').on(\'click\', function () {\r\n                                if ($(this).hasCl" +
"ass(\'disabled\'))\r\n                                    alert(\'Disconnected from t" +
"he Disco ICT Server, please refresh this page and try again\');\r\n                " +
"                else\r\n                                    attachmentUploader.onl" +
"ineUpload();\r\n                            });\r\n                            if (w" +
"indow.location.protocol != \'https:\') {\r\n                                $attachm" +
"entInput.find(\'.photo\')\r\n                                    .removeClass(\'enabl" +
"ed\')\r\n                                    .addClass(\'disabled\')\r\n               " +
"                     .attr(\'title\', \'Capture Image: this functionality is only a" +
"vailable over a HTTPS connection\');\r\n                            }\r\n            " +
"                $attachmentInput.find(\'.photo\').click(function () {\r\n           " +
"                     if (!$(this).hasClass(\'enabled\'))\r\n                        " +
"            alert(\'This functionality is only available over a HTTPS connection\'" +
");\r\n                                else if ($(this).hasClass(\'disabled\'))\r\n    " +
"                                alert(\'Disconnected from the Disco ICT Server, p" +
"lease refresh this page and try again\');\r\n                                else\r\n" +
"                                    attachmentUploader.uploadImage();\r\n         " +
"                   });\r\n                            $attachmentInput.find(\'.uplo" +
"ad\').click(function () {\r\n                                if ($(this).hasClass(\'" +
"disabled\'))\r\n                                    alert(\'Disconnected from the Di" +
"sco ICT Server, please refresh this page and try again\');\r\n                     " +
"           else\r\n                                    attachmentUploader.uploadFi" +
"les();\r\n                            });\r\n\r\n                            var resou" +
"rcesTab;\r\n                            $(document).on(\'dragover\', function () {\r\n" +
"                                if (!resourcesTab) {\r\n                          " +
"          var tabs = $Attachments.closest(\'.ui-tabs\');\r\n                        " +
"            resourcesTab = {\r\n                                        tabs: tabs" +
",\r\n                                        resourcesIndex: tabs.children(\'ul.ui-" +
"tabs-nav\').find(\'a[href=\"#UserDetailTab-Resources\"]\').closest(\'li\').index()\r\n   " +
"                                 };\r\n                                }\r\n        " +
"                        var selectedIndex = resourcesTab.tabs.tabs(\'option\', \'ac" +
"tive\');\r\n                                if (resourcesTab.resourcesIndex !== sel" +
"ectedIndex)\r\n                                    resourcesTab.tabs.tabs(\'option\'" +
", \'active\', resourcesTab.resourcesIndex);\r\n                            });\r\n    " +
"                        //#endregion\r\n                            ");

            
            #line 281 "..\..\Views\User\UserParts\_Resources.cshtml"
                                   }

            
            #line default
            #line hidden
WriteLiteral("                        ");

            
            #line 282 "..\..\Views\User\UserParts\_Resources.cshtml"
                         if (canRemoveAnyAttachments || canRemoveOwnAttachments)
                        {
            
            #line default
            #line hidden
WriteLiteral(@"
                            //#region Remove Attachments

                            $attachmentOutput.find('span.remove').click(removeAttachment);

                            function removeAttachment() {
                                $this = $(this).closest('a');

                                var data = { id: $this.attr('data-attachmentid') };

                                if (!$dialogRemoveAttachment) {
                                    $dialogRemoveAttachment = $('#dialogRemoveAttachment').dialog({
                                        resizable: false,
                                        height: 140,
                                        modal: true,
                                        autoOpen: false
                                    });
                                }

                                $dialogRemoveAttachment.dialog(""enable"");
                                $dialogRemoveAttachment.dialog('option', 'buttons', {
                                    ""Remove"": function () {
                                        $dialogRemoveAttachment.dialog(""disable"");
                                        $dialogRemoveAttachment.dialog(""option"", ""buttons"", null);
                                        $.ajax({
                                            url: '");

            
            #line 308 "..\..\Views\User\UserParts\_Resources.cshtml"
                                             Write(Url.Action(MVC.API.User.AttachmentRemove()));

            
            #line default
            #line hidden
WriteLiteral("\',\r\n                                            dataType: \'json\',\r\n              " +
"                              data: data,\r\n                                     " +
"       success: function (d) {\r\n                                                " +
"if (d == \'OK\') {\r\n                                                    // Do noth" +
"ing, await SignalR notification\r\n                                               " +
" } else {\r\n                                                    alert(\'Unable to " +
"remove attachment: \' + d);\r\n                                                }\r\n " +
"                                               $dialogRemoveAttachment.dialog(\"c" +
"lose\");\r\n                                            },\r\n                       " +
"                     error: function (jqXHR, textStatus, errorThrown) {\r\n       " +
"                                         alert(\'Unable to remove attachment: \' +" +
" textStatus);\r\n                                                $dialogRemoveAtta" +
"chment.dialog(\"close\");\r\n                                            }\r\n        " +
"                                });\r\n                                    },\r\n   " +
"                                 Cancel: function () {\r\n                        " +
"                $dialogRemoveAttachment.dialog(\"close\");\r\n                      " +
"              }\r\n                                });\r\n\r\n                        " +
"        $dialogRemoveAttachment.dialog(\'open\');\r\n\r\n                             " +
"   return false;\r\n                            }\r\n\r\n                            /" +
"/#endregion\r\n                        ");

            
            #line 336 "..\..\Views\User\UserParts\_Resources.cshtml"
                               }

            
            #line default
            #line hidden
WriteLiteral(@"
                            $attachmentOutput.children('a').each(function () {
                                $this = $(this);
                                if ($this.attr('data-mimetype').toLowerCase().indexOf('image/') == 0)
                                    $this.shadowbox({ gallery: 'attachments', player: 'img', title: $this.find('.comments').text() });
                                else
                                    $this.click(onDownload);
                            });
                        });
                    </script>
                </div>
            </td>
        </tr>
    </table>
    <script>
        $('#UserDetailTabItems').append('<li><a href=""#UserDetailTab-Resources"" id=""UserDetailTab-ResourcesLink"">Attachments [");

            
            #line 352 "..\..\Views\User\UserParts\_Resources.cshtml"
                                                                                                                          Write(Model.User.UserAttachments == null ? 0 : Model.User.UserAttachments.Count);

            
            #line default
            #line hidden
WriteLiteral("]</a></li>\');\r\n    </script>\r\n</div>\r\n");

            
            #line 355 "..\..\Views\User\UserParts\_Resources.cshtml"
 if (canRemoveAnyAttachments || canRemoveOwnAttachments)
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" id=\"dialogRemoveAttachment\"");

WriteLiteral(" class=\"dialog\"");

WriteLiteral(" title=\"Remove this Attachment?\"");

WriteLiteral(">\r\n        <p>\r\n            <i");

WriteLiteral(" class=\"fa fa-exclamation-triangle fa-lg\"");

WriteLiteral("></i>&nbsp;Are you sure?\r\n        </p>\r\n    </div>\r\n");

            
            #line 362 "..\..\Views\User\UserParts\_Resources.cshtml"
}
            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591
