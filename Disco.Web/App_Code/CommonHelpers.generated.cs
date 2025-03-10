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

namespace Disco.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    
    #line 7 "..\..\App_Code\CommonHelpers.cshtml"
    using System.Web.Mvc;
    
    #line default
    #line hidden
    
    #line 8 "..\..\App_Code\CommonHelpers.cshtml"
    using System.Web.Mvc.Html;
    
    #line default
    #line hidden
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.WebPages.Html;
    
    #line 2 "..\..\App_Code\CommonHelpers.cshtml"
    using Disco;
    
    #line default
    #line hidden
    
    #line 3 "..\..\App_Code\CommonHelpers.cshtml"
    using Disco.Models.Repository;
    
    #line default
    #line hidden
    
    #line 4 "..\..\App_Code\CommonHelpers.cshtml"
    using Disco.Services;
    
    #line default
    #line hidden
    
    #line 5 "..\..\App_Code\CommonHelpers.cshtml"
    using Disco.Services.Web;
    
    #line default
    #line hidden
    
    #line 6 "..\..\App_Code\CommonHelpers.cshtml"
    using Disco.Web;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    public class CommonHelpers : System.Web.WebPages.HelperPage
    {

#line 10 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDate(DateTime d, string ElementId = null, bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {
WriteLiteralTo(__razor_helper_writer, "<span ");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, ElementId == null ? null : new HtmlString(string.Format("id=\"{0}\" ", ElementId)));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, " title=\"");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
                                                                      WriteTo(__razor_helper_writer, d.ToFullDateTime());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\" data-livestamp=\"");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                           WriteTo(__razor_helper_writer, d.ToUnixEpoc());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\" data-isodate=\"");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                          WriteTo(__razor_helper_writer, d.ToISO8601());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\" class=\"date nowrap");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                                             WriteTo(__razor_helper_writer, WithoutSuffix ? " noMomentSuffix" : null);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\">");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                                                                                         WriteTo(__razor_helper_writer, d.ToFullDateTime());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>");


#line 11 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                                                                                                                                                 

#line default
#line hidden
});

#line 11 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 12 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDate(DateTime? d, string NullValue = "n/a", string ElementId = null, bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {
WriteLiteralTo(__razor_helper_writer, "<span ");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, ElementId == null ? null : new HtmlString(string.Format("id=\"{0}\" ", ElementId)));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, " title=\"");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
                                                                      WriteTo(__razor_helper_writer, d.ToFullDateTime(NullValue));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\" data-livestamp=\"");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                    WriteTo(__razor_helper_writer, d.ToUnixEpoc());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\" data-isodate=\"");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                   WriteTo(__razor_helper_writer, d.ToISO8601());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\" class=\"date nowrap");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                                                      WriteTo(__razor_helper_writer, WithoutSuffix ? " noMomentSuffix" : null);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\">");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                                                                                                  WriteTo(__razor_helper_writer, d.ToFullDateTime(NullValue));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>");


#line 13 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                                                                                                                                                                                                   

#line default
#line hidden
});

#line 13 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 14 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDateAndUser(DateTime? d, User u, string DateNullValue = "n/a", bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 15 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 16 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyDate(d, DateNullValue, WithoutSuffix: WithoutSuffix));


#line default
#line hidden

#line 16 "..\..\App_Code\CommonHelpers.cshtml"
                                                                 ;
    

#line default
#line hidden

#line 17 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyUser(u, null, " by"));


#line default
#line hidden

#line 17 "..\..\App_Code\CommonHelpers.cshtml"
                                 ;


#line default
#line hidden
});

#line 18 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 19 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDateAndUser(DateTime d, User u, bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 20 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 21 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyDate(d, WithoutSuffix: WithoutSuffix));


#line default
#line hidden

#line 21 "..\..\App_Code\CommonHelpers.cshtml"
                                                  ;
    

#line default
#line hidden

#line 22 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyUser(u, null, " by"));


#line default
#line hidden

#line 22 "..\..\App_Code\CommonHelpers.cshtml"
                                 ;


#line default
#line hidden
});

#line 23 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 24 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDateAndUser(DateTime? d, string UserId, string DateNullValue = "n/a", bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 25 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 26 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyDate(d, DateNullValue, WithoutSuffix: WithoutSuffix));


#line default
#line hidden

#line 26 "..\..\App_Code\CommonHelpers.cshtml"
                                                                 ;
    

#line default
#line hidden

#line 27 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyUser(UserId, null, " by"));


#line default
#line hidden

#line 27 "..\..\App_Code\CommonHelpers.cshtml"
                                      ;


