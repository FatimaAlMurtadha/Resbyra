namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http; // for IResult

public class Bookings
{
// Booking list / single booking
public record GetAll_Data(int userId, decimal TotalPrice, DateTime Date, int UserId, int PackageId);
public record Get_Data(decimal TotalPrice, DateTime Date, int UserId, int PackageId);

// Booking creation
public record Post_Args(decimal TotalPrice, DateTime Date, int UserId, int PackageId);

// Booking update
public record Put_Args(int Id, decimal TotalPrice, DateTime Date, int UserId, int PackageId);
}

//GET All
public static async Task<List<GetAll_Data>> GetAll(Config config)
{
    List<GetAll_Data> result = new();
    string query = "SELECT id, total_price, date, user_id, package_id FROM bookings";
    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query);
    while (reader.Read())
    {
        result.Add(new(
            reader.GetInt32(0),
            reader.GetDecimal(1),
            reader.GetDateTime(2),
            reader.GetInt32(3),
            reader.GetInt32(4)
        ));
    }
    return result;
}
//GET Booking ID
public static async Task<IResult> Get(int id, Config config)
{
    Get_Data? result = null;
    string query = "SELECT total_price, date, user_id, package_id FROM bookings WHERE id=@id";
    var parameters = new MySqlParameter[] { new("@id", id) };
    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);
    if (reader.Read())
        result = new(reader.GetDecimal(0), reader.GetDateTime(1), reader.GetInt32(2), reader.GetInt32(3));

    return result is null ? Results.NotFound(new { message = $"Booking {id} not found." }) : Results.Ok(result);
}

//Post Booking
public static async Task Post(Post_Args args, Config config)
{
    string query = "INSERT INTO bookings(total_price, date, user_id, package_id) VALUES(@total_price, @date, @user_id, @package_id)";
    var parameters = new MySqlParameter[]
    {
        new("@total_price", args.TotalPrice),
        new("@date", args.Date),
        new("@user_id", args.UserId),
        new("@package_id", args.PackageId)
    };
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
}
//Put
public static async Task Put(Put_Args args, Config config)
{
    string query = "UPDATE bookings SET total_price=@total_price, date=@date, user_id=@user_id, package_id=@package_id WHERE id=@id";
    var parameters = new MySqlParameter[]
    {
        new("@id", args.Id),
        new("@total_price", args.TotalPrice),
        new("@date", args.Date),
        new("@user_id", args.UserId),
        new("@package_id", args.PackageId)
    };
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
}


