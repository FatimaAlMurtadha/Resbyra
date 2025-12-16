namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
CREATE TABLE amenities(
      id INT AUTO_INCREMENT PRIMARY KEY,
      amenity_name VARCHAR(300)
    );
*/
class CustomCardsAmenities
{

  // Link amenity <-> custom card
  public record Link_Args(int CardId, int AmenityId);

  // Get all amenities linked to a card
  public static async Task<IResult> ByCard(int cardId, Config config)
  {
    var result = new List<Amenities.GetAll_Data>();

    string query = """
            SELECT a.id, a.amenity_name
            FROM amenities AS a
            INNER JOIN custom_card_amenities AS ca ON ca.amenity_id = a.id
            WHERE ca.card_id = @cardId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),   // id
          reader.GetString(1)   // amenity_name
      ));
    }

    return Results.Ok(result);
  }

  // Get all custom cards linked to an amenity
  public static async Task<IResult> ByAmenity(int amenityId, Config config)
  {
    var result = new List<CustomCards.GetAll_Data>();

    string query = """
            SELECT c.id, c.user_id, c.title, c.budget, c.start_date, c.end_date
            FROM custom_cards AS c
            INNER JOIN custom_card_amenities AS ca ON ca.card_id = c.id
            WHERE ca.amenity_id = @amenityId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@amenityId", amenityId)
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

  // Link amenity to card
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
      return userAuth;

    string query = """
            INSERT INTO custom_card_amenities (card_id, amenity_id)
            VALUES (@cardId, @amenityId)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", link.CardId),
            new("@amenityId", link.AmenityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity linked to custom card." });
  }

  // Unlink amenity from card
  public static async Task<IResult> Unlink(int cardId, int amenityId, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
      return userAuth;

    string query = """
            DELETE FROM custom_card_amenities
            WHERE card_id = @cardId
              AND amenity_id = @amenityId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId),
            new("@amenityId", amenityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity unlinked from custom card." });
  }

  // Search amenities linked to a card (simple version)
  public static async Task<IResult> Search(int cardId, string? term, Config config)
  {
    var result = new List<Amenities.GetAll_Data>();

    string query;
    MySqlParameter[] parameters;

    if (string.IsNullOrWhiteSpace(term))
    {
      query = """
                SELECT a.id, a.amenity_name
                FROM amenities AS a
                INNER JOIN custom_card_amenities AS ca ON ca.amenity_id = a.id
                WHERE ca.card_id = @cardId
            """;

      parameters = new MySqlParameter[]
      {
                new("@cardId", cardId)
      };
    }
    else
    {
      query = """
                SELECT a.id, a.amenity_name
                FROM amenities AS a
                INNER JOIN custom_card_amenities AS ca ON ca.amenity_id = a.id
                WHERE ca.card_id = @cardId
                  AND a.amenity_name LIKE @term
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
          reader.GetString(1)
      ));
    }

    return Results.Ok(result);
  }


} 