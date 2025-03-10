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

namespace Disco.Web.Areas.Config.Views.Enrolment
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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Config/Views/Enrolment/Index.cshtml")]
    public partial class Index : Disco.Services.Web.WebViewPage<Disco.Web.Areas.Config.Models.Enrolment.IndexModel>
    {
        public Index()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
  
    Authorization.Require(Claims.Config.Enrolment.Show);

    var canConfig = Authorization.Has(Claims.Config.Enrolment.Configure);
    var canShowStatus = Authorization.Has(Claims.Config.Enrolment.ShowStatus);

    ViewBag.Title = Html.ToBreadcrumb("Configuration", MVC.Config.Config.Index(), "Device Enrolment");

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 530px;\"");

WriteLiteral(">\r\n    <table>\r\n        <tr>\r\n            <th>\r\n                Pending Timeout:\r" +
"\n            </th>\r\n            <td>\r\n");

            
            #line 17 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                
            
            #line default
            #line hidden
            
            #line 17 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                 if (canConfig)
                {
                    
            
            #line default
            #line hidden
            
            #line 19 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(Html.TextBoxFor(model => model.PendingTimeoutMinutes, new { type = "number", min = "1" }));

            
            #line default
            #line hidden
            
            #line 19 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                                                                                              
                    
            
            #line default
            #line hidden
            
            #line 20 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(AjaxHelpers.AjaxSave());

            
            #line default
            #line hidden
            
            #line 20 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                           
                    
            
            #line default
            #line hidden
            
            #line 21 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(AjaxHelpers.AjaxLoader());

            
            #line default
            #line hidden
            
            #line 21 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                             

            
            #line default
            #line hidden
WriteLiteral("                    <span> minutes <span");

WriteLiteral(" class=\"smallText\"");

WriteLiteral(">(default: 30)</span></span>\r\n");

WriteLiteral("                    <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
                    $(function () {
                        var $DOM = $('#PendingTimeoutMinutes');
                        var $DOMAjaxSave = $DOM.next('.ajaxSave');
                        $DOM
                    .watermark('Minutes')
                    .focus(function () { $DOM.select() })
                    .keydown(function (e) {
                        $DOMAjaxSave.show();
                        if (e.which == 13) {
                            $(this).blur();
                        }
                    }).blur(function () {
                        $DOMAjaxSave.hide();
                    })
                    .change(function () {
                        $DOMAjaxSave.hide();
                        var $ajaxLoading = $DOMAjaxSave.next('.ajaxLoading').show();
                        var data = { PendingTimeoutMinutes: parseInt($DOM.val()) };
                        if (data.PendingTimeoutMinutes <= 0) {
                            alert('Pending Timeout must be greater than zero');
                            $ajaxLoading.hide();
                            return;
                        } else {
                        $.ajax({
                            url: '");

            
            #line 48 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                             Write(Url.Action(MVC.API.Enrolment.PendingTimeoutMinutes()));

            
            #line default
            #line hidden
WriteLiteral(@"',
                            dataType: 'json',
                            method: 'POST',
                            data: data,
                            success: function (d) {
                                if (d == 'OK') {
                                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                                } else {
                                    $ajaxLoading.hide();
                                    alert('Unable to update pending timeout: ' + d);
                                }
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert('Unable to update pending timeout: ' + textStatus);
                                $ajaxLoading.hide();
                            }
                        });
                        }
                    });
                    });
                    </script>
");

            
            #line 69 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                }
                else
                {
                    
            
            #line default
            #line hidden
            
            #line 72 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(TimeSpan.FromMinutes(Model.PendingTimeoutMinutes));

            
            #line default
            #line hidden
            
            #line 72 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                                                      
                }

            
            #line default
            #line hidden
WriteLiteral("            </td>\r\n        </tr>\r\n        <tr>\r\n            <td");

WriteLiteral(" colspan=\"2\"");

WriteLiteral(">\r\n                <span");

WriteLiteral(" class=\"smallText\"");

WriteLiteral(@">
                    If a device enrolment is not automatically approved it will remain pending until the timeout is reached.
                    Pending enrolments can be approved manually from the Enrolment Status page.
                </span>
            </td>
        </tr>
    </table>
</div>
<div");

WriteLiteral(" class=\"form\"");

WriteLiteral(" style=\"width: 530px; margin-top: 15px\"");

WriteLiteral(">\r\n    <h2>Apple Mac Secure Enroll</h2>\r\n    <table>\r\n        <tr>\r\n            <" +
"th>\r\n                Username:\r\n            </th>\r\n            <td>\r\n");

            
            #line 94 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                
            
            #line default
            #line hidden
            
            #line 94 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                 if (canConfig)
                {
                    
            
            #line default
            #line hidden
            
            #line 96 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(Html.TextBoxFor(model => model.MacSshUsername));

            
            #line default
            #line hidden
            
            #line 96 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                                                   
                    
            
            #line default
            #line hidden
            
            #line 97 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(AjaxHelpers.AjaxSave());

            
            #line default
            #line hidden
            
            #line 97 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                           
                    
            
            #line default
            #line hidden
            
            #line 98 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(AjaxHelpers.AjaxLoader());

            
            #line default
            #line hidden
            
            #line 98 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                             

            
            #line default
            #line hidden
WriteLiteral("                    <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
                    $(function () {
                        var $DOM = $('#MacSshUsername');
                        var $DOMAjaxSave = $DOM.next('.ajaxSave');
                        $DOM
                    .watermark('Username')
                    .focus(function () { $DOM.select() })
                    .keydown(function (e) {
                        $DOMAjaxSave.show();
                        if (e.which == 13) {
                            $(this).blur();
                        }
                    }).blur(function () {
                        $DOMAjaxSave.hide();
                    })
                    .change(function () {
                        $DOMAjaxSave.hide();
                        var $ajaxLoading = $DOMAjaxSave.next('.ajaxLoading').show();
                        var data = { MacSshUsername: $DOM.val() };
                        $.ajax({
                            url: '");

            
            #line 119 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                             Write(Url.Action(MVC.API.Bootstrapper.MacSshUsername()));

            
            #line default
            #line hidden
WriteLiteral(@"',
                            dataType: 'json',
                            data: data,
                            success: function (d) {
                                if (d == 'OK') {
                                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                                } else {
                                    $ajaxLoading.hide();
                                    alert('Unable to update Username: ' + d);
                                }
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert('Unable to update Username: ' + textStatus);
                                $ajaxLoading.hide();
                            }
                        });
                    });
                    });
                    </script>
");

            
            #line 138 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                }
                else
                {
                    if (string.IsNullOrEmpty(Model.MacSshUsername))
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <span");

WriteLiteral(" class=\"smallMessage\"");

WriteLiteral(">&lt;None Specified&gt;</span>\r\n");

            
            #line 144 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                    }
                    else
                    {
                        
            
            #line default
            #line hidden
            
            #line 147 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                   Write(Model.MacSshUsername);

            
            #line default
            #line hidden
            
            #line 147 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                             
                    }
                }

            
            #line default
            #line hidden
WriteLiteral("            </td>\r\n        </tr>\r\n\r\n        <tr>\r\n            <th>\r\n             " +
"   Password:\r\n            </th>\r\n            <td>\r\n");

            
            #line 158 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                
            
            #line default
            #line hidden
            
            #line 158 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                 if (canConfig)
                {

            
            #line default
            #line hidden
WriteLiteral("                    <input");

WriteLiteral(" id=\"MacSshPassword\"");

WriteLiteral(" type=\"password\"");

WriteLiteral(" />\r\n");

            
            #line 161 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 161 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(AjaxHelpers.AjaxSave());

            
            #line default
            #line hidden
            
            #line 161 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                           
                    
            
            #line default
            #line hidden
            
            #line 162 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
               Write(AjaxHelpers.AjaxLoader());

            
            #line default
            #line hidden
            
            #line 162 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                             

            
            #line default
            #line hidden
WriteLiteral("                    <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
                    $(function () {
                        var $DOM = $('#MacSshPassword');
                        var $DOMAjaxSave = $DOM.next('.ajaxSave');
                        $DOM
                    .watermark('Password')
                    .focus(function () { $DOM.select() })
                    .keydown(function (e) {
                        $DOMAjaxSave.show();
                        if (e.which == 13) {
                            $(this).blur();
                        }
                    }).blur(function () {
                        $DOMAjaxSave.hide();
                    })
                    .change(function () {
                        $DOMAjaxSave.hide();
                        var $ajaxLoading = $DOMAjaxSave.next('.ajaxLoading').show();
                        var data = { MacSshPassword: $DOM.val() };
                        $.ajax({
                            url: '");

            
            #line 183 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                             Write(Url.Action(MVC.API.Bootstrapper.MacSshPassword()));

            
            #line default
            #line hidden
WriteLiteral(@"',
                            dataType: 'json',
                            data: data,
                            success: function (d) {
                                if (d == 'OK') {
                                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                                } else {
                                    $ajaxLoading.hide();
                                    alert('Unable to update Password: ' + d);
                                }
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert('Unable to update Password: ' + textStatus);
                                $ajaxLoading.hide();
                            }
                        });
                    });
                    });
                    </script>
");

            
            #line 202 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                }
                else
                {

            
            #line default
            #line hidden
WriteLiteral("                    ");

WriteLiteral("********");

WriteLiteral("\r\n");

            
            #line 206 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("            </td>\r\n        </tr>\r\n        <tr>\r\n            <td");

WriteLiteral(" colspan=\"2\"");

WriteLiteral(">\r\n                <span");

WriteLiteral(" class=\"smallText\"");

WriteLiteral(">\r\n                    <strong>Instructions:</strong> The above credentials must " +
"be\r\n                    able to connect to the requesting Apple Mac client via <" +
"a");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(" href=\"http://en.wikipedia.org/wiki/Secure_Shell\"");

WriteLiteral(">SSH</a>. Enter/Script the following command:\r\n                </span>\r\n         " +
"       <div");

WriteLiteral(" class=\"code\"");

WriteLiteral(">\r\n                    curl&nbsp;<a");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(" href=\"http://disco:9292/Services/Client/Unauthenticated/MacSecureEnrol\"");

WriteLiteral(">http://disco:9292/Services/Client/Unauthenticated/MacSecureEnrol</a>\r\n          " +
"      </div>\r\n                <span");

WriteLiteral(" class=\"smallText\"");

WriteLiteral(">This url will return a <a");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(" href=\"http://json.org/\"");

WriteLiteral(">JSON</a> response containing basic information about the enrolment.</span><br />" +
"\r\n                <span");

WriteLiteral(" class=\"smallMessage\"");

WriteLiteral(">\r\n                    This command makes use of <a");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(" href=\"http://curl.haxx.se/\"");

WriteLiteral(">cURL</a> (bundled with OSX). Other methods can also trigger a Mac Secure Enroll," +
"\r\n                    such as an anchor (<span");

WriteLiteral(" class=\"code\"");

WriteLiteral(">&lt;a&gt;</span>) or <span");

WriteLiteral(" class=\"code\"");

WriteLiteral(">&lt;script&gt;</span>\r\n                    tag embedded on the organisation\'s in" +
"tranet.\r\n                </span>\r\n            </td>\r\n        </tr>\r\n    </table>" +
"\r\n</div>\r\n");

            
            #line 228 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
 if (canShowStatus && Authorization.Has(Claims.Config.Logging.Show))
{

            
            #line default
            #line hidden
WriteLiteral("    <h2>Live Enrolment Logging</h2>\r\n");

            
            #line 231 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
    
            
            #line default
            #line hidden
            
            #line 231 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
Write(Html.Partial(MVC.Config.Shared.Views.LogEvents, new Disco.Web.Areas.Config.Models.Shared.LogEventsModel()
{
    IsLive = true,
    TakeFilter = 100,
    StartFilter = DateTime.Today.AddDays(-1),
    ModuleFilter = Disco.Services.Devices.Enrolment.EnrolmentLog.Current,
    ViewPortHeight = 250
}));

            
            #line default
            #line hidden
            
            #line 238 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
  
}

            
            #line default
            #line hidden
WriteLiteral("<div");

WriteLiteral(" class=\"actionBar\"");

WriteLiteral(">\r\n");

            
            #line 241 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
    
            
            #line default
            #line hidden
            
            #line 241 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
     if (Authorization.Has(Claims.Config.Enrolment.DownloadBootstrapper))
    {
        
            
            #line default
            #line hidden
            
            #line 243 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
   Write(Html.ActionLinkButton("Download Bootstrapper", MVC.Services.Client.Bootstrapper()));

            
            #line default
            #line hidden
            
            #line 243 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                                                                           
    }

            
            #line default
            #line hidden
WriteLiteral("    ");

            
            #line 245 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
     if (canShowStatus)
    {
        
            
            #line default
            #line hidden
            
            #line 247 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
   Write(Html.ActionLinkButton("Enrolment Status", MVC.Config.Enrolment.Status()));

            
            #line default
            #line hidden
            
            #line 247 "..\..\Areas\Config\Views\Enrolment\Index.cshtml"
                                                                                 
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

        }
    }
}
#pragma warning restore 1591
