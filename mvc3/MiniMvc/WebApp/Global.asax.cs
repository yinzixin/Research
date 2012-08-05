using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Artech.MiniMvc;

namespace WebApp
{
public class Global : System.Web.HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {            
        RouteTable.Routes.Add("default",new Route{Url = "{controller}/{action}"});
        ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory());
        ControllerBuilder.Current.DefaultNamespaces.Add("WebApp");
    }

    protected void Session_Start(object sender, EventArgs e)
    {

    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {

    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {

    }

    protected void Application_Error(object sender, EventArgs e)
    {

    }

    protected void Session_End(object sender, EventArgs e)
    {

    }

    protected void Application_End(object sender, EventArgs e)
    {

    }
}
}