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


    // there are errors on the code Post + put + Delete need the following
    // 1-> a parameter "booking" instead of "args" because it's a assigned for the languege
    // 2-> a return value at the end of each method 
    // t.e 
    // return Results.Ok(new { message = "booking created successfully." });
    // 3 -> an Interface <IResult> after Task --> Fatima
    public static async Task<IResult> Post(Post_Args booking, Config config, HttpContext ctx)
    {
        
        // To post a booking "Add" is users's feature 
        // So we need to make it (only inlogged user) access
        // Throug calling our authentication function or method

        var user_authentication = Authentication.RequireUser(ctx);

        // Chech 
        if (user_authentication is not null)
        {
            return user_authentication;
        }

        // check the role from the session
        // bring user_id from the session // it can't be null
        int current_user_id = Authentication.GetUserId(ctx)!.Value;
        // End othorization
        



        string query = """
            INSERT INTO bookings(total_price, date, user_id, package_id)
            VALUES(@total_price, @date, @user_id, @package_id)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@total_price", booking.TotalPrice),
            new("@date", booking.Date),
            new("@user_id", booking.UserId),
            new("@package_id", booking.PackageId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok(new { message = "booking created successfully." });
    }

    // PUT
    public static async Task<IResult> Put(int id, Put_Args booking, Config config, HttpContext ctx)
    {
        
        // To put a booking "Add" is users's feature 
        // So we need to make it (only inlogged user) access
        // Throug calling our authentication function or method

        var user_authentication = Authentication.RequireUser(ctx);

        // Check 
        if (user_authentication is not null)
        {
            return user_authentication;
        }

        // check the role from the session
        // bring user_id from the session // it can't be null
        int current_user_id = Authentication.GetUserId(ctx)!.Value;
        // End othorization
        // this variable takes the role depending on the session inlogged User Role
        string role = Authentication.GetRole(ctx)!;

        // we need also to bring and link the booking withe the owner of the session "booking's owner"
        string query_search = "SELECT user_id FROM bookings WHERE id = @id";
        // a variable to save the booking owner with a null value that we reassign on reader
        int? booking_owner_id = -1;

        // need to read information of the booking owner from database
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query_search, new MySqlParameter("@id", id)))
        {
            if (reader.Read())
            {
                booking_owner_id = reader.GetInt32(0);

            }
            else
            {
                return Results.NotFound();
            }
        }

        // Securty
        if (role != "admin" && booking_owner_id != current_user_id)
        {
            return Results.Forbid();
        }
        // End // Fatima
        

        string query = """
            UPDATE bookings 
            SET total_price = @total_price, date = @date, user_id = @user_id, package_id = @package_id
            WHERE id = @id
        """;

        var parameters = new MySqlParameter[]
        {
            new("@id", id),
            new("@total_price", booking.TotalPrice),
            new("@date", booking.Date),
            new("@user_id", booking.UserId),
            new("@package_id", booking.PackageId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok(new { message = "booking updated successfully." });
    }

    // DELETE
    public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
    {
        
        // To delete a booking "Add" is users's feature 
        // So we need to make it (only inlogged user) access
        // Throug calling our authentication function or method

        var user_authentication = Authentication.RequireUser(ctx);

        // Chech 
        if (user_authentication is not null)
        {
            return user_authentication;
        }

        // check the role from the session
        // bring user_id from the session // it can't be null
        int current_user_id = Authentication.GetUserId(ctx)!.Value;
        // End othorization

        // this variable takes the role depending on the session inlogged User Role
        string role = Authentication.GetRole(ctx)!;

        // we need also to bring and link the booking withe the owner of the session "booking's owner"
        string query_search = "SELECT user_id FROM bookings WHERE id = @id";
        // a variable to save the booking owner with a null value that we reassign on reader
        int? booking_owner_id = -1;

        // need to read information of the booking owner from database
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query_search, new MySqlParameter("@id", id)))
        {
            if (reader.Read())
            {
                booking_owner_id = reader.GetInt32(0);

            }
            else
            {
                return Results.NotFound();
            }
        }

        // Securty
        if (role != "admin" && booking_owner_id != current_user_id)
        {
            return Results.Forbid();
        }
        // End // Fatima

        

        string query = "DELETE FROM bookings WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok(new { message = "booking deleted successfully." });
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
