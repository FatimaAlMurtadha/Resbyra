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


