using Shared.Api.Models;
using Shared.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shared.Api.Extensions
{
    public static class CommonFilteringExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> src, FilterBase filter, int? pageSizeLimit = null)
        {
            if (filter is null)
            {
                return src;
            }

            if (filter.Skip >= 0)
            {
                src = src.Skip(filter.Skip.Value);
            }

            int maximumPageSize = pageSizeLimit ?? 1000;

            int take = filter.Take == null || filter.Take > maximumPageSize || filter.Take <= 0 ? maximumPageSize : filter.Take.Value;

            src = src.Take(take);

            return src;
        }

        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string orderByProperty, bool? orderDesc)
        {
            var expression = source.Expression;

            if (string.IsNullOrWhiteSpace(orderByProperty))
            {
                return source;
            }

            PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(T)).Find(orderByProperty, true);

            if (prop == null)
            {
                return source;
            }

            string method = orderDesc.GetValueOrDefault(true) ? "OrderBy" : "OrderByDescending";
            var parameter = Expression.Parameter(typeof(T), "x");
            var selector = Expression.PropertyOrField(parameter, prop.Name);

            expression = Expression.Call(typeof(Queryable), method,
                new Type[] { source.ElementType, selector.Type },
                expression, Expression.Quote(Expression.Lambda(selector, parameter)));

            return source.Provider.CreateQuery<T>(expression);
        }

        public static Expression<Func<T, bool>> BuildWhereOrElseClause<T, Tprop>(string clauseParam, List<Tprop> values)
        {
            if (string.IsNullOrWhiteSpace(clauseParam))
            {
                throw new ArgumentException("You must specify where clause parameter to build OR statement");
            }

            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("You must specify IDs for where clause parameter to build OR statement");
            }

            ParameterExpression param = Expression.Parameter(typeof(T), "a");

            MemberExpression member = Expression.Property(param, clauseParam);

            ConstantExpression constant;
            Expression exp = null, equality;

            for (int i = 0; i < values.Count; i++)
            {
                constant = Expression.Constant(values[i], typeof(Tprop));

                equality = Expression.Equal(member, constant);

                if (i == 0)
                {
                    exp = equality;
                }
                else
                {
                    exp = Expression.OrElse(exp, equality);
                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(exp, param);

            return lambda;
        }

        public static Expression<Func<T, bool>> BuildWhereOrElseClauseContains<T>(string clauseParam, string[] values)
        {
            if (string.IsNullOrWhiteSpace(clauseParam))
            {
                throw new ArgumentException("You must specify where clause parameter to build OR statement");
            }

            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("You must specify IDs for where clause parameter to build OR statement");
            }

            ParameterExpression param = Expression.Parameter(typeof(T), "a");

            MemberExpression member = Expression.Property(param, clauseParam);

            ConstantExpression constant;
            Expression exp = null, containsMethodExp;

            for (int i = 0; i < values.Length; i++)
            {
                constant = Expression.Constant(values[i], typeof(string));

                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                containsMethodExp = Expression.Call(member, method, constant);

                if (i == 0)
                {
                    exp = containsMethodExp;
                }
                else
                {
                    exp = Expression.OrElse(exp, containsMethodExp);
                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(exp, param);

            return lambda;
        }
    }
}
