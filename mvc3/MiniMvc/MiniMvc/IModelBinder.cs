using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artech.MiniMvc
{
public interface IModelBinder
{
    object BindModel(ControllerContext controllerContext, string modelName, Type modelType);
}
}