#line default
#line hidden
});

#line 28 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 29 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDateAndUser(DateTime d, string UserId, bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 30 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 31 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyDate(d, WithoutSuffix: WithoutSuffix));


#line default
#line hidden

#line 31 "..\..\App_Code\CommonHelpers.cshtml"
                                                  ;
    

#line default
#line hidden

#line 32 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, FriendlyUser(UserId, null, " by"));


#line default
#line hidden

#line 32 "..\..\App_Code\CommonHelpers.cshtml"
                                      ;


#line default
#line hidden
});

#line 33 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 34 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDateAndTitleUser(DateTime? d, User u, string DateNullValue = "n/a", bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 35 "..\..\App_Code\CommonHelpers.cshtml"
 


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "    <span");

WriteAttributeTo(__razor_helper_writer, "title", Tuple.Create(" title=\"", 1906), Tuple.Create("\"", 1952)

#line 36 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 1914), Tuple.Create<System.Object, System.Int32>(d.ToFullDateTime(DateNullValue)

#line default
#line hidden
, 1914), false)
, Tuple.Create(Tuple.Create(" ", 1946), Tuple.Create("by", 1947), true)

#line 36 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create(" ", 1949), Tuple.Create<System.Object, System.Int32>(u

#line default
#line hidden
, 1950), false)
);

WriteLiteralTo(__razor_helper_writer, " data-livestamp=\"");


#line 36 "..\..\App_Code\CommonHelpers.cshtml"
                                           WriteTo(__razor_helper_writer, d.ToUnixEpoc());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\"");

WriteAttributeTo(__razor_helper_writer, "class", Tuple.Create(" class=\"", 1986), Tuple.Create("\"", 2048)
, Tuple.Create(Tuple.Create("", 1994), Tuple.Create("date", 1994), true)
, Tuple.Create(Tuple.Create(" ", 1998), Tuple.Create("nowrap", 1999), true)

#line 36 "..\..\App_Code\CommonHelpers.cshtml"
                             , Tuple.Create(Tuple.Create("", 2005), Tuple.Create<System.Object, System.Int32>(WithoutSuffix ? " noMomentSuffix" : null

#line default
#line hidden
, 2005), false)
);

WriteLiteralTo(__razor_helper_writer, ">");


#line 36 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                                           WriteTo(__razor_helper_writer, d.ToFullDateTime(DateNullValue));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 37 "..\..\App_Code\CommonHelpers.cshtml"


#line default
#line hidden
});

#line 37 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 38 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyDateAndTitleUser(DateTime d, User u, bool WithoutSuffix = false)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 39 "..\..\App_Code\CommonHelpers.cshtml"
 


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "    <span");

WriteAttributeTo(__razor_helper_writer, "title", Tuple.Create(" title=\"", 2188), Tuple.Create("\"", 2221)

#line 40 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 2196), Tuple.Create<System.Object, System.Int32>(d.ToFullDateTime()

#line default
#line hidden
, 2196), false)
, Tuple.Create(Tuple.Create(" ", 2215), Tuple.Create("by", 2216), true)

#line 40 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create(" ", 2218), Tuple.Create<System.Object, System.Int32>(u

#line default
#line hidden
, 2219), false)
);

WriteLiteralTo(__razor_helper_writer, " data-livestamp=\"");


#line 40 "..\..\App_Code\CommonHelpers.cshtml"
                              WriteTo(__razor_helper_writer, d.ToUnixEpoc());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\"");

WriteAttributeTo(__razor_helper_writer, "class", Tuple.Create(" class=\"", 2255), Tuple.Create("\"", 2317)
, Tuple.Create(Tuple.Create("", 2263), Tuple.Create("date", 2263), true)
, Tuple.Create(Tuple.Create(" ", 2267), Tuple.Create("nowrap", 2268), true)

#line 40 "..\..\App_Code\CommonHelpers.cshtml"
                , Tuple.Create(Tuple.Create("", 2274), Tuple.Create<System.Object, System.Int32>(WithoutSuffix ? " noMomentSuffix" : null

#line default
#line hidden
, 2274), false)
);

WriteLiteralTo(__razor_helper_writer, ">");


#line 40 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                              WriteTo(__razor_helper_writer, d.ToFullDateTime());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 41 "..\..\App_Code\CommonHelpers.cshtml"


#line default
#line hidden
});

#line 41 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 42 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyUser(User u, string nullValue = null, string prepend = null)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 43 "..\..\App_Code\CommonHelpers.cshtml"
 
    if (u != null)
    {
        

#line default
#line hidden

#line 46 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, prepend);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, " <span");

WriteAttributeTo(__razor_helper_writer, "title", Tuple.Create(" title=\"", 2480), Tuple.Create("\"", 2490)

#line 46 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 2488), Tuple.Create<System.Object, System.Int32>(u

#line default
#line hidden
, 2488), false)
);

