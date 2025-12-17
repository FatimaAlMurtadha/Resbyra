namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;


class Destinations
{
  // DTO 

  public record GetAll_Data(int Id, string Name , string Description, string Climate, decimal AverageCost, int CityId);

  // GET / destinations
  // Bring all destinations

  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    string query = "SELECT id, name,description, climate, average_cost, city_id FROM destinations";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0),//id
            reader.GetString(1),//name
            reader.GetString(2),//description
            reader.GetString(3),//climate
            reader.GetDecimal(4),//avrage_cost
            reader.GetInt32(5)//city_id
        ));
      }
    }
    return Results.Ok(result);
  }

  // DTO
  public record Get_Data(string Description, string Name ,string Climate, decimal AverageCost, int CityId);

  // GET / Destinations by /{id}
  // Fetch a specific destination by its ID

  public static async Task<IResult> Get(int id, Config config)
  {
    Get_Data? result = null;

    string query = "SELECT name, description, climate, average_cost, city_id FROM destinations WHERE id = @id";

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
  public record Post_Args(string Name, string Description , string Climate, decimal AverageCost, int CityId);

  // POST/ Destinations
  // Insert a new destination into db

  public static async Task<IResult> Post(Post_Args destination, Config config, HttpContext ctx)
  {
    // To post a destination "Add" is admin's feature 
    // So we need to make it (only Admin) access
    // Throug calling our authentication function or method

    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Chech 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    // End othorization

    string query = """
            INSERT INTO destinations(description, climate, average_cost, city_id)
            VALUES (@description, @climate, @average_cost, @city_id)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@name", destination.Name),
            new("@description", destination.Description),
            new("@climate", destination.Climate),
            new("@average_cost", destination.AverageCost),
            new("@city_id", destination.CityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination created successfully." });
  }

  // DTO for PUT (updating an existing city)
  public record Put_Args(int Id, string Name , string Description, string Climate, decimal AverageCost, int CityId);

  // PUT /destinations/{id}
  // Update an existing destination
  public static async Task<IResult> Put(Put_Args destination, Config config, HttpContext ctx)
  {
    // To put a destination "update, edite" is admin's feature 
    // So we need to make it (only Admin) access
    // Throug calling our authentication function or method

    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Chech 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    // End of authorization
    string query = """
            UPDATE destinations
            SET name = @name , description = @description, climate = @climate, average_cost = @average_cost,
            city_id = @city_id
            WHERE id = @id
        """;

    var parameters = new MySqlParameter[]
    {
            new("@id", destination.Id),
            new("@name", destination.Name),
            new("@destination", destination.Description),
            new("@climate", destination.Climate),
            new("@average_cost", destination.AverageCost),
            new("@city_id", destination.CityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination updated successfully." });
  }

  // DELETE /destination by /{id}
  // Remove a destination from the database
  public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
  {

    // To delete a destination is admin's feature 
    // So we need to make it (only Admin) access
    // Throug calling our authentication function or method

    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Chech 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    // End of authorization

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
      query = "SELECT id, name, description, climate, average_cost, city_id FROM destinations";
    }
    else
    {
      query = """
              SELECT id, name , description, climate, average_cost, city_id
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
            reader.GetString("name"),
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
            reader.GetString("name"),
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


 
