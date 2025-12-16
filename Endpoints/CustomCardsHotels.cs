namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
CREATE TABLE custom_card_hotels(
        card_id INT NOT NULL,
        hotel_id INT NOT NULL,
        PRIMARY KEY (card_id, hotel_id),
        FOREIGN KEY (card_id) REFERENCES custom_cards(id),
        FOREIGN KEY (hotel_id) REFERENCES hotels(id)
      );

CREATE TABLE hotels (
          id INT AUTO_INCREMENT PRIMARY KEY,
          name VARCHAR(200) NOT NULL,
          phone_number VARCHAR(50),
          rating DECIMAL(3,1) NOT NULL,
          address VARCHAR(255) NOT NULL,
          description TEXT NOT NULL,
          destination_id INT NOT NULL,
          FOREIGN KEY (destination_id) REFERENCES destinations(id)
        );
*/
class CustomCardHotels
{

  // Link hotel <-> custom card
  public record Link_Args(int CardId, int HotelId);

  // All activities on one card
  public static async Task<IResult> ByCard(int cardId, Config config)
  {
    var result = new List<Hotels.GetAll_Data>();

    string query = """
                         SELECT h.id, h.name, h.phone_number, h.rating, h.address, h.description, h.destination_id
                         FROM hotels AS h
                         INNER JOIN custom_card_hotels AS ch ON ch.hotel_id = h.id
                         WHERE ch.card_id = @cardId
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId)
    };

    using var reader =
        await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0), // hotelId
          reader.GetString(1),// hotelName
          reader.GetString(2), //hotelPhN
          reader.GetDecimal(3), // rating
          reader.GetString(4), //address
          reader.GetString(5), // description
          reader.GetInt32(6) // destination_id

      ));
    }

    return Results.Ok(result);
  }

  // All custom cards that has an activity
  public static async Task<IResult> ByHotel(int hotelId, Config config)
  {
    var result = new List<CustomCards.GetAll_Data>();

    string query = """
                         SELECT c.id, c.user_id, c.title, c.budget, c.start_date, c.end_date
                         FROM custom_cards AS c
                         INNER JOIN custom_card_hotels AS ch ON ch.card_id = c.id
                         WHERE ch.hotel_id = @hotelId
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@hotelId", hotelId)
    };

    using var reader =
        await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0), // id
          reader.GetInt32(1), // user_id
          reader.GetString(2), // title
          reader.IsDBNull(3) ? null : reader.GetDecimal(3),// budget
          reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),//start date
          reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)) //End date
      ));
    }

    return Results.Ok(result);
  }

  // connect or link activity with custom card
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
    {
      return userAuth;
    }


    string query = """
                         INSERT INTO custom_card_hotels (card_id, hotel_id)
                         VALUES (@cardId, @hotelId)
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", link.CardId),
            new("@hotelId", link.HotelId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Hotel linked to custom card." });
  }

  // Delete the link
  public static async Task<IResult> Unlink(int cardId, int hotelId, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
    {
      return userAuth;
    }


    string query = """
                         DELETE FROM custom_card_hotels
                         WHERE card_id = @cardId
                           AND hotel_id = @hotelId
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId),
            new("@hotelId", hotelId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Hotel unlinked from custom card." });
  }

  public static async Task<IResult> Search(int cardId, string? term, Config config)
  {
    var result = new List<Hotels.GetAll_Data>();

    string query;
    MySqlParameter[] parameters;

    if (string.IsNullOrWhiteSpace(term))
    {
      // No term -> return all hotels for this card
      query = """
                SELECT h.id, h.name, h.phone_number, h.rating, h.address , h.description, h.destination_id
                FROM hotels AS h
                INNER JOIN custom_card_hotels AS ch ON ch.hotel_id = h.id
                WHERE ch.card_id = @cardId
                """;

      parameters = new MySqlParameter[]
      {
            new("@cardId", cardId)
      };
    }
    else
    {
      // Search by hotel name
      query = """
                SELECT h.id, h.name, h.phone_number, h.rating, h.address , h.description, h.destination_id
                FROM hotels AS h
                INNER JOIN custom_card_hotels AS ch ON ch.hotel_id = h.id
                WHERE ch.card_id = @cardId
                  AND h.name LIKE @term
                """;

      parameters = new MySqlParameter[]
      {
            new("@cardId", cardId),
            new("@term", "%" + term + "%")
      };
    }

    using var reader =
        await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0), // id
          reader.GetString(1), //name
          reader.GetString(2),// phN
          reader.GetDecimal(3), // rating
          reader.GetString(4), //address
          reader.GetString(5), // description
          reader.GetInt32(6) // destination_id

      ));
    }

    return Results.Ok(result);
  }

}//fatima