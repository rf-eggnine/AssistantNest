// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AssistantNest.OpenApi;

internal abstract class ISchemaUpdatingFilter : ISchemaFilter
{
    public const string ObjectType = "object";
    public const string StringType = "string";
    public const string BooleanType = "boolean";
    public const string IntegerType = "integer";
    public const string UuidFormat = "uuid";
    public const string DateTimeFormat = "date-time";
    protected abstract Type Type {get;}

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {        
        if(Type.Equals(context.Type))
        {
            Apply(schema);
        }
    }
    protected abstract void Apply(OpenApiSchema schema);
}
