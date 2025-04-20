// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using AssistantNest.Models;
using Microsoft.OpenApi.Models;

namespace AssistantNest.OpenApi;


internal class AnProjectsSchemaFilter : ISchemaUpdatingFilter
{
    protected override Type Type => typeof(AnProject);

    public void Aply(OpenApiSchema schema) => Apply(schema);
    protected override void Apply(OpenApiSchema schema)
    {
        schema.Type = ObjectType;
        schema.Properties.Add(nameof(AnProject.Id), new OpenApiSchema() { Type = StringType, Format = UuidFormat });
        schema.Properties.Add(nameof(AnProject.Name), new OpenApiSchema() { Type = StringType, Format = UuidFormat });
        schema.Properties.Add(nameof(AnProject.NickName), new OpenApiSchema() { Type = StringType, Format = UuidFormat });
        schema.Properties.Add(nameof(AnProject.OpenAiId), new OpenApiSchema() { Type = BooleanType });
    }
}
