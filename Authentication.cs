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
  
  public static IResult? RequireAdmin(HttpContext ctx)
  {
    if (!IsLoggedIn(ctx))
      return Results.Unauthorized();

    if (!IsAdmin(ctx))
      return Results.Forbid();

    return null;
  }

  // No need for a user role GetRolr function, because we have this role as a default role
  // Now IsUser needed in order to allow just "inloggad" users to handel their bookings 

  // Check if user is admin
  public static bool IsUser(HttpContext ctx)
  {
    string? role = GetRole(ctx);
    return role == "user";
  }


  // A function or method to define the role and authentications

  public static IResult? RequireAdmin(HttpContext ctx)
  {
    if (!IsLoggedIn(ctx))
    {
      return Results.Unauthorized();
    }
    if (!IsAdmin(ctx))
    {
      return Results.Forbid();
    }

    return null;
  }

  // A function or method to define the role "Authorization" and authentications

  public static IResult? RequireUser(HttpContext ctx)
  {
    if (!IsLoggedIn(ctx))
    {
      return Results.Unauthorized();
    }
    if (!IsUser(ctx))
    {
      return Results.Forbid();
    }

    return null;
  }

}



