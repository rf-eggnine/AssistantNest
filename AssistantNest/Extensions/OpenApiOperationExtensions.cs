// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace AssistantNest.Extensions;

internal static class OpenApiOperationExtensions
{
    internal static OpenApiOperation AddEmptyStandardErrorResponses(this OpenApiOperation op)
    {
        op.Responses.Add("400", new OpenApiResponse() { Description = "Bad Request" });
        op.Responses.Add("401", new OpenApiResponse() { Description = "Unauthorized" });
        op.Responses.Add("403", new OpenApiResponse() { Description = "Forbidden" });
        op.Responses.Add("404", new OpenApiResponse() { Description = "Not Found" });
        op.Responses.Add("500", new OpenApiResponse() { Description = "Internal Server Error" });
        return op;
    }
    internal static OpenApiOperation AddEmptyStandardOkResponse(this OpenApiOperation op)
    {
        op.Responses.Remove("200");
        op.Responses.Add("200", new OpenApiResponse() { Description = "OK" });
        return op;
    }
    internal static OpenApiOperation AddGetProjectsResponse(this OpenApiOperation op)
    {
        op.Responses.Remove("200");
        op.Responses.Add("200", new OpenApiResponse()
        {
            Description = "OK",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                { "application/json", new OpenApiMediaType()
                    {
                        Schema = new OpenApiSchema()
                        {
                            Type = "array",
                            Items = new OpenApiSchema()
                            {
                                Type = "object",
                                Reference = new OpenApiReference()
                                {
                                    Id = "AnProject",
                                    Type = ReferenceType.Schema
                                }
                            }
                        },
                    }
                },
            }
        });
        return op;
    }
}
