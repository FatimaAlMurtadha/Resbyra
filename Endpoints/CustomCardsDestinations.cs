namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
/*

      CREATE TABLE destinations (
      id INT AUTO_INCREMENT PRIMARY KEY,
      name VARCHAR(200) NOT NULL,
      description TEXT NOT NULL,
      climate VARCHAR(100) NOT NULL,
      average_cost DECIMAL(10,2) NOT NULL,
      city_id INT NOT NULL,
      FOREIGN KEY (city_id) REFERENCES cities(id)
    );
*/
class CustomCardsDestinations
{
  // Link destination <-> custom card
  public record Link_Args(int CardId, int DestinationId);

  // Get all destinations linked to a card
  public static async Task<IResult> ByCard(int cardId, Config config)
  {
    var result = new List<Destinations.GetAll_Data>();

    string query = """
            SELECT d.id, d.name, d.description, d.climate, d.average_cost, d.city_id
            FROM destinations AS d
            INNER JOIN custom_card_destinations AS cd ON cd.destination_id = d.id
            WHERE cd.card_id = @cardId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),// id
          reader.GetString(1),// name
          reader.GetString(2),// description
          reader.GetString(3),// climate
          reader.GetDecimal(4),// average_cost
          reader.GetInt32(5)// city_id
      ));
    }

    return Results.Ok(result);
  }

  //  Get all custom cards linked to a destination
  public static async Task<IResult> ByDestination(int destinationId, Config config)
  {
    var result = new List<CustomCards.GetAll_Data>();

    string query = """
            SELECT c.id, c.user_id, c.title, c.budget, c.start_date, c.end_date
            FROM custom_cards AS c
            INNER JOIN custom_card_destinations AS cd ON cd.card_id = c.id
            WHERE cd.destination_id = @destinationId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@destinationId", destinationId)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),
          reader.GetInt32(1),
          reader.GetString(2),
          reader.IsDBNull(3) ? null : reader.GetDecimal(3),
          reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),
          reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5))
      ));
    }

    return Results.Ok(result);
  }

  // Link destination to card
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
      return userAuth;

    string query = """
            INSERT INTO custom_card_destinations (card_id, destination_id)
            VALUES (@cardId, @destinationId)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", link.CardId),
            new("@destinationId", link.DestinationId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination linked to custom card." });
  }

  // Unlink destination from card
  public static async Task<IResult> Unlink(int cardId, int destinationId, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
      return userAuth;

    string query = """
            DELETE FROM custom_card_destinations
            WHERE card_id = @cardId
              AND destination_id = @destinationId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId),
            new("@destinationId", destinationId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination unlinked from custom card." });
  }

  // Search destinations linked to a card
  public static async Task<IResult> Search(int cardId, string? term, Config config)
  {
    var result = new List<Destinations.GetAll_Data>();

    string query;
    MySqlParameter[] parameters;

    if (string.IsNullOrWhiteSpace(term))
    {
      query = """
                SELECT d.id, d.name, d.description, d.climate, d.average_cost, d.city_id
                FROM destinations AS d
                INNER JOIN custom_card_destinations AS cd ON cd.destination_id = d.id
                WHERE cd.card_id = @cardId
            """;

      parameters = new MySqlParameter[]
      {
                new("@cardId", cardId)
      };
    }
    else
    {
      query = """
                SELECT d.id, d.name, d.description, d.climate, d.average_cost, d.city_id
                FROM destinations AS d
                INNER JOIN custom_card_destinations AS cd ON cd.destination_id = d.id
                WHERE cd.card_id = @cardId
                  AND d.name LIKE @term
            """;

      parameters = new MySqlParameter[]
      {
                new("@cardId", cardId),
                new("@term", "%" + term + "%")
      };
    }

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),
          reader.GetString(1),
          reader.GetString(2),
          reader.GetString(3),
          reader.GetDecimal(4),
          reader.GetInt32(5)
      ));
    }

    return Results.Ok(result);
  }


}//fatima



/*
program.cs

// CustomCardsDestinations ROUTES

app.MapGet("/custom-cards-destinations/card/{cardId}", CustomCardsDestinations.ByCard);

app.MapGet("/custom-cards-destinations/destination/{destinationId}", CustomCardsDestinations.ByDestination);

app.MapPost("/custom-cards-destinations", CustomCardsDestinations.Link);

app.MapDelete("/custom-cards-destinations/{cardId}/{destinationId}", CustomCardsDestinations.Unlink);

app.MapGet("/custom-cards-destinations/search", CustomCardsDestinations.Search);


*/