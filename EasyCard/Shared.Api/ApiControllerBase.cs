using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api
{
    public class ApiControllerBase: ControllerBase
    {
        public void EnsureExists<T>(T entity)
        {

        }
    }
}
