using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Swagger
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerExcludeAttribute : Attribute
    {
    }
}
