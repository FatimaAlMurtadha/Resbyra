namespace server;

class Me
{
    public record Me_Data(int? UserId, string? Email, string? Role);

    public static IResult Get(HttpContext ctx)
    {
        if (!ctx.Session.IsAvailable || !ctx.Session.Keys.Contains("user_id"))
            return Results.Unauthorized();

        int? userId = ctx.Session.GetInt32("user_id");
        string? email = ctx.Session.GetString("email");
        string? role = ctx.Session.GetString("role");

        return Results.Ok(new Me_Data(userId, email, role));
    }
}