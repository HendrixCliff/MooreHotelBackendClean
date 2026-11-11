
namespace MooreHotelAndSuites.API.Extensions;

public static class RefreshTokenCookie
{
    public const string CookieName = "moore_hotel_refresh";

    private const string CookiePath = "/api/users";

    public static void Set(
        HttpResponse response,
        string refreshToken,
        DateTime expiry)
    {
        response.Cookies.Append(
            CookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = new DateTimeOffset(
                    DateTime.SpecifyKind(expiry, DateTimeKind.Utc)),
                IsEssential = true,
                Path = CookiePath
            });
    }

    public static void Clear(HttpResponse response)
    {
        response.Cookies.Delete(
            CookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = CookiePath
            });
    }
}
