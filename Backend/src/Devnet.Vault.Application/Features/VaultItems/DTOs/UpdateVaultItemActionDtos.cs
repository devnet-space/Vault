namespace Devnet.Vault.Application.Features.VaultItems.DTOs;

public record UpdateVaultItemActionDtos
{
    public long VaultEntryId { get; set; }
}

public record UpdateVaultItemTitleDto : UpdateVaultItemActionDtos
{
    public string Title { get; set; } = null!;
}

public record UpdateVaultItemDataDto : UpdateVaultItemActionDtos
{
    public string EncryptedData { get; set; } = null!;
}

public record UpdateVaultItemGroupDto : UpdateVaultItemActionDtos
{
    public long? GroupId { get; set; }
}