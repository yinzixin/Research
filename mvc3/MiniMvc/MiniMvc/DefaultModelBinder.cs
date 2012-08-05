using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.ComponentModel;
using System.Reflection;

namespace Artech.MiniMvc
{
public class DefaultModelBinder : IModelBinder
{
    public object BindModel(ControllerContext controllerContext, string modelName, Type modelType)
    {
        if (modelType.IsValueType || typeof(string) == modelType)
        {
            object instance;
            if (GetValueTypeInstance(controllerContext, modelName, modelType, out instance))
            {
                return instance;
            };
            return Activator.CreateInstance(modelType);
        }
        object modelInstance = Activator.CreateInstance(modelType);
        foreach (PropertyInfo property in modelType.GetProperties())
        {
            if (!property.CanWrite || (!property.PropertyType.IsValueType && property.PropertyType!= typeof(string)))
            {
                continue;
            }
            object propertyValue;
            if (GetValueTypeInstance(controllerContext, property.Name, property.PropertyType, out propertyValue))
            {
                property.SetValue(modelInstance, propertyValue, null);
            }
        }
        return modelInstance;
    }
    private  bool GetValueTypeInstance(ControllerContext controllerContext, string modelName, Type modelType, out object value)
    {
        var form = HttpContext.Current.Request.Form;
        string key;
        if (null != form)
        {
            key = form.AllKeys.FirstOrDefault(k => string.Compare(k, modelName, true) == 0);
            if (key != null)
            {
                value =  Convert.ChangeType(form[key], modelType);
                return true;
            }
        }

        key = controllerContext.RequestContext.RouteData.Values
            .Where(item => string.Compare(item.Key, modelName, true) == 0)
            .Select(item => item.Key).FirstOrDefault();
        if (null != key)
        {
            value = Convert.ChangeType(controllerContext.RequestContext.RouteData.Values[key], modelType);
            return true;
        }

        key = controllerContext.RequestContext.RouteData.DataTokens
            .Where(item => string.Compare(item.Key, modelName, true) == 0)
            .Select(item => item.Key).FirstOrDefault();
        if (null != key)
        {
            value = Convert.ChangeType(controllerContext.RequestContext.RouteData.DataTokens[key], modelType);
            return true;
        }
        value = null;
        return false;
    }
}
}
