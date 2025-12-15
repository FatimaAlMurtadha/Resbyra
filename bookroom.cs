namespace server;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class BookRoom
{
    // DTO
    public record GetAll_Data(int Id, int UserId, int TravelId, int RoomId, DateTime CheckInDate, DateTime CheckOutDate, decimal TotalPrice);

    // GET all bookings
    public static async Task<IResult> GetAll(Config config)
    {
        List<GetAll_Data> result = new();
        string query = """
            SELECT id, user_id, travel_id, room_id, check_in, check_out, total_price
            FROM bookroom
        """;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32("id"),
                    reader.GetInt32("user_id"),
                    reader.GetInt32("travel_id"),
                    reader.GetInt32("room_id"),
                    reader.GetDateTime("check_in"),
                    reader.GetDateTime("check_out"),
                    reader.GetDecimal("total_price")
                ));
            }
        }
        return Results.Ok(result);
    }

    // GET booking by id
    public static async Task<IResult> Get(int id, Config config)
    {
        string query = @"SELECT user_id, travel_id, room_id, check_in, check_out, total_price 
                         FROM bookroom WHERE id = @id";
        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                var result = new
                {
                    UserId = reader.GetInt32("user_id"),
                    TravelId = reader.GetInt32("travel_id"),
                    RoomId = reader.GetInt32("room_id"),
                    CheckIn = reader.GetDateTime("check_in"),
                    CheckOut = reader.GetDateTime("check_out"),
                    TotalPrice = reader.GetDecimal("total_price")
                };
                return Results.Ok(result);
            }
        }
        return Results.NotFound(new { message = "Booking not found." });
    }

    // POST
    public record BookingRoomCreate(int UserId, int TravelId, int RoomId, DateTime CheckInDate, DateTime CheckOutDate, decimal TotalPrice);

    public static async Task<IResult> Post(BookingRoomCreate bookingRoom, Config config, HttpContext ctx)
    {
        string query = @"INSERT INTO bookroom (user_id, travel_id, room_id, check_in, check_out, total_price)
                         VALUES (@user_id, @travel_id, @room_id, @check_in, @check_out, @total_price)";
        var parameters = new MySqlParameter[]
        {
            new("@user_id", bookingRoom.UserId),
            new("@travel_id", bookingRoom.TravelId),
            new("@room_id", bookingRoom.RoomId),
            new("@check_in", bookingRoom.CheckInDate),
            new("@check_out", bookingRoom.CheckOutDate),
            new("@total_price", bookingRoom.TotalPrice)
        };
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok(new { message = "Room booked successfully." });
    }
//Put
public record BookingRoomUpdate(int BookingId, int RoomId, int TravelId, DateTime CheckIn, DateTime CheckOut, decimal TotalPrice);

public static async Task<IResult> Put(BookingRoomUpdate bookingRoom, Config config, HttpContext ctx)
{
    // Admin authentication
    var admin_authentication = Authentication.RequireAdmin(ctx);
    if (admin_authentication is not null)
        return admin_authentication;
    string query = """
        UPDATE booking_rooms
        SET
            travel_id = @travel_id,
            check_in = @check_in,
            check_out = @check_out,
            total_price = @total_price
        WHERE booking_id = @booking_id
          AND room_id = @room_id
    """;
    var parameters = new MySqlParameter[]
    {
        new("@booking_id", bookingRoom.BookingId),
        new("@room_id", bookingRoom.RoomId),
        new("@travel_id", bookingRoom.TravelId),
        new("@check_in", bookingRoom.CheckIn),
        new("@check_out", bookingRoom.CheckOut),
        new("@total_price", bookingRoom.TotalPrice)
    };
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Booking room updated." });
}
    // DELETE
    public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
    {
        // Admin authentication
        var admin_authentication = Authentication.RequireAdmin(ctx);
        if (admin_authentication is not null)
        {
            return admin_authentication;
        }

        string query = "DELETE FROM bookroom WHERE id = @id";
        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok(new { message = "Booking deleted." });
    }
}

