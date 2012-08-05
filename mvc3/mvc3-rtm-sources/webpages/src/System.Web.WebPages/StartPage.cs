﻿using System.Collections.Generic;
using System.Globalization;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages {

    /// <summary>
    /// Wrapper class to be used by _pagestart.cshtml files to call into
    /// the actual page.
    /// Most of the properties and methods just delegate the call to ChildPage.XXX
    /// </summary>
    public abstract class StartPage : WebPageRenderingBase {

        public WebPageRenderingBase ChildPage {
            get;
            set;
        }

        public override HttpContextBase Context {
            get {
                return ChildPage.Context;
            }
            set {
                ChildPage.Context = value;
            }
        }

        public override string Layout {
            get {
                return ChildPage.Layout;
            }
            set {
                if (value == null) {
                    ChildPage.Layout = null;
                }
                else {
                    ChildPage.Layout = NormalizeLayoutPagePath(value);
                }
            }
        }

        public override IDictionary<object, dynamic> PageData {
            get {
                return ChildPage.PageData;
            }
        }

        public override dynamic Page {
            get {
                return ChildPage.Page;
            }
        }

        internal bool RunPageCalled {
            get;
            set;
        }

        public override void ExecutePageHierarchy() {
            // Push the current pagestart on the stack. 
            TemplateStack.Push(Context, this);
            try {
                // Execute the developer-written code of the InitPage
                Execute();

                // If the child page wasn't explicitly run by the developer of the InitPage, then run it now.
                // The child page is either the next InitPage, or the final WebPage.
                if (!RunPageCalled) {
                    RunPage();
                }
            }
            finally {
                TemplateStack.Pop(Context);
            }
        }

        /// <summary>
        /// Returns either the root-most init page, or the provided page itself if no init page is found
        /// </summary>
        public static WebPageRenderingBase GetStartPage(WebPageRenderingBase page, string fileName, IEnumerable<string> supportedExtensions) {
            if (page == null) {
                throw new ArgumentNullException("page");
            }
            if (String.IsNullOrEmpty(fileName)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "fileName"), "fileName");
            }
            if (supportedExtensions == null) {
                throw new ArgumentNullException("supportedExtensions");
            }

            // Build up a list of pages to execute, such as one of the following:
            // ~/somepage.cshtml
            // ~/_pageStart.cshtml --> ~/somepage.cshtml
            // ~/_pageStart.cshtml --> ~/sub/_pageStart.cshtml --> ~/sub/somepage.cshtml
            WebPageRenderingBase currentPage = page;
            var pageDirectory = VirtualPathUtility.GetDirectory(page.VirtualPath);

            // Start with the requested page's directory, find the init page,
            // and then traverse up the hierarchy to find init pages all the
            // way up to the root of the app.
            while (!String.IsNullOrEmpty(pageDirectory) && pageDirectory != "/" && Util.IsWithinAppRoot(pageDirectory)) {

                // Go through the list of support extensions
                foreach (var extension in supportedExtensions) {
                    var path = VirtualPathUtility.Combine(pageDirectory, fileName + "." + extension);
                    if (currentPage.FileExists(path, useCache: true)) {
                        var factory = currentPage.GetObjectFactory(path);
                        var parentStartPage = (StartPage)factory();

                        parentStartPage.VirtualPath = path;
                        parentStartPage.ChildPage = currentPage;
                        currentPage = parentStartPage;

                        break;
                    }
                }

                pageDirectory = currentPage.GetDirectory(pageDirectory);
            }

            // At this point 'currentPage' is the root-most StartPage (if there were
            // any StartPages at all) or it is the requested page itself.
            return currentPage;
        }

        public override HelperResult RenderPage(string path, params object[] data) {
            return ChildPage.RenderPage(NormalizePath(path), data);
        }

        public void RunPage() {
            RunPageCalled = true;
            //ChildPage.PageContext = PageContext;
            ChildPage.ExecutePageHierarchy();
        }

        public override void Write(HelperResult result) {
            ChildPage.Write(result);
        }

        public override void WriteLiteral(object value) {
            ChildPage.WriteLiteral(value);
        }

        public override void Write(object value) {
            ChildPage.Write(value);
        }
    }
}
