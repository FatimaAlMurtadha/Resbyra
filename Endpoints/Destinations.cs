namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using server;

class Destinations
{
  // DTO 

  public record GetAll_Data(int Id, string Description, string Climate, decimal AverageCost, int CityId);

  // GET / destinations
  // Bring all destinations

  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    string query = "SELECT id, description, climate, average_cost, city_id FROM destinations";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32("id"),
            reader.GetString("description"),
            reader.GetString("climate"),
            reader.GetDecimal("average_cost"),
            reader.GetInt32("city_id")
        ));
      }
    }
    return Results.Ok(result);
  }

  // DTO
  public record Get_Data(string Description, string Climate, decimal AverageCost, int CityId);

  // GET / Destinations by /{id}
  // Fetch a specific destination by its ID

  public static async Task<IResult> Get(int id, Config config)
  {
    Get_Data? result = null;

    string query = "SELECT description, climate, average_cost, city_id FROM destinations WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        result = new(
            reader.GetString("description"),
            reader.GetString("climate"),
            reader.GetDecimal("average_cost"),
            reader.GetInt32("city_id")
        );
      }
    }

    if (result is null)
    {
      return Results.NotFound(new { message = $"Destination with id {id} was not found." });
    }

    return Results.Ok(result);
  }

  // DTO for POST
  public record Post_Args(string Description, string Climate, decimal AverageCost, int CityId);

  // POST/ Destinations
  // Insert a new destination into db

  public static async Task<IResult> Post(Post_Args destination, Config config)
  {
    string query = """
            INSERT INTO destinations(description, climate, average_cost, city_id)
            VALUES (@description, @climate, @average_cost, @city_id)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@description", destination.Description),
            new("@climate", destination.Climate),
            new("@average_cost", destination.AverageCost),
            new("@city_id", destination.CityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination created successfully." });
  }

  // DTO for PUT (updating an existing city)
  public record Put_Args(int Id, string Description, string Climate, decimal AverageCost, int CityId);

  // PUT /destinations/{id}
  // Update an existing destination
  public static async Task<IResult> Put(Put_Args destination, Config config)
  {
    string query = """
                       UPDATE destinations
                       SET description = @description, climate = @climate, average_cost = @average_cost,
                           city_id = @city_id
                       WHERE id = @id
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@id", destination.Id),
      new("@description", destination.Description),
      new("@climate", destination.Climate),
      new("@average_cost", destination.AverageCost),
      new("@city_id", destination.CityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination updated successfully." });
  }

  // DELETE /destination by /{id}
  // Remove a destination from the database
  public static async Task<IResult> Delete(int id, Config config)
  {
    string query = "DELETE FROM destinations WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination deleted successfully." });
  }

  
  
  // GET /destinations/search?term=beach
  // FoodActivity-style search: tom term = alla, annars LIKE på description/climate
  public static async Task<IResult> Search(string? term, Config config)
  {
    List<GetAll_Data> result = new();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
      // Ingen term -> alla destinations
      query = "SELECT id, description, climate, average_cost, city_id FROM destinations";
    }
    else
    {
      query = """
              SELECT id, description, climate, average_cost, city_id
              FROM destinations
              WHERE description LIKE @term
                 OR climate LIKE @term
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
            reader.GetString("description"),
            reader.GetString("climate"),
            reader.GetDecimal("average_cost"),
            reader.GetInt32("city_id")
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
            reader.GetString("description"),
            reader.GetString("climate"),
            reader.GetDecimal("average_cost"),
            reader.GetInt32("city_id")
          ));
        }
      }
    }

    return Results.Ok(result);
  }

  
  

}


 
