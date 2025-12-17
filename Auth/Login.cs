namespace server;

using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;


class Login
{
  // Logout 
  public static void Delete(HttpContext ctx)
  {
    if (ctx.Session.IsAvailable)
    {
      ctx.Session.Clear();
    }
  }

  // Check if logged in 
  public static bool Get(HttpContext ctx)
  {
    if (!ctx.Session.IsAvailable)
    {
      return false; 
    }
    return ctx.Session.Keys.Contains("user_id"); 

  }

  public record Post_Data(string Email, string Password);

  // Here is log in function 
  public static async Task<bool> Post(Post_Data credentials, Config config, HttpContext ctx)
  {
    bool result = false;

    // Get user by email
    string query = "SELECT id, password, role FROM users WHERE email = @Email"; // I stoped here

    var parameters = new MySqlParameter[]
    {
      new("@email", credentials.Email),
      // needed to check the password with same function on Registeration.cs 

      // new("@password", credentials.Password),
    };

    // needed to use reader to get id, password and role

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        int id = reader.GetInt32(reader.GetOrdinal("id")); // id
        string storedHash = reader.GetString(reader.GetOrdinal("password")); // password
        string role = reader.GetString(reader.GetOrdinal("role")); // role

        // Check incoming password (same as Registeration)
        string incomingHash = HashPassword(credentials.Password);

        // Compare
        if (incomingHash == storedHash)
        {
          // Save session
          ctx.Session.SetInt32("user_id", id);
          ctx.Session.SetString("role", role);
          ctx.Session.SetString("email", credentials.Email);
          result = true;
        }
      }
    }

    return result;
  }

  // Using the same hashing function as Registeration.cs
  private static string HashPassword(string password)
  {
    using var sha = SHA256.Create();
    byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
  }



  // needed to comment this query and will use reader instead in order to read all the values not only ID
  /*object query_result = await MySqlHelper.ExecuteScalarAsync(config.ConnectionString, query, parameters);
  if (query_result is int id)
  {
    ctx.Session.SetInt32("user_id", id);
    result = true;
  }

  return result;
}*/



}

//
