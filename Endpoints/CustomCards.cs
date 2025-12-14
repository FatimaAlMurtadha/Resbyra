namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
/*
CREATE TABLE custom_cards (
            id INT AUTO_INCREMENT PRIMARY KEY,
            user_id INT NOT NULL,
            title VARCHAR(200) NOT NULL,
            budget DECIMAL(10,2),
            start_date DATE,
            end_date DATE,
            FOREIGN KEY (user_id) REFERENCES users(id)
        );
*/
class CustomCards
{
  // DTO for returning a list of cards (GET /custom-cards)
  public record GetAll_Data(int Id, int UserId, string Title, decimal? Budget, DateOnly? StartDate, DateOnly? EndDate);

  // GET /custom-cards
  // Bring all custom cards from the database
  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    string query = "SELECT id, user_id, title, budget, start_date, end_date FROM custom_cards";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(3),
            reader.GetDecimal(4),
            reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
            reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6))
        ));
      }
    }

    return Results.Ok(result);
  }

  // DTO for returning a single card (GET /custom-cards/{id})
  public record Get_Data(int UserId, string Title, decimal? Budget, DateOnly? StartDate, DateOnly? EndDate);

  // GET /custom-cards/{id}
  // Fetch a specific card by its ID
  public static async Task<IResult> Get(int id, Config config)
  {
    Get_Data? result = null;

    string query = "SELECT user_id, title, budget, start_date, end_date FROM custom_cards WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        result = new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetDecimal(2),
            reader.IsDBNull(3) ? null : DateOnly.FromDateTime(reader.GetDateTime(3)),
            reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4))
        );
      }
    }

    if (result is null)
    {
      return Results.NotFound(new { message = $"Custom card with id {id} was not found." });
    }

    return Results.Ok(result);
  }

  // DTO for POST (creating a new card)
  public record Post_Args(int UserId, string Title, decimal? Budget, DateOnly? StartDate, DateOnly? EndDate);

  // POST /custom-cards
  // Insert a new custom card into the database
  public static async Task<IResult> Post(Post_Args card, Config config, HttpContext ctx)
  {
    var user_authentication = Authentication.RequireUser(ctx);

    if (user_authentication is not null)
    {
      return user_authentication;
    }

    string query = """
            INSERT INTO custom_cards (user_id, title, budget, start_date, end_date)
            VALUES (@id, @title, @budget, @start, @end)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@id", card.UserId),
            new("@title", card.Title),
            new("@budget", card.Budget),
            new("@start", card.StartDate?.ToDateTime(TimeOnly.MinValue)),
            new("@end", card.EndDate?.ToDateTime(TimeOnly.MinValue))
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Custom card created successfully." });
  }

  // DTO for PUT (updating an existing card)
  public record Put_Args(int Id, int UserId, string Title, decimal? Budget, DateOnly? StartDate, DateOnly? EndDate);

  // PUT /custom-cards/{id}
  // Update an existing custom card
  public static async Task<IResult> Put(Put_Args card, Config config, HttpContext ctx)
  {
    var user_authentication = Authentication.RequireUser(ctx);

    if (user_authentication is not null)
    {
      return user_authentication;
    }

    string query = """
            UPDATE custom_cards
            SET user_id = @id,
                title = @title,
                budget = @budget,
                start_date = @start,
                end_date = @end
            WHERE id = @id
        """;

    var parameters = new MySqlParameter[]
    {
            new("@id", card.Id),
            new("@id", card.UserId),
            new("@title", card.Title),
            new("@budget", card.Budget),
            new("@start", card.StartDate?.ToDateTime(TimeOnly.MinValue)),
            new("@end", card.EndDate?.ToDateTime(TimeOnly.MinValue))
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Custom card updated successfully." });
  }

  // DELETE /custom-cards/{id}
  // Remove a custom card from the database
  public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
  {
    var user_authentication = Authentication.RequireUser(ctx);

    if (user_authentication is not null)
    {
      return user_authentication;
    }

    string query = "DELETE FROM custom_cards WHERE id = @id";

    var parameters = new MySqlParameter[]
    {
            new("@id", id)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Custom card deleted successfully." });
  }

  // GET /custom-cards/search?term=...
  // Search custom cards by title (FoodActivity-style search)
  public static async Task<IResult> Search(string? term, Config config)
  {
    List<GetAll_Data> result = new();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
      // No term -> return all cards
      query = "SELECT id, user_id, title, budget, start_date, end_date FROM custom_cards";
    }
    else
    {
      query = """
            SELECT id, user_id, title, budget, start_date, end_date
            FROM custom_cards
            WHERE title LIKE @term
        """;

      parameters = new MySqlParameter[]
      {
            new("@term", "%" + term + "%")
      };
    }

    if (parameters is null)
    {
      using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query);
      while (reader.Read())
      {
        result.Add(new GetAll_Data(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetDecimal(3),
            reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),
            reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5))
        ));
      }
    }
    else
    {
      using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);
      while (reader.Read())
      {
        result.Add(new GetAll_Data(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetDecimal(3),
            reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),
            reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5))
        ));
      }
    }

    return Results.Ok(result);
  }
} //fatima