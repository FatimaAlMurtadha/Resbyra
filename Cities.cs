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
  public static async Task<IResult> Post(Post_Args city, Config config)
  {
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
  public static async Task<IResult> Put(Put_Args city, Config config)
  {
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
  public static async Task<IResult> Delete(int id, Config config)
  {
    string query = "DELETE FROM cities WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "City deleted successfully." });
  }
}
