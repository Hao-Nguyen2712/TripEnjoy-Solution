namespace TripEnjoy.Client.ViewModels
{
    public class RefreshTokenRequestVM
    {
        public string ExpiredAccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
