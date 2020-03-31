using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Shared.Api.Swagger
{
    public class PolymorphismDocumentFilter<T> : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            RegisterSubClasses(context.SchemaRepository, context.SchemaGenerator, typeof(T));
        }

        private static void RegisterSubClasses(SchemaRepository schemaRegistry, ISchemaGenerator schemaGenerator, Type abstractType)
        {
            const string discriminatorName = "$type";
            OpenApiSchema parentSchema = null;

            if (schemaRegistry.TryGetIdFor(abstractType, out string parentSchemaId))
                parentSchema = schemaRegistry.Schemas[parentSchemaId];
            else
                parentSchema = schemaRegistry.GetOrAdd(abstractType, parentSchemaId, () => new OpenApiSchema());

            // set up a discriminator property (it must be required)
            parentSchema.Discriminator = new OpenApiDiscriminator() { PropertyName = discriminatorName };
            parentSchema.Required = new HashSet<string> { discriminatorName };

            if (parentSchema.Properties == null)
                parentSchema.Properties = new Dictionary<string, OpenApiSchema>();

            if (!parentSchema.Properties.ContainsKey(discriminatorName))
                parentSchema.Properties.Add(discriminatorName, new OpenApiSchema() { Type = "string", Default = new OpenApiString(abstractType.FullName) });

            // register all subclasses
            var derivedTypes = abstractType.GetTypeInfo().Assembly.GetTypes()
                .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

            foreach (var item in derivedTypes)
                schemaGenerator.GenerateSchema(item, schemaRegistry);
        }
    }

    public class PolymorphismSchemaFilter<T> : ISchemaFilter
    {
        private readonly Lazy<HashSet<Type>> derivedTypes = new Lazy<HashSet<Type>>(Init);

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (!derivedTypes.Value.Contains(type))
                return;

            var clonedSchema = new OpenApiSchema
            {
                Properties = schema.Properties,
                Type = schema.Type,
                Required = schema.Required
            };

            // schemaRegistry.Definitions[typeof(T).Name]; does not work correctly in SwashBuckle
            if (context.SchemaRepository.Schemas.TryGetValue(typeof(T).Name, out OpenApiSchema _))
            {
                schema.AllOf = new List<OpenApiSchema> {
                new OpenApiSchema { Reference = new OpenApiReference { Id = typeof(T).Name, Type = ReferenceType.Schema } },
                clonedSchema
            };
            }

            var assemblyName = Assembly.GetAssembly(type).GetName();
            schema.Discriminator = new OpenApiDiscriminator { PropertyName = "$type" };
            schema.AddExtension("x-ms-discriminator-value", new OpenApiString($"{type.FullName}, {assemblyName.Name}"));

            // reset properties for they are included in allOf, should be null but code does not handle it
            schema.Properties = new Dictionary<string, OpenApiSchema>();
        }

        private static HashSet<Type> Init()
        {
            var abstractType = typeof(T);
            var dTypes = abstractType.GetTypeInfo().Assembly
                .GetTypes()
                .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

            var result = new HashSet<Type>();

            foreach (var item in dTypes)
                result.Add(item);

            return result;
        }
    }
}
