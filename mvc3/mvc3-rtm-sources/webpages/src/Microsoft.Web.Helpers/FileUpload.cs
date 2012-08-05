using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;
using Microsoft.Web.Helpers.Resources;

namespace Microsoft.Web.Helpers {
    public static class FileUpload {
        /// <summary>
        /// Creates HTML for multiple file upload.
        /// </summary>
        /// <param name="name">The value assigned to the name attribute of the file upload input elements.</param>
        /// <param name="initialNumberOfFiles">Initial number of file upload fields to display.</param>
        /// <param name="allowMoreFilesToBeAdded">If true, allows more file upload fields to be added.</param>
        /// <param name="includeFormTag">If true, result includes form tag around all file input tags and submit button. 
        /// If false, user needs to specify their own form tag around call to GetHtml with their own submit button.</param>
        /// <param name="addText">Text to display on a link that allows more file upload fields can be added.</param>
        /// <param name="uploadText">Text to display on the submit button.</param>
        public static HtmlString GetHtml(
            string name = null,
            int initialNumberOfFiles = 1,
            bool allowMoreFilesToBeAdded = true,
            bool includeFormTag = true,
            string addText = null,
            string uploadText = null) {

            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            FileUploadImplementation fileUpload = new FileUploadImplementation(httpContext);
            return fileUpload.GetHtml(name: name, initialNumberOfFiles: initialNumberOfFiles, allowMoreFilesToBeAdded: allowMoreFilesToBeAdded, includeFormTag: includeFormTag,
                addText: addText, uploadText: uploadText);
        }
    }

    internal class FileUploadImplementation {
        private static readonly object _countKey = new object();
        private static readonly object _scriptAlreadyRendered = new object();

        private readonly HttpContextBase _httpContext;

        public FileUploadImplementation(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        private int RenderCount {
            get {
                int? count = _httpContext.Items[_countKey] as int?;
                if (!count.HasValue) {
                    count = 0;
                }
                return count.Value;
            }
            set {
                _httpContext.Items[_countKey] = value;
            }
        }

        private bool ScriptAlreadyRendered {
            get {
                bool? rendered = _httpContext.Items[_scriptAlreadyRendered] as bool?;
                return rendered.HasValue && rendered.Value;
            }
            set {
                _httpContext.Items[_scriptAlreadyRendered] = value;
            }
        }

        public HtmlString GetHtml(
            string name,
            int initialNumberOfFiles,
            bool allowMoreFilesToBeAdded,
            bool includeFormTag,
            string addText,
            string uploadText) {

            if (initialNumberOfFiles < 0) {
                throw new ArgumentOutOfRangeException(
                    "initialNumberOfFiles",
                    String.Format(CultureInfo.InvariantCulture, CommonResources.Argument_Must_Be_GreaterThanOrEqualTo, "0"));
            }

            if (String.IsNullOrEmpty(addText)) {
                addText = HelpersToolkitResources.FileUpload_AddMore;
            }
            if (String.IsNullOrEmpty(uploadText)) {
                uploadText = HelpersToolkitResources.FileUpload_Upload;
            }
            if (String.IsNullOrEmpty(name)) {
                name = "fileUpload";
            }


            TagBuilder formTag = null;
            if (includeFormTag) {
                // <form method="post" enctype="multipart/form-data" >
                formTag = new TagBuilder("form");
                formTag.MergeAttribute("method", "post");
                formTag.MergeAttribute("enctype", "multipart/form-data");
                formTag.MergeAttribute("action", "");
            }

            // <div id="file-upload-all-files">
            TagBuilder outerDivTag = new TagBuilder("div");
            outerDivTag.MergeAttribute("id", "file-upload-" + RenderCount);
            outerDivTag.MergeAttribute("class", "file-upload");

            // <div><input type="file" name="fileUpload"/></div>                
            TagBuilder fileInputTag = new TagBuilder("input");
            fileInputTag.MergeAttribute("type", "file");
            fileInputTag.MergeAttribute("name", "fileUpload");
            TagBuilder innerDivTag = new TagBuilder("div");
            innerDivTag.InnerHtml = fileInputTag.ToString(TagRenderMode.SelfClosing);

            outerDivTag.InnerHtml = String.Join(String.Empty, Enumerable.Repeat(innerDivTag.ToString(), initialNumberOfFiles));

            TagBuilder aTag = null;
            if (allowMoreFilesToBeAdded) {
                // <a href="#" onclick="FileUploadHelper.addInputElement(1, "foo"); return false;" >Add more!</a>
                aTag = new TagBuilder("a");
                aTag.MergeAttribute("href", "#");
                aTag.MergeAttribute("onclick",
                        String.Format(CultureInfo.InvariantCulture, 
                            "FileUploadHelper.addInputElement({0}, {1}); return false;",
                            RenderCount, HttpUtility.JavaScriptStringEncode(name, addDoubleQuotes: true)));
                aTag.SetInnerText(addText);
            }

            // <input value="Upload" type="submit"/>
            TagBuilder submitInputTag = null;
            if (includeFormTag) {
                submitInputTag = new TagBuilder("input");
                submitInputTag.MergeAttribute("type", "submit");
                submitInputTag.MergeAttribute("value", uploadText);
            }

            StringBuilder finalHtml = new StringBuilder();
            if (allowMoreFilesToBeAdded && !ScriptAlreadyRendered) {
                finalHtml.Append(_UploadScript);
                ScriptAlreadyRendered = true;
            }

            if (includeFormTag) {
                StringBuilder formTagContent = new StringBuilder();
                ComposeFileUploadTags(formTagContent, outerDivTag, aTag, submitInputTag);
                formTag.InnerHtml = formTagContent.ToString();
                finalHtml.Append(formTag.ToString());
            }
            else {
                ComposeFileUploadTags(finalHtml, outerDivTag, aTag, submitInputTag);
            }

            RenderCount++;
            return new HtmlString(finalHtml.ToString());
        }

        private static void ComposeFileUploadTags(StringBuilder sb, TagBuilder outerDivTag, TagBuilder aTag, TagBuilder submitTag) {
            sb.Append(outerDivTag.ToString());

            if ((aTag != null) || (submitTag != null)) {

                TagBuilder divTag = new TagBuilder("div");
                divTag.MergeAttribute("class", "file-upload-buttons");

                StringBuilder innerHtml = new StringBuilder();
                if (aTag != null) {
                    innerHtml.Append(aTag.ToString());
                }

                if (submitTag != null) {
                    innerHtml.Append(submitTag.ToString(TagRenderMode.SelfClosing));
                }

                divTag.InnerHtml = innerHtml.ToString();
                sb.Append(divTag.ToString());
            }
        }

        private const string _UploadScript =
"<script type=\"text/javascript\">" +
    "if (!window[\"FileUploadHelper\"]) window[\"FileUploadHelper\"] = {}; " +
    "FileUploadHelper.addInputElement = function(index, name) { " +
        "var inputElem = document.createElement(\"input\"); " +
        "inputElem.type = \"file\"; " +
        "inputElem.name = name; " +
        "var divElem = document.createElement(\"div\"); " +
        "divElem.appendChild(inputElem.cloneNode(false)); " + //Appending the created node creates an editable text field in IE8.
        "var inputs = document.getElementById(\"file-upload-\" + index); " +
        "inputs.appendChild(divElem); " +
"} </script>";
    }
}