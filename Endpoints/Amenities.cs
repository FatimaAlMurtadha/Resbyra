namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class Amenities
{
  // DTO for returning a list of amenities (GET /amenities)
  public record GetAll_Data(int Id, string AmenityName);

  // GET /amenities
  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    string query = "SELECT id, amenity_name FROM amenities";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32("id"),
            reader.GetString("amenity_name")
        ));
      }
    }

    return Results.Ok(result);
  }


  // DTO for returning a single amenity
  public record Get_Data(string AmenityName);

  // GET /amenities/{id}
  public static async Task<IResult> Get(int id, Config config)
  {
    Get_Data? result = null;

    string query = "SELECT amenity_name FROM amenities WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
      new("@id", id)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        result = new(
            reader.GetString("amenity_name")
        );
      }
    }

    if (result is null)
    {
      return Results.NotFound(new { message = $"Amenity with id {id} was not found." });
    }

    return Results.Ok(result);
  }


  // GET /amenities/search?term=...
  public static async Task<IResult> Search(string? term, Config config)
  {
    List<GetAll_Data> result = new();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
      query = "SELECT id, amenity_name FROM amenities";
    }
    else
    {
      query = """
              SELECT id, amenity_name
              FROM amenities
              WHERE amenity_name LIKE @term
              """;

      parameters = new MySqlParameter[]
      {
        new("@term", "%" + term + "%")
      };
    }

    if (parameters is null)
    {
      using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
      {
        while (reader.Read())
        {
          result.Add(new(
              reader.GetInt32("id"),
              reader.GetString("amenity_name")
          ));
        }//jag håller på att somna zzzzzz
      }
    }
    else
    {
      using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
      {
        while (reader.Read())
        {
          result.Add(new(
              reader.GetInt32("id"),
              reader.GetString("amenity_name")
          ));
        }
      }
    }

    return Results.Ok(result);
  }


  // DTO for POST
  public record Post_Args(string AmenityName);

  // POST /amenities
  public static async Task<IResult> Post(Post_Args amenity, Config config, HttpContext ctx)
  {
    var admin_auth = Authentication.RequireAdmin(ctx);
    if (admin_auth is not null)
      return admin_auth;

    string query = """
            INSERT INTO amenities(amenity_name)
            VALUES (@name)
        """;

    var parameters = new MySqlParameter[]
    {
      new("@name", amenity.AmenityName)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity created successfully." });
  }


  // DTO for PUT
  public record Put_Args(int Id, string AmenityName);

  // PUT /amenities/{id}
  public static async Task<IResult> Put(Put_Args amenity, Config config, HttpContext ctx)
  {
    var admin_auth = Authentication.RequireAdmin(ctx);
    if (admin_auth is not null)
      return admin_auth;

    string query = """
            UPDATE amenities
            SET amenity_name = @name
            WHERE id = @id
        """;

    var parameters = new MySqlParameter[]
    {
      new("@id", amenity.Id),
      new("@name", amenity.AmenityName)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity updated successfully." });
  }


  // DELETE /amenities/{id}
  public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
  {
    var admin_auth = Authentication.RequireAdmin(ctx);
    if (admin_auth is not null)
      return admin_auth;

    string query = "DELETE FROM amenities WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
      new("@id", id)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity deleted successfully." });
  }
}
