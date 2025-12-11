namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class Cities
{
  // DTO for returning a list of cities (GET /cities)
  public record GetAll_Data(int Id, string Name, int CountryId);
  // GET /cities
 // Bring all cities from the database
 
  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    string query = "SELECT id, name, country_id FROM cities";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32("id"),
            reader.GetString("name"),
            reader.GetInt32("country_id")
        ));
      }
    }

    return Results.Ok(result);
  }

  // DTO for returning a single city (GET /cities/{id})
  public record Get_Data(string Name, int CountryId);

  // GET /cities by /{id}
  // Fetch a specific city by its ID
  public static async Task<IResult> Get(int id, Config config)
  {
    Get_Data? result = null;

    string query = "SELECT name, country_id FROM cities WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        result = new(
            reader.GetString("name"),
            reader.GetInt32("country_id")
        );
      }
    }

    if (result is null)
    {
      return Results.NotFound(new { message = $"City with id {id} was not found." });
    }

    return Results.Ok(result);
  }

  // DTO for POST (creating a new city)
  public record Post_Args(string Name, int CountryId);


  // POST /cities 
  // Insert a new city into the database
  // We need to use the thierd parameter this time 
  // In order to check the role
  public static async Task<IResult> Post(Post_Args city, Config config, HttpContext ctx)
  {
    // To post a city "Add" is admin's feature 
    // So we need to make it (only Admin) access
    // Throug calling our authentication function or method

    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Chech 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    // End of authentication
    string query = """
            INSERT INTO cities(name, country_id)
            VALUES (@name, @country_id)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@name", city.Name),
            new("@country_id", city.CountryId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "City created successfully." });
  }

  // DTO for PUT (updating an existing city)

  public record Put_Args(int Id, string Name, int CountryId);


  // PUT /cities/{id}
  // Update an existing city
  // The same should be applyed to Put function as on POST
  // Only admin 
  public static async Task<IResult> Put(Put_Args city, Config config, HttpContext ctx)
  {
    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Chech 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }


    string query = """
            UPDATE cities
            SET name = @name, country_id = @country_id
            WHERE id = @id
        """;

    var parameters = new MySqlParameter[]
    {
            new("@id", city.Id),
            new("@name", city.Name),
            new("@country_id", city.CountryId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "City updated successfully." });
  }

  // DELETE /cities by /{id}
  // Remove a city from the database
  // Only admin
  public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
  {
    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Chech 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    string query = "DELETE FROM cities WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "City deleted successfully." });
  }
  
  
  // GET /cities/search?term=stock
  // FoodActivity-style search
  public static async Task<IResult> Search(string? term, Config config)
  {
    List<GetAll_Data> result = new();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
      // Ingen term -> alla cities
      query = "SELECT id, name, country_id FROM cities";
    }
    else
    {
      query = """
              SELECT id, name, country_id
              FROM cities
              WHERE name LIKE @term
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
            reader.GetString("name"),
            reader.GetInt32("country_id")
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
            reader.GetInt32("id"),
            reader.GetString("name"),
            reader.GetInt32("country_id")
          ));
        }
      }
    }

    return Results.Ok(result);
  }
  
  
  
}
