using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[EnableRateLimiting("ApiThrottle")]
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>
    /// Get JWT access and refresh tokens
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (request.Username != "admin" || request.Password != "password")
            return Unauthorized();

        var (access, refresh) = await auth.GenerateTokensAsync(request.Username, ct);
        return Ok(new { access_token = access, refresh_token = refresh });
    }

    /// <summary>
    /// Refresh JWT access and refresh tokens
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var (access, refresh) = await auth.RefreshAsync(request.RefreshToken, ct);
        return Ok(new { access_token = access, refresh_token = refresh });
    }
}

public record LoginRequest(string Username, string Password);
public record RefreshRequest(string RefreshToken);