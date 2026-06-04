using Devnet.Vault.Domain.Enums;

namespace Devnet.Vault.Application.Features.Groups.DTOs;

public record CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public long? ParentGroupId { get; set; }
    public bool IsFavourite { get; set; }
    public string? MetadataJson { get; set; }
    public GroupType GroupType { get; set; }
}

public record CreateGroupResponse
{
    public long GroupId { get; set; }
}
