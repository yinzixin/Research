using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Artech.MiniMvc
{
public class RawContentResult: ActionResult
{
    public string RawData { get; private set; }
    public RawContentResult(string rawData)
    {
        RawData = rawData; 
    }
    public override void ExecuteResult(ControllerContext context)
    {
        context.RequestContext.HttpContext.Response.Write(this.RawData);
    }
}
}