WriteLiteralTo(__razor_helper_writer, ">");


#line 46 "..\..\App_Code\CommonHelpers.cshtml"
    WriteTo(__razor_helper_writer, u.FriendlyId());


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 47 "..\..\App_Code\CommonHelpers.cshtml"
    }
    else
    {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "        <span>");


#line 50 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, nullValue);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 51 "..\..\App_Code\CommonHelpers.cshtml"
    }


#line default
#line hidden
});

#line 52 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 53 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult FriendlyUser(string UserId, string nullValue = null, string prepend = null)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 54 "..\..\App_Code\CommonHelpers.cshtml"
 
    if (UserId != null)
    {
        

#line default
#line hidden

#line 57 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, prepend);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, " <span>");


#line 57 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, Disco.Services.Interop.ActiveDirectory.ActiveDirectory.FriendlyAccountId(UserId));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 58 "..\..\App_Code\CommonHelpers.cshtml"
    }
    else
    {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "        <span>");


#line 61 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, nullValue);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 62 "..\..\App_Code\CommonHelpers.cshtml"
    }


#line default
#line hidden
});

#line 63 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 66 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult RadioButtonList(string id, List<System.Web.Mvc.SelectListItem> items, int columns = 1)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 67 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 68 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, ItemList(null, "radio", id, items, columns));


#line default
#line hidden

#line 68 "..\..\App_Code\CommonHelpers.cshtml"
                                                


#line default
#line hidden
});

#line 69 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 70 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult RadioButtonList(string containerName, string id, List<System.Web.Mvc.SelectListItem> items, int columns = 1)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 71 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 72 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, ItemList(containerName, "radio", id, items, columns));


#line default
#line hidden

#line 72 "..\..\App_Code\CommonHelpers.cshtml"
                                                         


#line default
#line hidden
});

#line 73 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 74 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult CheckBoxList(string id, List<System.Web.Mvc.SelectListItem> items, int columns = 1, bool alignEven = true, int? forceUniqueIds = null, bool htmlEncodeText = true)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 75 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 76 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, ItemList(null, "checkbox", id, items, columns, alignEven, forceUniqueIds, htmlEncodeText));


#line default
#line hidden

#line 76 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                              


#line default
#line hidden
});

#line 77 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 78 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult CheckBoxList(string containerName, string id, List<System.Web.Mvc.SelectListItem> items, int columns = 1, bool alignEven = true, int? forceUniqueIds = null, bool htmlEncodeText = true)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 79 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 80 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, ItemList(containerName, "checkbox", id, items, columns, alignEven, forceUniqueIds, htmlEncodeText));


#line default
#line hidden

#line 80 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                       


#line default
#line hidden
});

#line 81 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 82 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult CheckboxBulkSelect(string BulkSelectContainerId, string ParentJQuerySelector = null)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 83 "..\..\App_Code\CommonHelpers.cshtml"
 Html.GetPageHelper().BundleDeferred("~/ClientScripts/Modules/Disco-jQueryExtensions");


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "    <span");

WriteAttributeTo(__razor_helper_writer, "id", Tuple.Create(" id=\"", 3993), Tuple.Create("\"", 4020)

#line 84 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 3998), Tuple.Create<System.Object, System.Int32>(BulkSelectContainerId

#line default
#line hidden
, 3998), false)
);

WriteLiteralTo(__razor_helper_writer, " class=\"checkboxBulkSelectContainer\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n");


#line 85 "..\..\App_Code\CommonHelpers.cshtml"
        

#line default
#line hidden

#line 85 "..\..\App_Code\CommonHelpers.cshtml"
         if (string.IsNullOrWhiteSpace(ParentJQuerySelector))
        {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "            <script");

WriteLiteralTo(__razor_helper_writer, " type=\"text/javascript\"");

WriteLiteralTo(__razor_helper_writer, ">$(function () { $(\'#");


#line 87 "..\..\App_Code\CommonHelpers.cshtml"
                                  WriteTo(__razor_helper_writer, BulkSelectContainerId);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\').checkboxBulkSelect(); });</script>\r\n");


#line 88 "..\..\App_Code\CommonHelpers.cshtml"
        }
        else
        {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "            <script");

WriteLiteralTo(__razor_helper_writer, " type=\"text/javascript\"");

WriteLiteralTo(__razor_helper_writer, ">$(function () { $(\'#");


