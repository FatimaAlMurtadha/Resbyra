namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
// The relation between the packages and the destinations M:N
  string package_destinations_table = """
        CREATE TABLE package_destinations (
            package_id INT NOT NULL,
            destination_id INT NOT NULL,
            PRIMARY KEY (package_id, destination_id),
            FOREIGN KEY (package_id) REFERENCES packages(id),
            FOREIGN KEY (destination_id) REFERENCES destinations(id)
        );

*/
class PackagesDestinations
{
  // A usual logic of tabels' classes wn't be able to be used here because this table is only a relation table
  // So A view or a JOIN quiery would be perfect 
  public record GetAll_Data(int PackageId, string PackageName, string Packagetype, decimal TotalPrice, int DurationDays,
  string PackageDescription, int DestinationId, string DestinationName, string Climate, string CityName );

  // GET /package-destinations info 
  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    // View to show everything

    string query = "SELECT * FROM view_package_destinations";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0), // package_id
            reader.GetString(1), // package_name
            reader.GetString(2), // package_type
            reader.GetDecimal(3), // total_price
            reader.GetInt32(4),   // duration_days
            reader.GetString(5),  // package_description
            reader.GetInt32(6),   // destination_id
            reader.GetString(7),  // destination_name
            reader.GetString(8),  // climate
            reader.GetString(9)   // city_name
        ));
      }
    }

    return Results.Ok(result);
  }

  // DTO för single result (list of destinations for one package)
  public record GetByPackage_Data(int DestinationId, string DestinationName, string Climate, string CityName);

  // GET /package-destinations/{packageId} // Show the distinations wich are linked to a specific package 
  public static async Task<IResult> GetByPackage(int packageId, Config config)
  {
    List<GetByPackage_Data> result = new();

    string query = """
            SELECT d.id AS destination_id, d.name AS destination_name, d.climate, c.name AS city_name
            FROM package_destinations AS pd
            INNER JOIN destinations AS d ON pd.destination_id = d.id
            INNER JOIN cities AS c ON d.city_id = c.id
            WHERE pd.package_id = @id
            ORDER BY d.name
        """;

    var parameters = new MySqlParameter[]
    {
         new("@id", packageId)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0), // destination_id
            reader.GetString(1), // destination_name
            reader.GetString(2), // climate
            reader.GetString(3) // city_name
        ));
      }
    }

    return Results.Ok(result);
  }

  // record 
  public record GetByDestination_Data(int PackageId, string PackageName, string Type, decimal TotalPrice, int DurationDays, string PackageDescription);

  public static async Task<IResult> GetByDestination(int destinationId, Config config)
  {
    List<GetByDestination_Data> result = new();

    string query = """
            SELECT p.id AS package_id, p.name AS package_name, p.type , p.total_price, p.duration_days, p.description
            FROM package_destinations AS pd 
            INNER JOIN packages AS p ON pd.package_id = p.id
            WHERE pd.destination_id = @id
            ORDER BY p.name;
        """;

    var parameters = new MySqlParameter[]
    {
            new("@id", destinationId)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0), // package_id
            reader.GetString(1), // package_name
            reader.GetString(2), // type
            reader.GetDecimal(3), // total_price
            reader.GetInt32(4), // duration_days
            reader.GetString(5) // description
        ));
      }
    }

    return Results.Ok(result);
  }



  // POST /package-destinations
  // Only admin can add destinations to a package

  public record Post_Args(int PackageId, int DestinationId);

  public static async Task<IResult> Post(Post_Args package_destination, Config config, HttpContext ctx)
  {
    // To post "Add" is admin's feature 
    // So we need to make it (only Admin) access
    // Throug calling our authentication function or method

    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Check 
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    // End othorization

    string query = """
            INSERT INTO package_destinations (package_id, destination_id)
            VALUES (@package_id, @destination_id)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@package_id", package_destination.PackageId),
            new("@destination_id", package_destination.DestinationId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination added to package successfully." });
  }
  // Put method is not needed because a realation could only exist or not exist // a relation can't be modified     

  // DELETE /package-destinations/{packageId}/{destinationId}
  // Only admin can remove destinations from a package
  public static async Task<IResult> Delete(int packageId, int destinationId, Config config, HttpContext ctx)
  {
    // To delete is an admin's feature 
    // So we need to make it (only Admin) access
    // Throug calling our authentication function or method

    var admin_authentication = Authentication.RequireAdmin(ctx);

    // Check
    if (admin_authentication is not null)
    {
      return admin_authentication;
    }
    // End of authorization

    string query = """
            DELETE FROM package_destinations
            WHERE package_id = @package_id AND destination_id = @destination_id
        """;

    var parameters = new MySqlParameter[]
    {
            new("@package_id", packageId),
            new("@destination_id", destinationId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination removed from package successfully." });
  }



} //fatima