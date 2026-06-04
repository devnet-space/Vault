using Devnet.Vault.Api.Extensions;
using Devnet.Vault.Application.Features.Account.DTOs;
using Devnet.Vault.Application.Features.Account.Queries;
using Devnet.Vault.Application.Features.Shared.Otp.Commands;
using Devnet.Vault.Application.Features.Shared.Otp.DTOs;
using Devnet.Vault.Domain.Constants.Routes;
using Devnet.Vault.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;
using static Devnet.Vault.Domain.Constants.Routes.ApiEndpoints;

namespace Devnet.Vault.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController(IMediator _mediator) : ControllerBase
{
    #region Request OTP

    /// <summary>
    /// Request OTP for authentication
    /// </summary>
    [HttpPost(ApiEndpoints.AuthApiEndpoints.REQUEST_AUTH_OTP_ENDPOINT)]
    [ProducesResponseType(typeof(RequestOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new RequestOtpCommand(request), cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Request OTP to update current user's email address
    /// </summary>
    [HttpPost(AccountApiEndpoints.REQUEST_UPDATE_EMAIL_OTP_ENDPOINT)]
    [ProducesResponseType(typeof(RequestOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestUpdateEmailOtp([FromBody] RequestUpdateEmailOtpRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var userDetails = await _mediator.Send(new GetProfileDetailsQuery(userId), cancellationToken);
        if (userDetails == null)
            return NotFound(UserInfoMessages.USER_NOT_FOUND);

        if (userDetails.Email == request.Email)
            return BadRequest(ProfileMessages.EMAIL_ALREADY_IN_USE);

        var otpRequest = new RequestOtpRequest
        {
            Identifier = request.Email ?? throw new InvalidOperationException(ProfileMessages.INVALID_EMAIL_ADDRESS),
            ChannelType = NotificationChannel.Email,
            Purpose = OtpPurpose.Authentication
        };

        var response = await _mediator.Send(new RequestOtpCommand(otpRequest), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Request OTP to update current user's phone number
    /// </summary>
    [HttpPost(AccountApiEndpoints.REQUEST_UPDATE_PHONE_OTP_ENDPOINT)]
    [ProducesResponseType(typeof(RequestOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestUpdatePhoneNumberOtp([FromBody] RequestUpdatePhoneNumberOtpRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });
        if (request.ChannelType == NotificationChannel.Email)
            return BadRequest(new { message = OtpValidationMessages.CHANNEL_INVALID });
        // TO DO: Get Country Code from Country Id sent in request
        var userDetails = await _mediator.Send(new GetProfileDetailsQuery(userId), cancellationToken);
        if (userDetails == null)
            return NotFound(UserInfoMessages.USER_NOT_FOUND);

        if (userDetails.PhoneNumber == request.PhoneNumber)
            return BadRequest(ProfileMessages.PHONE_NUMBER_ALREADY_IN_USE);

        var otpRequest = new RequestOtpRequest
        {
            Identifier = request.PhoneNumber ?? throw new InvalidOperationException(ProfileMessages.INVALID_PHONE_NUMBER),
            ChannelType = request.ChannelType,
            Purpose = OtpPurpose.Authentication
        };

        var response = await _mediator.Send(new RequestOtpCommand(otpRequest), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Request OTP for account deactivation
    /// </summary>
    [HttpPost(AccountApiEndpoints.DEACTIVATE_OTP_REQUEST_ENDPOINT)]
    [ProducesResponseType(typeof(RequestOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestDeactivationOtp(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        // Get user contact details for OTP
        var userDetails = await _mediator.Send(new GetProfileDetailsQuery(userId), cancellationToken);
        if (userDetails == null)
            return NotFound(UserInfoMessages.USER_NOT_FOUND);

        var otpRequest = new RequestOtpRequest
        {
            Identifier = userDetails.Email ?? userDetails.PhoneNumber ?? throw new InvalidOperationException(OtpValidationMessages.CHANNEL_VALUE_NULL),
            ChannelType = userDetails.Email != null ? NotificationChannel.Email : NotificationChannel.SMS,
            Purpose = OtpPurpose.AccountDeactivation
        };

        var response = await _mediator.Send(new RequestOtpCommand(otpRequest), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Request OTP for account deletion
    /// </summary>
    [HttpPost(AccountApiEndpoints.DELETE_ACCOUNT_OTP_REQUEST_ENDPOINT)]
    [ProducesResponseType(typeof(RequestOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestDeletionOtp(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        // Get user contact details for OTP
        var userDetails = await _mediator.Send(new GetProfileDetailsQuery(userId), cancellationToken);
        if (userDetails == null)
            return NotFound(UserInfoMessages.USER_NOT_FOUND);

        var otpRequest = new RequestOtpRequest
        {
            Identifier = userDetails.Email ?? userDetails.PhoneNumber ?? throw new InvalidOperationException(OtpValidationMessages.CHANNEL_VALUE_NULL),
            ChannelType = userDetails.Email != null ? NotificationChannel.Email : NotificationChannel.SMS,
            Purpose = OtpPurpose.AccountDeletion
        };

        var response = await _mediator.Send(new RequestOtpCommand(otpRequest), cancellationToken);
        return Ok(response);
    }
    #endregion
}