#line 91 "..\..\App_Code\CommonHelpers.cshtml"
                                  WriteTo(__razor_helper_writer, BulkSelectContainerId);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\').checkboxBulkSelect({ parentSelector: \'");


#line 91 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                   WriteTo(__razor_helper_writer, ParentJQuerySelector);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\' }); });</script>\r\n");


#line 92 "..\..\App_Code\CommonHelpers.cshtml"
        }


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "    </span>\r\n");


#line 94 "..\..\App_Code\CommonHelpers.cshtml"


#line default
#line hidden
});

#line 94 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 95 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult ItemList(string containerId, string inputType, string id, List<System.Web.Mvc.SelectListItem> items, int columns = 1, bool alignEven = true, int? forceUniqueIds = null, bool htmlEncodeText = true)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 96 "..\..\App_Code\CommonHelpers.cshtml"
 
    int itemsPerColumn = items.Count / columns;
    int columnWidth = (100 / columns);
    var itemNextId = 0;


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "    <table");

WriteAttributeTo(__razor_helper_writer, "id", Tuple.Create(" id=\"", 4827), Tuple.Create("\"", 4844)

#line 100 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 4832), Tuple.Create<System.Object, System.Int32>(containerId

#line default
#line hidden
, 4832), false)
);

WriteLiteralTo(__razor_helper_writer, " class=\"none\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n        <tr>\r\n");


#line 102 "..\..\App_Code\CommonHelpers.cshtml"
            

#line default
#line hidden

#line 102 "..\..\App_Code\CommonHelpers.cshtml"
             for (int i = 0; i < columns; i++)
            {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "                <td ");


#line 104 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, alignEven ? new HtmlString(string.Format(" style=\"width: {0}%\"", columnWidth)) : new HtmlString(string.Empty));


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, ">\r\n                    <ul");

WriteLiteralTo(__razor_helper_writer, " class=\"none\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n");


#line 106 "..\..\App_Code\CommonHelpers.cshtml"
                        

#line default
#line hidden

#line 106 "..\..\App_Code\CommonHelpers.cshtml"
                          
                            int itemsForThisColumn = itemsPerColumn + (items.Count % columns > i ? 1 : 0);
                            for (int i2 = 0; i2 < itemsForThisColumn && itemNextId < items.Count; i2++)
                            {
                                var item = items[itemNextId];
                                itemNextId++;
                                var itemId = forceUniqueIds.HasValue ? string.Format("{0}_{1}_{2}", id, item.Value, forceUniqueIds++) : string.Format("{0}_{1}", id, item.Value);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "                                <li>\r\n                                    <input");

WriteAttributeTo(__razor_helper_writer, "id", Tuple.Create(" id=\"", 5755), Tuple.Create("\"", 5767)

#line 114 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 5760), Tuple.Create<System.Object, System.Int32>(itemId

#line default
#line hidden
, 5760), false)
);

WriteAttributeTo(__razor_helper_writer, "name", Tuple.Create(" name=\"", 5768), Tuple.Create("\"", 5778)

#line 114 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 5775), Tuple.Create<System.Object, System.Int32>(id

#line default
#line hidden
, 5775), false)
);

WriteAttributeTo(__razor_helper_writer, "value", Tuple.Create(" value=\"", 5779), Tuple.Create("\"", 5798)

#line 114 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 5787), Tuple.Create<System.Object, System.Int32>(item.Value

#line default
#line hidden
, 5787), false)
);

WriteAttributeTo(__razor_helper_writer, "type", Tuple.Create(" type=\"", 5799), Tuple.Create("\"", 5816)

#line 114 "..\..\App_Code\CommonHelpers.cshtml"
             , Tuple.Create(Tuple.Create("", 5806), Tuple.Create<System.Object, System.Int32>(inputType

#line default
#line hidden
, 5806), false)
);

WriteLiteralTo(__razor_helper_writer, " ");


#line 114 "..\..\App_Code\CommonHelpers.cshtml"
                                                                            WriteTo(__razor_helper_writer, item.Selected ? new HtmlString("checked=\"checked\" ") : null);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, " /><label");

WriteAttributeTo(__razor_helper_writer, "for", Tuple.Create(" for=\"", 5891), Tuple.Create("\"", 5904)

#line 114 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                                        , Tuple.Create(Tuple.Create("", 5897), Tuple.Create<System.Object, System.Int32>(itemId

#line default
#line hidden
, 5897), false)
);

WriteLiteralTo(__razor_helper_writer, ">\r\n");


#line 115 "..\..\App_Code\CommonHelpers.cshtml"
                                        

