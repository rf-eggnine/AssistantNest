// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using AssistantNest.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AssistantNest.OpenApi;

internal class GetProjectsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if ("get".Equals(context?.ApiDescription?.HttpMethod, StringComparison.OrdinalIgnoreCase)
            && "Get Projects".Equals(context.ApiDescription.ActionDescriptor.DisplayName))
        {
            operation.AddGetProjectsResponse();
        }
    }
}
