using Devnet.Vault.Api.Extensions;
using Devnet.Vault.Application.Features.VaultItems.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Devnet.Vault.Application.Features.VaultItems.Commands.VaultItemsCommands;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;
using static Devnet.Vault.Domain.Constants.Routes.ApiEndpoints;

namespace Devnet.Vault.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VaultItemController(IMediator _mediator) : ControllerBase
{
    [HttpPost(VaultItemApiEndpoints.CREATE)]
    [ProducesResponseType(typeof(AddVaultItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> CreateVaultItem([FromBody] AddVaultItemRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new AddNewVaultItemCommand(request, userId), cancellationToken);
        return Ok(response);
    }

    [HttpPatch(VaultItemApiEndpoints.UPDATE_TITLE)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateTitle([FromBody] UpdateVaultItemTitleDto request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateVaultItemTitleCommand(request, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = VaultEntryValidationMessages.ENTRY_UPDATE_FAILED });
    }

    [HttpPatch(VaultItemApiEndpoints.UPDATE_DATA)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateData([FromBody] UpdateVaultItemDataDto request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateVaultItemDataCommand(request, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = VaultEntryValidationMessages.ENTRY_UPDATE_FAILED });
    }

    [HttpPatch(VaultItemApiEndpoints.UPDATE_GROUP)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateVaultItemGroupDto request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateVaultItemGroupCommand(request, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = VaultEntryValidationMessages.ENTRY_UPDATE_FAILED });
    }

    [HttpDelete(VaultItemApiEndpoints.DELETE_ITEM)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> DeleteVaultItem([FromQuery] long vaultEntryId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new DeleteVaultItemCommand(vaultEntryId, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = VaultEntryValidationMessages.ENTRY_DELETE_FAILED });
    }
}