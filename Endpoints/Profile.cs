namespace server;
using MySql.Data.MySqlClient;

static class Profile
{
  public record Profile_Data(string Email, string Role);

  public static async Task<Profile_Data?>
  Get(Config config, HttpContext ctx)
  {
    // Get user_id from session on Authentication class

    int? user_id = Authentication.GetUserId(ctx);
    if (user_id is null)
      return null;

    // A query to get user information by id

    string query = """
            SELECT email, role
            FROM users
            WHERE id = @id
        """;

    var parameters = new MySqlParameter[]
    {
            new("@id", user_id)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    if (reader.Read())
    {
      string email = reader.GetString(reader.GetOrdinal("email")); // get email
      string role = reader.GetString(reader.GetOrdinal("role")); // get role

      return new Profile_Data(email, role); // the record Profile_Data
    }

    return null;
  }
}