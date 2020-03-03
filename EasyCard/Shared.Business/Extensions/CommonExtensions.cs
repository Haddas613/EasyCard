using Shared.Business.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business.Extensions
{
    public static class CommonExtensions
    {
        public static T EnsureExists<T>(this T src, string entityName = null)
            where T : class
        {
            if (src == null)
            {
                throw new EntityNotFoundException(Messages.ApiMessages.EntityNotFound, entityName ?? src.GetType().Name, null);
            }

            return src;
        }
    }
}