#line default
#line hidden

#line 115 "..\..\App_Code\CommonHelpers.cshtml"
                                         if (htmlEncodeText)
                                        {

#line default
#line hidden

#line 116 "..\..\App_Code\CommonHelpers.cshtml"
           WriteTo(__razor_helper_writer, item.Text);


#line default
#line hidden

#line 116 "..\..\App_Code\CommonHelpers.cshtml"
                                                    }
                                    else
                                    { 

#line default
#line hidden

#line 118 "..\..\App_Code\CommonHelpers.cshtml"
         WriteTo(__razor_helper_writer, new HtmlString(item.Text));


#line default
#line hidden

#line 118 "..\..\App_Code\CommonHelpers.cshtml"
                                                                  }


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "                                    </label>\r\n                                </l" +
"i>\r\n");


#line 121 "..\..\App_Code\CommonHelpers.cshtml"
                            }
                        

#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "\r\n                    </ul>\r\n                </td>\r\n");


#line 125 "..\..\App_Code\CommonHelpers.cshtml"
            }


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "        </tr>\r\n    </table>\r\n");


#line 128 "..\..\App_Code\CommonHelpers.cshtml"


#line default
#line hidden
});

#line 128 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 131 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult Breadcrumbs(List<Tuple<string, ActionResult>> BreadCrumbs)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 132 "..\..\App_Code\CommonHelpers.cshtml"
 
    for (int index = 0; index < BreadCrumbs.Count; index++)
    {
        var breadCrumb = BreadCrumbs[index];
        if (index != 0)
        {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "            <span>&gt;</span>\r\n");


#line 139 "..\..\App_Code\CommonHelpers.cshtml"
        }
        if (breadCrumb.Item2 == null)
        {


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "            <span");

WriteAttributeTo(__razor_helper_writer, "title", Tuple.Create(" title=\"", 6710), Tuple.Create("\"", 6735)

#line 142 "..\..\App_Code\CommonHelpers.cshtml"
, Tuple.Create(Tuple.Create("", 6718), Tuple.Create<System.Object, System.Int32>(breadCrumb.Item1

#line default
#line hidden
, 6718), false)
);

WriteLiteralTo(__razor_helper_writer, ">");


#line 142 "..\..\App_Code\CommonHelpers.cshtml"
              WriteTo(__razor_helper_writer, breadCrumb.Item1);


#line default
#line hidden
WriteLiteralTo(__razor_helper_writer, "</span>\r\n");


#line 143 "..\..\App_Code\CommonHelpers.cshtml"
        }
        else
        {
            

#line default
#line hidden

#line 146 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, Html.GetPageHelper().ActionLink(breadCrumb.Item1, breadCrumb.Item2));


#line default
#line hidden

#line 146 "..\..\App_Code\CommonHelpers.cshtml"
                                                                                
        }
    }


#line default
#line hidden
});

#line 149 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 150 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult Breadcrumbs(string Title)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 151 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 152 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, Title);


#line default
#line hidden

#line 152 "..\..\App_Code\CommonHelpers.cshtml"
          


#line default
#line hidden
});

#line 153 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 154 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult BreadcrumbsTitle(List<Tuple<string, ActionResult>> BreadCrumbs)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 155 "..\..\App_Code\CommonHelpers.cshtml"
 
    for (int index = 0; index < BreadCrumbs.Count; index++)
    {
        var breadCrumb = BreadCrumbs[index];
        if (index != 0)
        {
            

#line default
#line hidden

#line 161 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, new HtmlString(" > "));


#line default
#line hidden

#line 161 "..\..\App_Code\CommonHelpers.cshtml"
                                    
        }
        

#line default
#line hidden

#line 163 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, breadCrumb.Item1);


#line default
#line hidden

#line 163 "..\..\App_Code\CommonHelpers.cshtml"
                         
    }


#line default
#line hidden
});

#line 165 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

#line 166 "..\..\App_Code\CommonHelpers.cshtml"
public static System.Web.WebPages.HelperResult BreadcrumbsTitle(string Title)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {

#line 167 "..\..\App_Code\CommonHelpers.cshtml"
 
    

#line default
#line hidden

#line 168 "..\..\App_Code\CommonHelpers.cshtml"
WriteTo(__razor_helper_writer, Title);


#line default
#line hidden

#line 168 "..\..\App_Code\CommonHelpers.cshtml"
          


#line default
#line hidden
});

#line 169 "..\..\App_Code\CommonHelpers.cshtml"
}
#line default
#line hidden

        public CommonHelpers()
        {
        }
    }
}
#pragma warning restore 1591
