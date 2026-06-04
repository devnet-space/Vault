using Devnet.Vault.Api.Extensions;
using Devnet.Vault.Application.Features.Account.Commands;
using Devnet.Vault.Application.Features.Account.DTOs;
using Devnet.Vault.Application.Features.Account.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;
using static Devnet.Vault.Domain.Constants.Routes.ApiEndpoints;

namespace Devnet.Vault.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Get current user's profile details
    /// </summary>
    [HttpGet(AccountApiEndpoints.USER_PROFILE_ENDPOINT)]
    [ProducesResponseType(typeof(ProfileDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfileDetails(CancellationToken cancellationToken)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            if (userId <= 0)
                return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

            var response = await _mediator.Send(new GetProfileDetailsQuery(userId), cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update current user's profile details
    /// </summary>
    [HttpPut(AccountApiEndpoints.USER_PROFILE_UPDATE_ENDPOINT)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfileDetails([FromForm] UpdateProfileDetailsRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new UpdateProfileDetailsCommand(request, userId, userId), cancellationToken);
        if (response)
            return Ok(response);
        return BadRequest(new { message = ProfileMessages.PROFILE_UPDATE_FAILED });
    }

    /// <summary>
    /// Update current user's email address
    /// </summary>
    [HttpPut(AccountApiEndpoints.UPDATE_EMAIL_ENDPOINT)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateEmailAddress([FromBody] UpdateEmailAddressRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new UpdateUserEmailAddressCommand(request, userId, userId), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Update current user's phone number
    /// </summary>
    [HttpPut(AccountApiEndpoints.UPDATE_PHONE_ENDPOINT)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new UpdateUserPhoneNumberCommand(request, userId, userId), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Deactivate current user's account
    /// </summary>
    [HttpPost(AccountApiEndpoints.DEACTIVATE_ACCOUNT_ENDPOINT)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateAccount([FromBody] DeactivateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new DeactivateAccountCommand(request, userId, userId), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Permanently delete current user's account
    /// </summary>
    [HttpPost(AccountApiEndpoints.DELETE_ACCOUNT_ENDPOINT)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new DeleteAccountCommand(request, userId, userId), cancellationToken);
        return Ok(response);
    }
}
