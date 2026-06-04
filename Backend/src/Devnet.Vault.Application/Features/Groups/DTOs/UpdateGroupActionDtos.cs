using Devnet.Vault.Domain.Enums;

namespace Devnet.Vault.Application.Features.Groups.DTOs;


public record UpdateGroupNameRequest
{
    public long GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record UpdateGroupFavouriteRequest
{
    public long GroupId { get; set; }
    public bool IsFavourite { get; set; }
}

public record UpdateGroupParentRequest
{
    public long GroupId { get; set; }
    public long? ParentGroupId { get; set; }
    public GroupType GroupType { get; set; }
}
public record UpdateGroupMetaDataJsonRequest
{
    public long GroupId { get; set; }
    public string MetadataJson { get; set; } = string.Empty;
}

public record DeleteGroupRequest
{
    public long GroupId { get; set; }
}