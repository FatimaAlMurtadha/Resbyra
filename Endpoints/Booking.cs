namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class Bookings
{
    // DTOs
    public record GetAll_Data(int Id, decimal TotalPrice, DateTime Date, int UserId, int PackageId);
    public record Get_Data(decimal TotalPrice, DateTime Date, int UserId, int PackageId);
    public record Post_Args(decimal TotalPrice, DateTime Date, int UserId, int PackageId);
    public record Put_Args(int Id, decimal TotalPrice, DateTime Date, 
    int UserId, int PackageId);

    // GET ALL
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

    // GET BY ID
    public static async Task<IResult> Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT total_price, date, user_id, package_id FROM bookings WHERE id = @id";
        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);
        if (reader.Read())
        {
            result = new(
                reader.GetDecimal(0),
                reader.GetDateTime(1),
                reader.GetInt32(2),
                reader.GetInt32(3)
            );
        }

        return result is null
            ? Results.NotFound(new { message = $"Booking with id {id} not found." })
            : Results.Ok(result);
    }

    // POST
    public static async Task Post(Post_Args args, Config config)
    {
        string query = """
            INSERT INTO bookings(total_price, date, user_id, package_id)
            VALUES(@total_price, @date, @user_id, @package_id)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@total_price", args.TotalPrice),
            new("@date", args.Date),
            new("@user_id", args.UserId),
            new("@package_id", args.PackageId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
    }

    // PUT
    public static async Task Put(int id, Put_Args args, Config config)
    {
        string query = """
            UPDATE bookings 
            SET total_price = @total_price, date = @date, user_id = @user_id, package_id = @package_id
            WHERE id = @id
        """;

        var parameters = new MySqlParameter[]
        {
            new("@id", id),
            new("@total_price", args.TotalPrice),
            new("@date", args.Date),
            new("@user_id", args.UserId),
            new("@package_id", args.PackageId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
    }

    // DELETE
    public static async Task Delete(int id, Config config)
    {
        string query = "DELETE FROM bookings WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
    }
    
    //dropping the search function here, arbaz if it is confusing or you need help intigrating it into your file please let me know :) /Oskar
    
    public static async Task<IResult> Search(string? term, Config config)
    {
        List<GetAll_Data> result = new();

        string query;
        MySqlParameter[]? parameters = null;
        
        if (string.IsNullOrWhiteSpace(term))
        {
            query = "SELECT id, total_price, date, user_id, package_id FROM bookings";
        }
        else
        {
            query = """
                        SELECT id, total_price, date, user_id, package_id
                        FROM bookings
                        WHERE CAST(total_price AS CHAR) LIKE @term
                           OR CAST(date AS CHAR) LIKE @term
                           OR CAST(user_id AS CHAR) LIKE @term
                           OR CAST(package_id AS CHAR) LIKE @term
                    """;

            parameters = new MySqlParameter[]
            {
                new("@term", "%" + term + "%")
            };
        }

        if (parameters is null)
        {
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
        }
        else
        {
            using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);
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
        }

        return Results.Ok(result);
    }
    
    
}
