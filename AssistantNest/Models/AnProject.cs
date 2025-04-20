// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;

namespace AssistantNest.Models;

public class AnProject
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string OpenAiOrganizationId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted => DeletedAt.HasValue;
    public Guid UserId { get; set; }
    public AnUser? User { get; set; } = null!;
}
