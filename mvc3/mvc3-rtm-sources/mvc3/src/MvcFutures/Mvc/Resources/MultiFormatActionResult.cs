namespace Microsoft.Web.Mvc.Resources {
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Net.Mime;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Web.Resources;

    /// <summary>
    /// Returns the response in the format specified by the request. By default, supports returning the model
    /// as a HTML view, XML and JSON.
    /// If the response format requested is not supported, then the NotAcceptable status code is returned
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public class MultiFormatActionResult : ActionResult {
        object _model;
        ContentType _responseFormat;
        HttpStatusCode _statusCode;

        public MultiFormatActionResult(object model, ContentType responseFormat)
            : this(model, responseFormat, HttpStatusCode.OK) {
        }

        public MultiFormatActionResult(object model, ContentType responseFormat, HttpStatusCode statusCode) {
            _model = model;
            _responseFormat = responseFormat;
            _statusCode = statusCode;
        }

        public override void ExecuteResult(ControllerContext context) {
            if (!TryExecuteResult(context, this._model, this._responseFormat)) {
                throw new HttpException((int)HttpStatusCode.NotAcceptable, string.Format(CultureInfo.CurrentCulture, MvcResources.Resources_UnsupportedFormat, this._responseFormat));
            }
        }

        public virtual bool TryExecuteResult(ControllerContext context, object model, ContentType responseFormat) {
            if (!FormatManager.Current.CanSerialize(responseFormat)) {
                return false;
            }
            context.HttpContext.Response.ClearContent();
            context.HttpContext.Response.StatusCode = (int)_statusCode;
            FormatManager.Current.Serialize(context, model, responseFormat);
            return true;
        }
    }
}
