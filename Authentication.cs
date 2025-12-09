namespace server;

class Authentication
{
  // Check if user is logged in by checking if the session is available
  public static bool IsLoggedIn(HttpContext ctx)
  {
    if (!ctx.Session.IsAvailable)
      return false;

    return ctx.Session.Keys.Contains("user_id");
  }

  // Get logged-in by user ID
  public static int? GetUserId(HttpContext ctx)
  {
    if (!ctx.Session.IsAvailable)
      return null;

    return ctx.Session.GetInt32("user_id");
  }

  // Get logged-in user role
  public static string? GetRole(HttpContext ctx)
  {
    if (!ctx.Session.IsAvailable)
      return null;

    return ctx.Session.GetString("role"); // As we used VARCHAR on role column
  }

  // Check if user is admin
  public static bool IsAdmin(HttpContext ctx)
  {
    string? role = GetRole(ctx);
    return role == "admin";
  }

  // No need for a user role GetRolr function, because we have this role as a default role
}
