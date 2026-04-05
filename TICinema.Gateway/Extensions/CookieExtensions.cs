namespace TICinema.Gateway.Extensions
{
    public static class CookieExtensions
    {
        public static void SetRefreshTokenCookie(this HttpResponse response, string token)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            response.Cookies.Append("refreshToken", token, options);
        }

        public static void DeleteRefreshTokenCookie(this HttpResponse response)
        {
            response.Cookies.Delete("refreshToken");
        }
    }
}
