namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
/*
 CREATE TABLE rooms (
      id INT AUTO_INCREMENT PRIMARY KEY,
      room_number INT NOT NULL,
      type VARCHAR(100) NOT NULL,
      price_per_night DECIMAL(10,2) NOT NULL,
      capacity INT NOT NULL,
      hotel_id INT NOT NULL,
      FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    );
*/
class CustomCardsRooms
{
  
  public record Link_Args(int CardId, int RoomId);

  // Get all rooms linked to a card
  public static async Task<IResult> ByCard(int cardId, Config config)
  {
    var result = new List<Rooms.GetAll_Data>();

    string query = """
            SELECT r.id, r.room_number, r.type, r.price_per_night, r.capacity, r.hotel_id
            FROM rooms AS r
            INNER JOIN custom_card_rooms AS cr ON cr.room_id = r.id
            WHERE cr.card_id = @cardId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),
          reader.GetInt32(1),
          reader.GetString(2),
          reader.GetDecimal(3),
          reader.GetInt32(4),
          reader.GetInt32(5)
      ));
    }

    return Results.Ok(result);
  }

  // Get all custom cards linked to a room
  public static async Task<IResult> ByRoom(int roomId, Config config)
  {
    var result = new List<CustomCards.GetAll_Data>();

    string query = """
            SELECT c.id, c.user_id, c.title, c.budget, c.start_date, c.end_date
            FROM custom_cards AS c
            INNER JOIN custom_card_rooms AS cr ON cr.card_id = c.id
            WHERE cr.room_id = @roomId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@roomId", roomId)
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

  // Link room to card
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
      return userAuth;

    string query = """
            INSERT INTO custom_card_rooms (card_id, room_id)
            VALUES (@cardId, @roomId)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", link.CardId),
            new("@roomId", link.RoomId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Room linked to custom card." });
  }

  // Unlink room from card
  public static async Task<IResult> Unlink(int cardId, int roomId, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
      return userAuth;

    string query = """
            DELETE FROM custom_card_rooms
            WHERE card_id = @cardId
              AND room_id = @roomId
        """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId),
            new("@roomId", roomId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Room unlinked from custom card." });
  }

  // Search rooms linked to a card
  public static async Task<IResult> Search(int cardId, string? term, Config config)
  {
    var result = new List<Rooms.GetAll_Data>();

    string query = """
                SELECT r.id, r.room_number, r.type, r.price_per_night, r.capacity, r.hotel_id
                FROM rooms AS r
                INNER JOIN custom_card_rooms AS cr ON cr.room_id = r.id
                WHERE cr.card_id = @cardId
            """;

      var parameters = new MySqlParameter[]
      {
                new("@cardId", cardId)
      };
    
    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),
          reader.GetInt32(1),
          reader.GetString(2),
          reader.GetDecimal(3),
          reader.GetInt32(4),
          reader.GetInt32(5)
      ));
    }

    return Results.Ok(result);
  }


}//fatima