namespace server;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
class BookRoom
{

    //DTO
    public record GetAll_Data(int Id,int UserId,int RoomId,DateTime CheckInDate,DateTime CheckOutDate,int Guests,decimal TotalPrice);
    //GET BOokroom
      public static async Task<IResult> GetAll(Config config)
      {
        List<GetAll_Data> result = new();

        string query = @"SELECT id, user_id, room_id, check_in, check_out, guests, total_price 
                         FROM bookroom";

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
        {
             result.Add(new(
                reader.GetInt32("id"),
                reader.GetInt32("user_id"),
                reader.GetInt32("room_id"),
                reader.GetDateTime("check_in"),
                reader.GetDateTime("check_out"),
                reader.GetInt32("guests"),
                reader.GetDecimal("total_price")
                ));
        }
        return Results.Ok(result);
        }
      }
}
//GET bookroom id
public static async Task<IResult> Get(int id, Config config)
    {

        string query = @"SELECT user_id, room_id, check_in, check_out, guests, total_price 
                         FROM bookroom WHERE id = @id";
       var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };
using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
      {
        while (reader.Read())
        {
            result.Add(new(
                reader.GetInt32("user_id"),
                reader.GetInt32("room_id"),
                reader.GetDateTime("check_in"),
                reader.GetDateTime("check_out"),
                reader.GetInt32("guests"),
                reader.GetDecimal("total_price")
            ));
            return Results.NotFound(new { message = $"Booking with id {id} was not found." });
        }
    }
}
//Post 
public record BookingRoomCreate( int UserId,int RoomId,DateTime RoomCheckInDate,DateTime RoomCheckOutDate,int Guests,decimal TotalPrice);
public static async Task<IResult> Post(BookingRoomCreate bookingRoom,Config config,HttpContext ctx)
{
    // Booking a room → normal logged-in user
  var admin_authentication = Authentication.RequireAdmin(ctx);

        // Check
        if (admin_authentication is not null)
        {
            return admin_authentication;
        }

string query = @"INSERT INTO bookroom (user_id, room_id, check_in, check_out, guests, total_price)
                         VALUES (@user_id, @room_id, @check_in, @check_out, @guests, @total_price)";
 var parameters = new MySqlParameter[]
  {
    new("@user_id", bookingRoom.UserId),
     new("@room_id", bookingRoom.RoomId),            
    new("@check_in", bookingRoom.RoomCheckInDate),
    new("@check_out", bookingRoom.RoomCheckOutDate),
    new("@guests", bookingRoom.Guests),
    new("@total_price", bookingRoom.TotalPrice)
  };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

        return Results.Ok(new { message = "Roomcreated booked successfully." });
    }
//Put
public record BookingRoomUpdate(int BookingId,int RoomId,DateTime CheckIn,DateTime CheckOut,int Guests,decimal TotalPrice);
public static async Task<IResult> Put(BookingRoomUpdate bookingRoom,Config config,HttpContext ctx)
{
   var admin_authentication = Authentication.RequireAdmin(ctx);

        // Check
        if (admin_authentication is not null)
        {
            return admin_authentication;
        }

        string query = """
        UPDATE booking_rooms
        SET
            check_in = @check_in,
            check_out = @check_out,
            guests = @guests,
            total_price = @total_price
        WHERE booking_id = @booking_id
          AND room_id = @room_id
    """;
var parameters = new MySqlParameter[]
    {
        new("@booking_id", bookingRoom.BookingId),
        new("@room_id", bookingRoom.RoomId),
        new("@check_in", bookingRoom.CheckIn),
        new("@check_out", bookingRoom.CheckOut),
        new("@guests", bookingRoom.Guests),
        new("@total_price", bookingRoom.TotalPrice)
    };
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
    return Results.Ok(new { message = "Booked room updated." });
    }
