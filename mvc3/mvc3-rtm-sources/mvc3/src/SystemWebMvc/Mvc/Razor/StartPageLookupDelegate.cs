namespace System.Web.Mvc.Razor {
    using System.Collections.Generic;
    using System.Web.WebPages;

    internal delegate WebPageRenderingBase StartPageLookupDelegate(WebPageRenderingBase page, string fileName, IEnumerable<string> supportedExtensions);
}
