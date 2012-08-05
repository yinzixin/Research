using System.Collections;
using System.Web;
using System.Web.Helpers.Test;
using System.Web.WebPages.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Web.Helpers.Test {
    [TestClass]
    public class FileUploadTest {
        private const string _fileUploadScript = "<script type=\"text/javascript\">if (!window[\"FileUploadHelper\"]) window[\"FileUploadHelper\"] = {}; FileUploadHelper.addInputElement = function(index, name) { var inputElem = document.createElement(\"input\"); inputElem.type = \"file\"; inputElem.name = name; var divElem = document.createElement(\"div\"); divElem.appendChild(inputElem.cloneNode(false)); var inputs = document.getElementById(\"file-upload-\" + index); inputs.appendChild(divElem); } </script>";

        [TestMethod]
        public void RenderThrowsWhenNumberOfFilesIsLessThanZero() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());

            ExceptionAssert.ThrowsArgGreaterThanOrEqualTo(
                () => fileUpload.GetHtml(name: null, initialNumberOfFiles: -2, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: "", uploadText: ""),
                "initialNumberOfFiles",
                "0");
        }

        [TestMethod]
        public void ResultIncludesFormTagAndSubmitButtonWhenRequested() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());
            string expectedResult = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-0"">"
                                  + @"<div><input name=""fileUpload"" type=""file"" /></div></div>"
                                  + @"<div class=""file-upload-buttons""><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var actualResult = fileUpload.GetHtml(name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            Assert.AreEqual(expectedResult, actualResult.ToString());
        }

        [TestMethod]
        public void ResultDoesNotIncludeFormTagAndSubmitButtonWhenNotRequested() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());
            string expectedResult = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>";

            // Act 
            var actualResult = fileUpload.GetHtml(name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: null, uploadText: null);
            Assert.AreEqual(expectedResult, actualResult.ToString());
        }

        [TestMethod]
        public void ResultIncludesCorrectNumberOfInputFields() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());
            string expectedResult = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div><div><input name=""fileUpload"" type=""file"" /></div>"
                                  + @"<div><input name=""fileUpload"" type=""file"" /></div></div>";

            // Act
            var actualResult = fileUpload.GetHtml(name: null, initialNumberOfFiles: 3, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: null, uploadText: null);

            // Assert
            Assert.AreEqual(expectedResult, actualResult.ToString());
        }

        [TestMethod]
        public void ResultIncludesAnchorTagWithCorrectAddText() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());
            string customAddText = "Add More";
            string expectedResult = _fileUploadScript
                                  + @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                  + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(0, &quot;fileUpload&quot;); return false;"">" + customAddText + "</a></div>";

            // Act
            var result = fileUpload.GetHtml(name: null, allowMoreFilesToBeAdded: true, includeFormTag: false, addText: customAddText, initialNumberOfFiles: 1, uploadText: null);

            // Assert
            Assert.AreEqual(expectedResult, result.ToString());
        }

        [TestMethod]
        public void ResultDoesNotIncludeAnchorTagNorAddTextWhenNotRequested() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());
            string customAddText = "Add More";
            string expectedResult = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>";

            // Act
            var result = fileUpload.GetHtml(name: null, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: customAddText, uploadText: null, initialNumberOfFiles: 1);

            // Assert
            Assert.AreEqual(expectedResult, result.ToString());
        }

        [TestMethod]
        public void ResultIncludesSubmitInputTagWithCustomUploadText() {
            // Arrange
            var fileUpload = new FileUploadImplementation(GetContext());
            string customUploadText = "Now!";
            string expectedResult = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-0"">"
                                  + @"<div><input name=""fileUpload"" type=""file"" /></div></div>"
                                  + @"<div class=""file-upload-buttons""><input type=""submit"" value=""" + customUploadText + @""" /></div></form>";

            // Act
            var result = fileUpload.GetHtml(name: null, includeFormTag: true, uploadText: customUploadText, allowMoreFilesToBeAdded: false, initialNumberOfFiles: 1, addText: null);

            // Assert
            Assert.AreEqual(expectedResult, result.ToString());
        }

        [TestMethod]
        public void FileUploadGeneratesUniqueIdsForMultipleCallsForCommonRequest() {
            // Arrange
            var context = GetContext();
            string expectedResult1 = @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div><div><input name=""fileUpload"" type=""file"" /></div>"
                                   + @"<div><input name=""fileUpload"" type=""file"" /></div></div>";
            string expectedResult2 = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-1""><div><input name=""fileUpload"" type=""file"" /></div>"
                                   + @"<div><input name=""fileUpload"" type=""file"" /></div></div><div class=""file-upload-buttons""><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var result1 = new FileUploadImplementation(context).GetHtml(name: null, initialNumberOfFiles: 3, allowMoreFilesToBeAdded: false, includeFormTag: false, addText: null, uploadText: null);
            var result2 = new FileUploadImplementation(context).GetHtml(name: null, initialNumberOfFiles: 2, allowMoreFilesToBeAdded: false, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            Assert.AreEqual(expectedResult1, result1.ToString());
            Assert.AreEqual(expectedResult2, result2.ToString());
        }

        [TestMethod]
        public void FileUploadGeneratesScriptOncePerRequest() {
            // Arrange
            var context = GetContext();
            string expectedResult1 = _fileUploadScript
                                   + @"<div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                   + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(0, &quot;fileUpload&quot;); return false;"">Add more files</a></div>";
            string expectedResult2 = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-1""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                   + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(1, &quot;fileUpload&quot;); return false;"">Add more files</a><input type=""submit"" value=""Upload"" /></div></form>";
            string expectedResult3 = @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-2"">"
                                  + @"<div><input name=""fileUpload"" type=""file"" /></div></div>"
                                  + @"<div class=""file-upload-buttons""><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var result1 = new FileUploadImplementation(context).GetHtml(name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: true, includeFormTag: false, addText: null, uploadText: null);
            var result2 = new FileUploadImplementation(context).GetHtml(name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: true, includeFormTag: true, addText: null, uploadText: null);
            var result3 = new FileUploadImplementation(context).GetHtml(name: null, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: false, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            Assert.AreEqual(expectedResult1, result1.ToString());
            Assert.AreEqual(expectedResult2, result2.ToString());
            Assert.AreEqual(expectedResult3, result3.ToString());
        }

        [TestMethod]
        public void FileUploadUsesNamePropertyInJavascript() {
            // Arrange
            var context = GetContext();
            string name = "foobar";
            string expectedResult = _fileUploadScript
                                   + @"<form action="""" enctype=""multipart/form-data"" method=""post""><div class=""file-upload"" id=""file-upload-0""><div><input name=""fileUpload"" type=""file"" /></div></div>"
                                   + @"<div class=""file-upload-buttons""><a href=""#"" onclick=""FileUploadHelper.addInputElement(0, &quot;foobar&quot;); return false;"">Add more files</a><input type=""submit"" value=""Upload"" /></div></form>";

            // Act
            var result = new FileUploadImplementation(context).GetHtml(name: name, initialNumberOfFiles: 1, allowMoreFilesToBeAdded: true, includeFormTag: true, addText: null, uploadText: null);

            // Assert
            Assert.AreEqual(expectedResult, result.ToString());
        }

        private HttpContextBase GetContext() {
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Items).Returns(new Hashtable());
            return context.Object;
        }
    }
}