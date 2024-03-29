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

namespace Disco.Web.Areas.Public.Views.Public
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
    using Disco.Models.Repository;
    using Disco.Services;
    using Disco.Services.Authorization;
    using Disco.Services.Web;
    using Disco.Web;
    using Disco.Web.Extensions;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Areas/Public/Views/Public/Credits.cshtml")]
    public partial class Credits : Disco.Services.Web.WebViewPage<dynamic>
    {
        public Credits()
        {
        }
        public override void Execute()
        {
            
            #line 1 "..\..\Areas\Public\Views\Public\Credits.cshtml"
  
    ViewBag.Title = "Credits";
    Html.BundleDeferred("~/Style/Credits");

            
            #line default
            #line hidden
WriteLiteral("\r\n<h2>\r\n    Organisations</h2>\r\n<div");

WriteLiteral(" id=\"organisationCredits\"");

WriteLiteral(">\r\n    <span");

WriteLiteral(" class=\"message\"");

WriteLiteral(">The development team would like to thank the following organisations\r\n        fo" +
"r their generous contributions:</span>\r\n    <ul>\r\n        <li><a");

WriteLiteral(" href=\"http://www.geelonghigh.vic.edu.au/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Geelong High School</a></li>\r\n        <li><a");

WriteLiteral(" href=\"http://www.bellarinesc.vic.edu.au/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Bellarine Secondary\r\n            College</a></li>\r\n    </ul>\r\n</div>\r\n<hr />\r\n<t" +
"able");

WriteLiteral(" id=\"pageMenu\"");

WriteLiteral(">\r\n    <tr>\r\n        <td>\r\n            <h2>\r\n                Platform</h2>\r\n     " +
"       <div");

WriteLiteral(" class=\"pageMenuArea MicrosoftNET\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.microsoft.com/net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft .NET Framework</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    The Microsoft .NET Framework is the hosting virtual machin" +
"e most of Disco ICT runs on.\r\n                    Most of Disco\'s components are" +
" written in <a");

WriteLiteral(" href=\"http://msdn.microsoft.com/en-us/vstudio/hh388566.aspx\"");

WriteLiteral("\r\n                        target=\"_blank\"");

WriteLiteral(">C#</a>.\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea MicrosoftASPNET\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.asp.net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft ASP.NET Framework</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    The Microsoft ASP.NET Framework powers all Web features of" +
" this web application.\r\n                    <a");

WriteLiteral(" href=\"http://www.nuget.org/packages/Microsoft.Web.Optimization\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft\r\n                        Web Optimization - Bundling</a> is used to pr" +
"ovide JavaScript, CSS and LESS\r\n                    minification and bundling.\r\n" +
"                </div>\r\n                <a");

WriteLiteral(" href=\"http://www.asp.net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft ASP.NET MVC Framework</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    The Microsoft ASP.NET MVC Framework providers the Model-View-Controller pattern
                    for ASP.NET which is implemented by this web application. Most of this web application's
                    views are written in <a");

WriteLiteral(" href=\"http://www.asp.net/web-pages/tutorials/basics/2-introduction-to-asp-net-we" +
"b-programming-using-the-razor-syntax\"");

WriteLiteral("\r\n                        target=\"_blank\"");

WriteLiteral(">C# Razor Syntax</a>. Web application start-up time is increase\r\n                " +
"    by pre-compiling all Razor views with <a");

WriteLiteral(" href=\"http://razorgenerator.codeplex.com/\"");

WriteLiteral("\r\n                        target=\"_blank\"");

WriteLiteral(">Razor Generator</a>.\r\n                </div>\r\n                <a");

WriteLiteral(" href=\"http://www.asp.net/entity-framework\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft .NET Entity\r\n                    Framework</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    The Microsoft .NET Entity Framework is the Object-Relation" +
"al Mapping (ORM) toolset\r\n                    used by this web application.\r\n   " +
"             </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea MicrosoftSQLServer\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.microsoft.com/sqlserver/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft SQL Server</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    Microsoft SQL Server is used for storage and querying of r" +
"elational data.\r\n                </div>\r\n                <a");

WriteLiteral(" href=\"http://msdn.microsoft.com/en-us/data/ff687142\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft SQL\r\n                    Server Compact</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    Microsoft SQL Server Compact provides file-based relational data storage. It is
                    used by this web application to store all logs and is available for plug-ins to
                    use for additional storage.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea MicrosoftSilverlight\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.silverlight.net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Microsoft Silverlight</a>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    Microsoft Silverlight is an application framework for writing and running rich Internet
                    applications. The run-time environment for Silverlight is available as a plug-in
                    for web browsers running under Microsoft Windows and Mac OS X. Silverlight supports
                    multimedia, graphics and animation, and give developers support for CLI languages
                    and development tools.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea SignalR\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://signalr.net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">SignalR</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.opensource.org/licenses/mit-license.php" +
"\" target=\"_blank\">MIT</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    SignalR is used extensively by this web application to provide real-time feedback
                    to the client browser. This includes real-time log viewing, enrolment status, document
                    import status and noticeboards.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea nuget\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://nuget.org/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">nuget</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.apache.org/licenses/LICENSE-2.0.html\"");

WriteLiteral("\r\n                    target=\"_blank\"");

WriteLiteral(">Apache License, Version 2.0</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    NuGet is a Visual Studio extension that makes it easy to i" +
"nstall and update third-party\r\n                    libraries and tools in Visual" +
" Studio.\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea T4MVC\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://t4mvc.codeplex.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">T4MVC</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.apache.org/licenses/LICENSE-2.0.html\" t" +
"arget=\"_blank\">Apache License,\r\n                    Version 2.0</a></span>\r\n    " +
"            <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    T4MVC is a T4 template for ASP.NET MVC apps that creates strongly typed helpers
                    that eliminate the use of literal strings when referring the controllers, actions
                    and views.
                </div>
            </div>
        </td>
        <td>
            <h2>
                SDK/Helpers</h2>
            <div");

WriteLiteral(" class=\"pageMenuArea dotless\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.dotlesscss.org/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">.less</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.apache.org/licenses/LICENSE-2.0.html\" t" +
"arget=\"_blank\">Apache License,\r\n                    Version 2.0</a></span>\r\n    " +
"            <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    .less is a Microsoft .NET Framework port of the popular <a" +
"");

WriteLiteral(" href=\"http://lesscss.org/\"");

WriteLiteral("\r\n                        target=\"_blank\"");

WriteLiteral(@">LESS JavaScript library</a>. LESS syntax adds features to the
                    Cascading StyleSheet specification for developers to take advantage of. It is compiled
                    to CSS for the client browser to consume.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea SpringNET\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.springframework.net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Spring.net</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.apache.org/licenses/LICENSE-2.0.html\" t" +
"arget=\"_blank\">Apache License,\r\n                    Version 2.0</a></span>\r\n    " +
"            <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    Spring.NET is an open source application framework that ma" +
"kes building enterprise\r\n                    .NET applications easier. In partic" +
"ular, this application makes use of <a");

WriteLiteral(" href=\"http://www.springframework.net/doc-latest/reference/html/expressions.html\"" +
"");

WriteLiteral("\r\n                        target=\"_blank\"");

WriteLiteral(">Spring Expression Evaluation</a>.\r\n                </div>\r\n            </div>\r\n " +
"           <div");

WriteLiteral(" class=\"pageMenuArea Quartz\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://quartznet.sourceforge.net/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Quartz.NET</a><span\r\n                    class=\"licence\"><a");

WriteLiteral(" href=\"http://www.apache.org/licenses/LICENSE-2.0.html\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Apache\r\n                        License, Version 2.0</a></span>\r\n               " +
" <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    Quartz.NET is a full-featured, open source job scheduling system that can be used
                    from smallest apps to large scale enterprise systems. Quartz.NET is a pure .NET
                    library written in C# and is a port of very popular open source Java job scheduling
                    framework, <a");

WriteLiteral(" href=\"http://www.quartz-scheduler.org/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Quartz</a>.\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea iTextSharp\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://sourceforge.net/projects/itextsharp/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">iTextSharp</a><span\r\n                    class=\"licence\"><a");

WriteLiteral(" href=\"http://opensource.org/licenses/AGPL-3.0\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">AGPL</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    iText# (iTextSharp) is a port of the iText open source jav" +
"a library for PDF generation\r\n                    written entirely in C# for the" +
" .NET platform.\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea CrystalIcons\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://everaldo.com/crystal/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Crystal Project Icons</a><span\r\n                    class=\"licence\"><a");

WriteLiteral(" href=\"http://opensource.org/licenses/LGPL-2.1\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">LGPLv2.1</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    The Crystal Project produces a set of icons targeted towar" +
"ds Linux based operating\r\n                    system distributions.\r\n           " +
"     </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea JsonNET\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://json.codeplex.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Json.NET</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.opensource.org/licenses/mit-license.php" +
"\" target=\"_blank\">MIT</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    While in most places the (Microsoft .NET Framework) built-in JSON Serializer is
                    used, however on occasion (and where other frameworks require) Json.NET is used.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea LibTiff\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://bitmiracle.com/libtiff/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">LibTiff.Net</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://bitmiracle.com/libtiff/help/license-and-cop" +
"yright.aspx\" target=\"_blank\">Copyright</a>\r\n                    | <a");

WriteLiteral(" href=\"http://opensource.org/licenses/BSD-3-Clause\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">New BSD</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    LibTiff.Net provides support for the Tag Image File Format" +
" (TIFF), a widely used\r\n                    format for storing image data.\r\n    " +
"            </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea DotNetZip\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://dotnetzip.codeplex.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">DotNetZip</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://opensource.org/licenses/MS-PL\" target=\"_bla" +
"nk\">Ms-PL</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    DotNetZip is an easy-to-use, FAST, FREE class library and " +
"toolset for manipulating\r\n                    zip files or folders.\r\n           " +
"     </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea SharpSSH\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.tamirgal.com/blog/page/SharpSSH.aspx\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">SharpSSH</a><span\r\n                    class=\"licence\"><a");

WriteLiteral(" href=\"http://www.jcraft.com/jsch/LICENSE.txt\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">BSD-Style</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    SharpSSH is a pure .NET implementation of the SSH2 client protocol suite. It provides
                    an API for communication with SSH servers and can be integrated into any .NET application.
                    The library is a C# port of the <a");

WriteLiteral(" href=\"http://www.jcraft.com/jsch/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">\r\n                        JSch</a> project from JCraft Inc.\r\n                </d" +
"iv>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea ZXing\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://code.google.com/p/zxing/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">ZXing</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.apache.org/licenses/LICENSE-2.0.html\" t" +
"arget=\"_blank\">Apache License,\r\n                    Version 2.0</a></span>\r\n    " +
"            <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    ZXing (pronounced ""zebra crossing"") is an open-source, multi-format 1D/2D barcode
                    image processing library implemented in Java, with ports to other languages.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea HtmlAgilityPack\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://htmlagilitypack.codeplex.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Html Agility Pack</a><span\r\n                    class=\"licence\"><a");

WriteLiteral(" href=\"http://opensource.org/licenses/MS-PL\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Ms-PL</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    This is an agile HTML parser that builds a read/write DOM and supports plain XPATH
                    or XSLT. It is a .NET code library that allows you to parse ""out of the web"" HTML
                    files. The parser is very tolerant with ""real world"" malformed HTML.
                </div>
            </div>
        </td>
        <td>
            <h2>
                Web Client</h2>
            <div");

WriteLiteral(" class=\"pageMenuArea jQuery\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://jquery.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">jQuery</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/mit-license.php\"");

WriteLiteral("\r\n                    target=\"_blank\"");

WriteLiteral(">MIT</a>/<a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/GPL-2.0\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">GPLv2</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    jQuery is used extensively by this web application to improve browser compatibility
                    and speed up development by providing query mechanisms for the browsers document
                    object model (DOM).
                </div>
                <div");

WriteLiteral(" class=\"pageMenuAreaSub\"");

WriteLiteral(">\r\n                    <h3>\r\n                        Plugins:</h3>\r\n             " +
"       <div>\r\n                        <a");

WriteLiteral(" href=\"http://github.com/jquery/jquery-color#readme\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Color</a>\r\n                        - The main purpose of this plugin to animate " +
"color properties on elements using\r\n                        jQuery\'s .animate() " +
"<span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/mit-license.php\"");

WriteLiteral("\r\n                            target=\"_blank\"");

WriteLiteral(">MIT</a>/<a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/GPL-3.0\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">GPL</a></span>\r\n                    </div>\r\n");

WriteLiteral("\r\n                    <div>\r\n                        <a");

WriteLiteral(" href=\"http://www.timdown.co.uk/jshashtable/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">jshashtable</a>\r\n                        - jshashtable is a JavaScript implement" +
"ation of a hash table. It associates objects\r\n                        (\"keys\") w" +
"ith other objects (\"values\"). <span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.apache.org/licenses/LICENSE-2.0.html\"");

WriteLiteral("\r\n                            target=\"_blank\"");

WriteLiteral(">Apache License, Version 2.0</a></span>\r\n                    </div>\r\n            " +
"        <div>\r\n                        <a");

WriteLiteral(" href=\"http://code.google.com/p/jquery-numberformatter/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Number Formatter</a>\r\n                        - This plugin is a number formatti" +
"ng and parsing plugin for jQuery. <span");

WriteLiteral(" class=\"licence\"");

WriteLiteral(">\r\n                            <a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/mit-license.php\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">MIT</a></span>\r\n                    </div>\r\n                    <div>\r\n         " +
"               <a");

WriteLiteral(" href=\"http://code.google.com/p/jquery-watermark/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Watermark</a>\r\n                        - This simple-to-use jQuery plugin adds w" +
"atermark capability to HTML input and textarea\r\n                        elements" +
". <span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/mit-license.php\"");

WriteLiteral("\r\n                            target=\"_blank\"");

WriteLiteral(">MIT</a>/<a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/GPL-2.0\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">GPL2</a></span>\r\n                    </div>\r\n                    <div>\r\n        " +
"                <a");

WriteLiteral(" href=\"http://bassistance.de/jquery-plugins/jquery-plugin-validation/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">\r\n                            Validation</a> - This jQuery plugin makes simple c" +
"lient-side form validation\r\n                        trivial, while offering lots" +
" of option for customization. <span");

WriteLiteral(" class=\"licence\"");

WriteLiteral(@"><a
                            href=""http://www.opensource.org/licenses/mit-license.php"" target=""_blank"">MIT</a>/<a
                                href=""http://www.opensource.org/licenses/GPL-3.0"" target=""_blank"">GPL</a></span>
                    </div>
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea jQueryUI\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://jqueryui.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">jQuery UI</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.opensource.org/licenses/mit-license.php" +
"\" target=\"_blank\">MIT</a>/<a\r\n                        href=\"http://www.opensourc" +
"e.org/licenses/GPL-2.0\" target=\"_blank\">GPL2</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    jQuery UI is used extensively by this web application to p" +
"rovide in-browser user\r\n                    interface widgets such as dialogs, d" +
"ate pickers and auto-complete drop-down menus.\r\n                </div>\r\n        " +
"        <div");

WriteLiteral(" class=\"pageMenuAreaSub\"");

WriteLiteral(">\r\n                    <h3>\r\n                        Plugins:</h3>\r\n             " +
"       <div>\r\n                        <a");

WriteLiteral(" href=\"http://code.google.com/p/dynatree/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Dynatree</a> - Dynatree\r\n                        is a jQuery plugin that allows " +
"the creation of dynamic html tree view controls using\r\n                        J" +
"avaScript. <span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/mit-license.php\"");

WriteLiteral("\r\n                            target=\"_blank\"");

WriteLiteral(">MIT</a></span>\r\n                    </div>\r\n                    <div>\r\n         " +
"               <a");

WriteLiteral(" href=\"http://isotope.metafizzy.co/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Isotope</a> - Isotope is\r\n                        a jQuery plugin which provides" +
" dynamic layout and transition functionality. <span\r\n                           " +
" class=\"licence\"><a");

WriteLiteral(" href=\"http://isotope.metafizzy.co/docs/license.html\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">\r\n                                non-commercial</a></span>\r\n                   " +
" </div>\r\n                    <div>\r\n                        <a");

WriteLiteral(" href=\"http://code.google.com/p/jquery-timepicker/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">TimePicker</a>\r\n                        - jQuery plugin that replaces a single t" +
"ext input with a set of pulldown menus to\r\n                        select hour, " +
"minute, and am/pm. <span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a");

WriteLiteral(" href=\"http://www.opensource.org/licenses/mit-license.php\"");

WriteLiteral("\r\n                            target=\"_blank\"");

WriteLiteral(">MIT</a></span>\r\n                    </div>\r\n                </div>\r\n            " +
"</div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea Modernizr\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://modernizr.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Modernizr</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.opensource.org/licenses/mit-license.php" +
"\" target=\"_blank\">MIT</a>/<a\r\n                        href=\"http://modernizr.com" +
"/license/\" target=\"_blank\">BSD-Style</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    Modernizr is an open-source JavaScript library that helps " +
"you build the next generation\r\n                    of HTML5 and CSS3-powered web" +
"sites.\r\n                </div>\r\n            </div>\r\n            <div");

WriteLiteral(" class=\"pageMenuArea knockoutjs\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://knockoutjs.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Knockout.js</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.opensource.org/licenses/mit-license.php" +
"\" target=\"_blank\">MIT</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    Knockout.js is used by this web application to provider Model-View-Controller (MVC)
                    patterns to the client browser. It enables advanced dynamic layouts such as the
                    real-time enrolment or document import status.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea HighchartsJS\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.highcharts.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Highcharts JS</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://creativecommons.org/licenses/by-nc/3.0/\" ta" +
"rget=\"_blank\">CC 3.0 Attrib-NonCommercial</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(">\r\n                    Highcharts JS is used by this web application to display i" +
"n-browser dynamic charts.\r\n                </div>\r\n            </div>\r\n         " +
"   <div");

WriteLiteral(" class=\"pageMenuArea TinyMCE\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.tinymce.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">TinyMCE</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://opensource.org/licenses/LGPL-2.1\" target=\"_" +
"blank\">LGPLv2.1</a></span>\r\n                <div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    TinyMCE is a platform independent web based JavaScript HTML WYSIWYG editor control.
                    TinyMCE has the ability to convert HTML TEXTAREA fields or other HTML elements to
                    editor instances.
                </div>
            </div>
            <div");

WriteLiteral(" class=\"pageMenuArea Shadowboxjs\"");

WriteLiteral(">\r\n                <a");

WriteLiteral(" href=\"http://www.shadowbox-js.com/\"");

WriteLiteral(" target=\"_blank\"");

WriteLiteral(">Shadowbox.js</a><span");

WriteLiteral(" class=\"licence\"");

WriteLiteral("><a\r\n                    href=\"http://www.shadowbox-js.com/LICENSE\" target=\"_blan" +
"k\">Non-Commercial License\r\n                    v1.0</a></span>\r\n                " +
"<div");

WriteLiteral(" class=\"pageMenuBlurb\"");

WriteLiteral(@">
                    Shadowbox is a web-based media viewer application that supports all of the web's
                    most popular media publishing formats. Shadowbox can showcase a wide assortment
                    of media in all major browsers without navigating users away from the linking page.
                </div>
            </div>
        </td>
    </tr>
</table>
");

        }
    }
}
#pragma warning restore 1591
