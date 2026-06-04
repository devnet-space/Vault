using Devnet.Vault.Api.Extensions;
using Devnet.Vault.Application.Features.Auth.Commands;
using Devnet.Vault.Application.Features.Auth.DTOs;
using Devnet.Vault.Domain.Constants.AppSettings;
using Devnet.Vault.Domain.Constants.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;

namespace Devnet.Vault.Api.Controllers;

/// <summary>
/// Authentication API endpoints using CQRS pattern
/// Handles OTP-based registration and login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator _mediator) : ControllerBase
{
    #region Login With OTP

    /// <summary>
    /// Register or login user after OTP validation
    /// </summary>
    [HttpPost(ApiEndpoints.AuthApiEndpoints.LOGIN_WITH_OTP_ENDPOINT)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> LoginWithOtp([FromBody] RegisterOrLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) ||
                string.IsNullOrWhiteSpace(request.Otp))
        {
            return BadRequest(new { message = AuthValidationMessages.OTP_IDENTIFIER_REQUIRED });
        }

        var ipAddress =
            request.IpAddress ??
            HttpContext.GetRequestIpAddress();

        var modifiedRequest = request with
        {
            IpAddress = ipAddress
        };

        var response = await _mediator.Send(new RegisterOrLoginCommand(modifiedRequest), cancellationToken);

        SetAuthCookies(response);

        return Ok(response);
    }

    #endregion

    #region Refresh Token

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [Authorize]
    [HttpPost(ApiEndpoints.AuthApiEndpoints.REFRESH_TOKEN_ENDPOINT)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]

    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = AuthValidationMessages.OTP_REFRESH_TOKEN_REQUIRED });

        var userId = HttpContext.GetUserId();

        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new RefreshTokenCommand(request, userId), cancellationToken);
        ClearAuthCookies();
        SetAuthCookies(response);

        return Ok(response);
    }

    #endregion

    #region Logout

    /// <summary>
    /// Logout current user
    /// </summary>
    [Authorize]
    [HttpPost(ApiEndpoints.AuthApiEndpoints.LOGOUT_ENDPOINT)]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        if (!Request.Cookies.TryGetValue(AppConstants.APP_REFRESH_TOKEN_NAME, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest(new { message = AuthValidationMessages.OTP_REFRESH_TOKEN_REQUIRED });
        }

        var response = await _mediator.Send(new LogoutCommand(userId, refreshToken), cancellationToken);

        ClearAuthCookies();

        return Ok(response);
    }

    #endregion

    #region Private Helpers

    private void SetAuthCookies(AuthResponse response)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(response.ExpiryMinutes)
        };

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(
                AppTimes.REFRESH_TOKEN_EXPIRY_TIME_IN_DAYS)
        };

        Response.Cookies.Append(
            AppConstants.APP_ACCESS_TOKEN_NAME,
            response.AccessToken,
            accessTokenOptions);

        Response.Cookies.Append(
            AppConstants.APP_REFRESH_TOKEN_NAME,
            response.RefreshToken,
            refreshTokenOptions);
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(AppConstants.APP_ACCESS_TOKEN_NAME);
        Response.Cookies.Delete(AppConstants.APP_REFRESH_TOKEN_NAME);
    }

    #endregion
}