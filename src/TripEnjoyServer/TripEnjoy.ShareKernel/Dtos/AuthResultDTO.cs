namespace TripEnjoy.ShareKernel.Dtos
{
    public record AuthResultDTO(
        string Token,
        string RefreshToken,
        string AspNetUserId
    );
}