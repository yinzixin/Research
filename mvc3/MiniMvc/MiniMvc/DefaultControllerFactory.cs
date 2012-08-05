using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Artech.MiniMvc
{
public class DefaultControllerFactory : IControllerFactory
{
    public IController CreateController(RequestContext requestContext, string controllerName)
    {
        string controllerType = controllerName + "Controller";
        foreach (var ns in requestContext.RouteData.Namespaces)
        {
            string controllerTypeName = string.Format("{0}.{1}", ns, controllerType);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var controllerTypeQName = controllerTypeName + "," + assembly.FullName;
                var type = Type.GetType(controllerTypeQName);
                if (null != type)
                {
                    return (IController)Activator.CreateInstance(type);
                }
            }    
        }
        foreach(var ns in ControllerBuilder.Current.DefaultNamespaces)
        {
            string controllerTypeName = string.Format("{0}.{1}", ns, controllerType);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var controllerTypeQName = controllerTypeName + "," + assembly.FullName;
                var type = Type.GetType(controllerTypeQName);
                if (null != type)
                {
                    return (IController)Activator.CreateInstance(type);
                }
            }                
        }
        return null;
    }
}
}
