namespace Application.Interfaces
{
    public interface IAuthService
    {
        //this
        Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(string username, CancellationToken ct = default);
        Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken, CancellationToken ct = default);
    }
}
