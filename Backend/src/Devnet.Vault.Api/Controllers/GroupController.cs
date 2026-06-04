using Devnet.Vault.Api.Extensions;
using Devnet.Vault.Application.Features.Groups.Commands;
using Devnet.Vault.Application.Features.Groups.DTOs;
using Devnet.Vault.Application.Features.Groups.Queries;
using Devnet.Vault.Domain.Constants.Routes;
using Devnet.Vault.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;

namespace Devnet.Vault.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GroupController(IMediator _mediator) : ControllerBase
{
    [HttpPost(ApiEndpoints.GroupApiEndpoints.CREATE)]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var response = await _mediator.Send(new CreateGroupCommand(request, userId), cancellationToken);
        return Ok(response);
    }

    [HttpPatch(ApiEndpoints.GroupApiEndpoints.UPDATE_NAME)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateGroupName([FromBody] UpdateGroupNameRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0)
            return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateGroupNameCommand(request, userId, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = GroupValidationMessages.GROUP_UPDATE_FAILED });
    }

    [HttpPatch(ApiEndpoints.GroupApiEndpoints.UPDATE_FAVOURITE)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateGroupFavourite([FromBody] UpdateGroupFavouriteRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0) return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateGroupFavouriteCommand(request, userId, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = GroupValidationMessages.GROUP_UPDATE_FAILED });
    }

    [HttpPatch(ApiEndpoints.GroupApiEndpoints.UPDATE_PARENT)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateGroupParent([FromBody] UpdateGroupParentRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0) return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateGroupParentCommand(request, userId, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = GroupValidationMessages.GROUP_UPDATE_FAILED });
    }


    [HttpPatch(ApiEndpoints.GroupApiEndpoints.UPDATE_METADATA)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> UpdateGroupMetaData([FromBody] UpdateGroupMetaDataJsonRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0) return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new UpdateGroupMetaDataJsonCommand(request, userId, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = GroupValidationMessages.GROUP_UPDATE_FAILED });
    }

    [HttpDelete(ApiEndpoints.GroupApiEndpoints.DELETE_GROUP)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> DeleteGroup([FromQuery] long groupId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0) return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var result = await _mediator.Send(new DeleteGroupCommand(new DeleteGroupRequest { GroupId = groupId }, userId, userId), cancellationToken);
        if (result) return Ok(result);
        return BadRequest(new { message = GroupValidationMessages.GROUP_DELETE_FAILED });
    }

    [HttpGet(ApiEndpoints.GroupApiEndpoints.CHILDREN)]
    [ProducesResponseType(typeof(List<GroupDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> GetChildren([FromQuery] long? parentGroupId, [FromQuery] GroupType groupType, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0) return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var items = await _mediator.Send(new GetChildGroupsQuery(userId, parentGroupId, groupType), cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiEndpoints.GroupApiEndpoints.PARENT)]
    [ProducesResponseType(typeof(GroupDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> GetCurrentGroupParent(long groupId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId <= 0) return Unauthorized(new { message = UserInfoMessages.REQUEST_USER_ID_INVALID });

        var item = await _mediator.Send(new GetParentGroupQuery(userId, groupId), cancellationToken);
        if (item == null) return NotFound(new { message = GroupValidationMessages.PARENT_ITSELF_OR_NOT_FOUND });
        return Ok(item);
    }

    [HttpGet(ApiEndpoints.GroupApiEndpoints.GET_BY_ID)]
    [ProducesResponseType(typeof(GroupDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    public async Task<IActionResult> GetById(long groupId, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetGroupByIdQuery(groupId), cancellationToken);
        if (item == null) return NotFound(new { message = GroupValidationMessages.GROUP_NOT_FOUND });
        return Ok(item);
    }
}
