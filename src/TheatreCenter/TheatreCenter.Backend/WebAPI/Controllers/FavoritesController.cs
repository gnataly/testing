using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace TheatreCenter.Backend.WebAPI.Controllers;

[ApiController]
[Route("/api/v1")]
public class FavoritesController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(IAccountService accountService, ILogger<FavoritesController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost("favorite_actors/{actorId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "CreateFavoriteActorByActorId",
        Summary = "Добавить актера в избранное",
        Description = "Добавить актера в избранное для текущего пользователя")]
    [SwaggerResponse(200, "Актер успешно добавлен в избранное", typeof(FavoriteResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> CreateFavoriteActorByActorId([Required][FromRoute] int actorId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.AddFavoriteActorAsync(currentUserId, actorId);

            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Actor not found or already in favorites"));
            }

            return Ok(new FavoriteResponseDto(true, "Актер добавлен в избранное"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpDelete("favorite_actors/{actorId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "DeleteFavoriteActorByActorId",
        Summary = "Удалить актера из избранного",
        Description = "Удалить актера из избранного для текущего пользователя")]
    [SwaggerResponse(200, "Актер успешно удален из избранного", typeof(FavoriteResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteFavoriteActorByActorId([Required][FromRoute] int actorId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.RemoveFavoriteActorAsync(currentUserId, actorId);

            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Actor not found in favorites"));
            }

            return Ok(new FavoriteResponseDto(true, "Актер удален из избранного"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpPost("favorite_musicals/{musicalId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "CreateFavoriteMusicalByMusicalId",
        Summary = "Добавить мюзикл в избранное",
        Description = "Добавить мюзикл в избранное для текущего пользователя")]
    [SwaggerResponse(200, "Мюзикл успешно добавлен в избранное", typeof(FavoriteResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> CreateFavoriteMusicalByMusicalId([Required][FromRoute] int musicalId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.AddFavoriteMusicalAsync(currentUserId, musicalId);

            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Musical not found or already in favorites"));
            }

            return Ok(new FavoriteResponseDto(true, "Мюзикл добавлен в избранное"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpDelete("favorite_musicals/{musicalId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "DeleteFavoriteMusicalByMusicalId",
        Summary = "Удалить мюзикл из избранного",
        Description = "Удалить мюзикл из избранное для текущего пользователя")]
    [SwaggerResponse(200, "Мюзикл успешно удален из избранного", typeof(FavoriteResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteFavoriteMusicalByMusicalId([Required][FromRoute] int musicalId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.RemoveFavoriteMusicalAsync(currentUserId, musicalId);

            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Musical not found in favorites"));
            }

            return Ok(new FavoriteResponseDto(true, "Мюзикл удален из избранного"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpPost("favorite_theatres/{theatreId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "CreateFavoriteTheatreByTheatreId",
        Summary = "Добавить театр в избранное",
        Description = "Добавить театр в избранное для текущего пользователя")]
    [SwaggerResponse(200, "Театр успешно добавлен в избранное", typeof(FavoriteResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> CreateFavoriteTheatreByTheatreId([Required][FromRoute] int theatreId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.AddFavoriteTheatreAsync(currentUserId, theatreId);

            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Theatre not found or already in favorites"));
            }

            return Ok(new FavoriteResponseDto(true, "Театр добавлен в избранное"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }

    [HttpDelete("favorite_theatres/{theatreId}")]
    [Authorize(Policy = "AdminOrUser")]
    [SwaggerOperation(
        OperationId = "DeleteFavoriteTheatreByTheatreId",
        Summary = "Удалить театр из избранного",
        Description = "Удалить театр из избранное для текущего пользователя")]
    [SwaggerResponse(200, "Театр успешно удален из избранного", typeof(FavoriteResponseDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteFavoriteTheatreByTheatreId([Required][FromRoute] int theatreId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _accountService.RemoveFavoriteTheatreAsync(currentUserId, theatreId);

            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Theatre not found in favorites"));
            }

            return Ok(new FavoriteResponseDto(true, "Театр удален из избранного"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
        }
    }
}