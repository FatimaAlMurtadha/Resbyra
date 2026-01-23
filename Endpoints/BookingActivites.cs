namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class BookActivities
{
    // DTO for GET
    public record GetAll_Data(
        int Id,
        int UserId,
        int ActivityId,
        int BookingId,
        DateTime BookedDate,
        TimeSpan BookedTime,
        decimal TotalPrice
    );

    // DTO for POST / PUT
    public record BookActivityInput(
        int ActivityId,
        int BookingId,
        DateTime BookedDate,
        TimeSpan BookedTime,
        decimal TotalPrice
    );

    // GET ALL
    public static async Task<IResult> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = """
            SELECT id, user_id, activity_id, booking_id,
                   booked_date, booked_time, total_price
            FROM booked_activities
        """;

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query);
        while (reader.Read())
        {
            result.Add(new(
                reader.GetInt32("id"),
                reader.GetInt32("user_id"),
                reader.GetInt32("activity_id"),
                reader.GetInt32("booking_id"),
                reader.GetDateTime("booked_date"),
                reader.GetTimeSpan("booked_time"),
                reader.GetDecimal("total_price")
            ));
        }

        return Results.Ok(result);
    }

    // GET BY ID
    public static async Task<IResult> Get(int id, Config config)
    {
        string query = """
            SELECT id, user_id, activity_id, booking_id,
                   booked_date, booked_time, total_price
            FROM booked_activities
            WHERE id = @id
        """;

        using var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            query,
            new MySqlParameter("@id", id)
        );

        if (!reader.Read())
            return Results.NotFound();

        return Results.Ok(new GetAll_Data(
            reader.GetInt32("id"),
            reader.GetInt32("user_id"),
            reader.GetInt32("activity_id"),
            reader.GetInt32("booking_id"),
            reader.GetDateTime("booked_date"),
            reader.GetTimeSpan("booked_time"),
            reader.GetDecimal("total_price")
        ));
    }

    // POST
    public static async Task<IResult> Post(
        BookActivityInput input,
        Config config,
        HttpContext ctx
    )
    {
        var auth = Authentication.RequireUser(ctx);
        if (auth is not null) return auth;

        int userId = Authentication.GetUserId(ctx)!.Value;

        string query = """
            INSERT INTO booked_activities
            (user_id, activity_id, booking_id, booked_date, booked_time, total_price)
            VALUES (@user_id, @activity_id, @booking_id, @booked_date, @booked_time, @total_price)
        """;

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            query,
            new MySqlParameter("@user_id", userId),
            new MySqlParameter("@activity_id", input.ActivityId),
            new MySqlParameter("@booking_id", input.BookingId),
            new MySqlParameter("@booked_date", input.BookedDate),
            new MySqlParameter("@booked_time", input.BookedTime),
            new MySqlParameter("@total_price", input.TotalPrice)
        );

        return Results.Ok(new { message = "Activity booked" });
    }

    // PUT (ADMIN)
    public static async Task<IResult> Put(
        int id,
        BookActivityInput input,
        Config config,
        HttpContext ctx
    )
    {
        var admin = Authentication.RequireAdmin(ctx);
        if (admin is not null) return admin;

        string query = """
            UPDATE booked_activities
            SET activity_id=@activity_id,
                booking_id=@booking_id,
                booked_date=@booked_date,
                booked_time=@booked_time,
                total_price=@total_price
            WHERE id=@id
        """;

        int rows = await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            query,
            new MySqlParameter("@id", id),
            new MySqlParameter("@activity_id", input.ActivityId),
            new MySqlParameter("@booking_id", input.BookingId),
            new MySqlParameter("@booked_date", input.BookedDate),
            new MySqlParameter("@booked_time", input.BookedTime),
            new MySqlParameter("@total_price", input.TotalPrice)
        );

        return rows == 0 ? Results.NotFound() : Results.Ok();
    }

    // DELETE (ADMIN)
    public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
    {
        var admin = Authentication.RequireAdmin(ctx);
        if (admin is not null) return admin;

        int rows = await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            "DELETE FROM booked_activities WHERE id=@id",
            new MySqlParameter("@id", id)
        );

        return rows == 0 ? Results.NotFound() : Results.Ok();
    }
}
