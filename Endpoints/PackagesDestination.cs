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
public class PackagesDestinations
{
  // A usual logic of tabels' classes wn't be able to be used here because this table is only a relation table
  // So A view or a JOIN quiery would be perfect 
  public record GetAll_Data(int PackageId, int DestinationId);

  // GET /package-destinations info 
  public static async Task<IResult> GetAll(Config config)
  {
    List<GetAll_Data> result = new();

    // Inner join to show everything

    string query = """
            SELECT p.name AS package_name, p.type , p.total_price, p.duration_days,p.description, d.name AS destination_name,
            d.climate, c.name AS city_name
            FROM package_destinations AS pd 
            INNER JOIN packages AS p ON pd.package_id = p.id
            INNER JOIN destinations AS d ON pd.destination_id = d.id
            INNER JOIN cities AS c ON d.city_id = c.id;
    
    
        """;

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0),
            reader.GetInt32(1)
        ));
      }
    }

    return Results.Ok(result);
  }

  // DTO för single result (list of destinations for one package)
  public record GetByPackage_Data(int DestinationId);

  
  // GET /package-destinations/{packageId} // Show the distinations wich are linked to a specific package 
  public static async Task<IResult> GetByPackage(int packageId, Config config)
  {
    List<GetByPackage_Data> result = new();

    string query = """
            SELECT p.name AS package_name, p.type , p.total_price, p.duration_days,p.description, d.name AS destination_name,
            d.climate, c.name AS city_name
            FROM package_destinations AS pd 
            INNER JOIN packages AS p ON pd.package_id = p.id
            INNER JOIN destinations AS d ON pd.destination_id = d.id
            INNER JOIN cities AS c ON d.city_id = c.id
            WHERE pd.package_id = @id
            ORDERD BY p.name;
    
    
        """;

    var parameters = new MySqlParameter[]
    {
            new("@pid", packageId)
    };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      while (reader.Read())
      {
        result.Add(new(reader.GetInt32(0)));
      }
    }

    return Results.Ok(result);
  }


  // POST /package-destinations
  // Only admin can add destinations to a package

  public record Post_Args(int PackageId, int DestinationId);

  public static async Task<IResult> Post(Post_Args data, Config config, HttpContext ctx)
  {
    var admin_auth = Authentication.RequireAdmin(ctx);
    if (admin_auth is not null)
      return admin_auth;

    string query = """
            INSERT INTO package_destinations (package_id, destination_id)
            VALUES (@pid, @did)
        """;

    var parameters = new MySqlParameter[]
    {
            new("@pid", data.PackageId),
            new("@did", data.DestinationId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination added to package successfully." });
  }
  // Put method is not needed because a realation could only exist or not exist // a relation can't be modified     

  // DELETE /package-destinations/{packageId}/{destinationId}
  // Only admin can remove destinations from a package
  public static async Task<IResult> Delete(int packageId, int destinationId, Config config, HttpContext ctx)
  {
    var admin_auth = Authentication.RequireAdmin(ctx);
    if (admin_auth is not null)
      return admin_auth;

    string query = """
            DELETE FROM package_destinations
            WHERE package_id = @pid AND destination_id = @did
        """;

    var parameters = new MySqlParameter[]
    {
            new("@pid", packageId),
            new("@did", destinationId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Destination removed from package successfully." });
  }



} //fatima