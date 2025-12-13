namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class Travelers
{
 public record GetAll_Data(int Id,string FirstName,string LastName,string PassportNumber,int Age,int BookingId,int UserId);
 //GET All
 public static async Task<IResult> GetAll(Config config)
{
        List<GetAll_Data> result = new();
string query = """
            SELECT id, first_name, last_name, passport_number, age, booking_id, user_id
            FROM travelers
        """;
using var reader =
await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query);
 while (reader.Read())
       {
            result.Add(new(
                reader.GetInt32("id"),
                reader.GetString("first_name"),
                reader.GetString("last_name"),
                reader.GetString("passport_number"),
                reader.GetInt32("age"),
                reader.GetInt32("booking_id"),
                 reader.GetInt32("user_id")
            ));
        }
        return Results.Ok(result);
    }
//Get BY ID
public static async Task<IResult> Get(int id, Config config)
    {
        string query = """
            SELECT first_name, last_name, passport_number, age, booking_id
            FROM travelers
            WHERE id = @id
        """;
       var parameters = new[]
        {
            new MySqlParameter("@id", id)
        };

        using var reader =
            await MySqlHelper.ExecuteReaderAsync(
                config.ConnectionString, query, parameters);
if (reader.Read())
        {
            var result = new
            {
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                PassportNumber = reader.GetString("passport_number"),
                Age = reader.GetInt32("age"),
                BookingId = reader.GetInt32("booking_id"),
                UserId = reader.GetInt32("user_id")
            };
            return Results.Ok(result);
        }
        return Results.NotFound(new { message = "Traveler not found." });
    }
    //Create
    public record TravelerCreate(string FirstName,string LastName,string PassportNumber,int Age,int BookingId,int UserId);
     public static async Task<IResult> Post(TravelerCreate traveler, Config config,HttpContext ctx)
     {
        // Logged-in user required
        var admin_authentication = Authentication.RequireAdmin(ctx);
        // Check
        if (admin_authentication is not null)
        {
            return admin_authentication;
        }
        string query = """
            INSERT INTO travelers
            (first_name, last_name, passport_number, age, booking_id, user_id)
            VALUES
            (@first_name, @last_name, @passport_number, @age, @booking_id, @user_id)
            """;
        var parameters = new MySqlParameter[]
       {
            new ("@first_name", traveler.FirstName),
            new ("@last_name", traveler.LastName),
            new ("@passport_number", traveler.PassportNumber),
            new ("@age", traveler.Age),
            new ("@booking_id", traveler.BookingId),
            new ("@user_id", traveler.UserId)
        };
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

        return Results.Ok(new { message = "Traveler created." });
    }
//Update


