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
  // FoodActivity-style search
  public static async Task<IResult> Search(string? term, Config config)
  {
    List<GetAll_Data> result = new();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
      // Ingen term -> visa alla länder
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
}