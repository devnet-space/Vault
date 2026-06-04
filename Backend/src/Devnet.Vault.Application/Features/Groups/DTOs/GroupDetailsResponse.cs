using Devnet.Vault.Domain.Enums;

namespace Devnet.Vault.Application.Features.Groups.DTOs;

public record GroupDetailsResponse
{
    public long GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentGroupId { get; set; }
    public bool IsFavourite { get; set; }
    public string MetadataJson { get; set; } = string.Empty;
    public GroupType GroupType { get; set; }
    public DateTime CreatedAt { get; set; }
}