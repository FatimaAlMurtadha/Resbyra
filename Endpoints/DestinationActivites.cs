namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class DestinationActivites
{
  // används när man vill koppla aktivitet <-> destination
  public record Link_Args(int DestinationId, int ActivityId);

  // alla aktiviteter för en destination
  public static async Task<IResult> ByDestination(int destinationId, Config config)
  {
    var result = new List<Activities.GetAll_Data>();

    string query = """
      SELECT a.id, a.name, a.description
      FROM activities a
      JOIN destination_activities da ON da.activity_id = a.id
      WHERE da.destination_id = @destinationId
    """;

    var parameters = new MySqlParameter[]
    {
      new("@destinationId", destinationId)
    };

    using var reader =
      await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
        reader.GetInt32("id"),
        reader.GetString("name"),
        reader.GetString("description")
      ));
    }

    return Results.Ok(result);
  }

  // alla destinations som har en viss aktivitet
  public static async Task<IResult> ByActivity(int activityId, Config config)
  {
    var result = new List<Destinations.GetAll_Data>();

    string query = """
      SELECT d.id, d.description, d.climate, d.average_cost, d.city_id
      FROM destinations d
      JOIN destination_activities da ON da.destination_id = d.id
      WHERE da.activity_id = @activityId
    """;

    var parameters = new MySqlParameter[]
    {
      new("@activityId", activityId)
    };

    using var reader =
      await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
        reader.GetInt32("id"),
        reader.GetString("description"),
        reader.GetString("climate"),
        reader.GetDecimal("average_cost"),
        reader.GetInt32("city_id")
      ));
    }

    return Results.Ok(result);
  }

  // koppla aktivitet till destination (admin)
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
      INSERT INTO destination_activities (destination_id, activity_id)
      VALUES (@destinationId, @activityId)
    """;

    var parameters = new MySqlParameter[]
    {
      new("@destinationId", link.DestinationId),
      new("@activityId", link.ActivityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Activity linked." });
  }

  // ta bort koppling (admin)
  public static async Task<IResult> Unlink(
    int destinationId,
    int activityId,
    Config config,
    HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
      DELETE FROM destination_activities
      WHERE destination_id = @destinationId
        AND activity_id = @activityId
    """;

    var parameters = new MySqlParameter[]
    {
      new("@destinationId", destinationId),
      new("@activityId", activityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Activity unlinked." });
  }
}
