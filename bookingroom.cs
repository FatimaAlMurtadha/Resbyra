namespace server;
using MySql.Data.MySqlClient;
class BookRoom
{

    //DTO
    public record GetAll_Data(int Id,int UserId,int RoomId,DateTime CheckInDate,DateTime CheckOutDate,int Guests,decimal TotalPrice);
    //One booking
    public record Get_Data(int UserId,int RoomId,DateTime CheckInDate,DateTime CheckOutDate,int Guests, decimal TotalPrice);

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
using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

        if (reader.Read())
        {
            result = new(
            
                reader.GetInt32("user_id"),
                reader.GetInt32("room_id"),
                reader.GetDateTime("check_in"),
                reader.GetDateTime("check_out"),
                reader.GetInt32("guests"),
                reader.GetDecimal("total_price")
            );
            return Results.NotFound(new { message = $"Booking with id {id} was not found." });
        
        }
    }
}
//Post 

