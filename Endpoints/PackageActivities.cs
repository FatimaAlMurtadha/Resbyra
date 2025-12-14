namespace server;

using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;



class PackageActivities
{
  // Link both tables together Packages <-> Activities
  public record Link_Args(int PackageId, int ActivityId);
  // All activities that are in one package
  public static async Task<IResult> ByPackage(int packageId, Config config)
  {
    var result = new List<Activities.GetAll_Data>();
    string query = """
              SELECT a.id, a.name, a.description
              FROM activities AS a
              INNER JOIN package_activities AS pa ON pa.activity_id = a.id
              WHERE pa.package_id = @packageId
              """;
    var parameters = new MySqlParameter[]
    {
      new("@packageId", packageId)
    };
    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);
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

  // The opposite ByActivity
  public static async Task<IResult> ByActivity(int activityId, Config config)
  {
    var result = new List<Packages.GetAll_Data>(); // Packages ?? There is not GettAll_Data method ??? 
    string query = """
                    SELECT p.id, p.name, p.type, p.total_price, p.duration_days, p.description, p.user_id
                    FROM packages AS p
                    INNER JOIN package_activities AS pa ON pa.package_id = p.id
                    WHERE pa.activity_id =@activityId
                  """;
    var parameters = new MySqlParameter[]
    {
      new("@activityId", activityId)

    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);
    {
      while (reader.Read())
      {
        result.Add(new(

        reader.GetInt32("id"),
        reader.GetString("name"),
        reader.GetString("type"),
        reader.GetDecimal("total_price"),
        reader.GetInt32("duration_days"),
        reader.GetString("description"),
        reader.GetInt32("user_id")

        ));
      }


    }
    return Results.Ok(result);


  }

  // Connect activity to oackage "Admin"
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
    {
      return adminAuth;
    }


    string query = """
                     INSERT INTO package_activities (package_id, activity_id)
                     VALUES (@packageId, @activityId)
                   """;


    var parameters = new MySqlParameter[]
    {
      new("@packageId", link.PackageId),
      new("@activityId", link.ActivityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Activity linked." });
  }

  // Delete connection (admin)
  public static async Task<IResult> Unlink(int packageId, int activityId, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
    {
      return adminAuth;
    }
      
    string query = """
                     DELETE FROM package_activities
                     WHERE package_id = @packageId
                       AND activity_id = @activityId
                   """;


    var parameters = new MySqlParameter[]
    {
      new("@destinationId", packageId),
      new("@activityId", activityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Activity unlinked." });
  }


} //fatima
