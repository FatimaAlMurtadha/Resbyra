namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class Countries
{
  // DTO för listresultat
  public record GetAll_Data(int Id, string CountryName);

  // DTO för single result
  public record Get_Data(string CountryName);

  // GET /countries
  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    string query = "SELECT id, country_name FROM countries";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0),
            reader.GetString(1)
        ));
      }
    }

    return Results.Ok(result);
  }

  // GET /countries/{id}
  public static async Task<IResult> Get(int id, Config config)
  {
    Get_Data? result = null;

    string query = "SELECT country_name FROM countries WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
      new("@id", id)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        result = new(reader.GetString(0));
      }
    }

    if (result is null)
    {
      return Results.NotFound(new
      {
        message = $"Country with id {id} was not found."
      });
    }

    return Results.Ok(result);
  }

  // GET /countries/search?term=...
  public static async Task<IResult> Search(string? term, Config config)
  {
    List<GetAll_Data> result = new();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
      // Ingen term -> alla länder
      query = "SELECT id, country_name FROM countries";
    }
    else
    {
      query = """
              SELECT id, country_name
              FROM countries
              WHERE country_name LIKE @term
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
              reader.GetInt32(0),
              reader.GetString(1)
          ));
        }
      }
    }
    else
    {
      using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
      {
        while (reader.Read())
        {
          result.Add(new(
              reader.GetInt32(0),
              reader.GetString(1)
          ));
        }
      }
    }

    return Results.Ok(result);
  }

  // DTO för POST
  public record Post_Args(string CountryName);

  // POST /countries
  // Skapa nytt land  snälla dubbel kolla så jag gjorde rätt Fatima, justera det annars så det blir bra :) /Oskar
  public static async Task<IResult> Post(Post_Args country, Config config, HttpContext ctx)
  {
    var admin_authentication = Authentication.RequireAdmin(ctx);
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }

    string query = """
            INSERT INTO countries (country_name)
            VALUES (@name)
        """;

    var parameters = new MySqlParameter[]
    {
      new("@name", country.CountryName)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Country created successfully." });
  }

  // DTO för PUT
  public record Put_Args(int Id, string CountryName);

  // PUT /countries/{id}
  // Uppdatera land –  snälla dubbel kolla så jag gjorde rätt Fatima, justera det annars så det blir bra :) /Oskar
  public static async Task<IResult> Put(Put_Args country, Config config, HttpContext ctx)
  {
    var admin_authentication = Authentication.RequireAdmin(ctx);
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }

    string query = """
            UPDATE countries
            SET country_name = @name
            WHERE id = @id
        """;

    var parameters = new MySqlParameter[]
    {
      new("@id", country.Id),
      new("@name", country.CountryName)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Country updated successfully." });
  }

  // DELETE /countries/{id}
  // Ta bort land – bara admin, snälla dubbel kolla så jag gjorde rätt Fatima, justera det annars så det blir bra :) /Oskar
  public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
  {
    var admin_authentication = Authentication.RequireAdmin(ctx);
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }

    string query = "DELETE FROM countries WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
      new("@id", id)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Country deleted successfully." });
  }
} 