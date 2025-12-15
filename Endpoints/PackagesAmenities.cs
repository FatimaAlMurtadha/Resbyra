namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
CREATE TABLE packages_amenities(
  package_id INT NOT NULL,
  amenity_id INT NOT NULL,
  PRIMARY KEY (package_id, amenity_id),
  FOREIGN KEY (package_id) REFERENCES packages(id),
  FOREIGN KEY (amenity_id) REFERENCES amenities(id)
);
*/
class PackagesAmenities
{
  // används när man vill koppla package <-> amenity
  public record Link_Args(int PackageId, int AmenityId);

  // alla amenities för ett package
  public static async Task<IResult> ByPackage(int packageId, Config config)
  {
    var result = new List<Amenities.GetAll_Data>();

    string query = """
                   SELECT a.id, a.amenity_name
                   FROM amenities a
                   JOIN packages_amenities pa ON pa.amenity_id = a.id
                   WHERE pa.package_id = @packageId
                   ORDER BY a.amenity_name ASC
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", packageId)
    };

    using var reader =
      await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
        reader.GetInt32("id"),
        reader.GetString("amenity_name")
      ));
    }

    return Results.Ok(result);
  }

  // alla packages som har en viss amenity
  public record Package_Data(
    int Id,
    string Name,
    string Type,
    decimal TotalPrice,
    int DurationDays,
    string Description,
    int? UserId
  );

  public static async Task<IResult> ByAmenity(int amenityId, Config config)
  {
    var result = new List<Package_Data>();

    string query = """
                   SELECT p.id, p.name, p.type, p.total_price, p.duration_days, p.description, p.user_id
                   FROM packages p
                   JOIN packages_amenities pa ON pa.package_id = p.id
                   WHERE pa.amenity_id = @amenityId
                   ORDER BY p.id ASC
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@amenityId", amenityId)
    };

    using var reader =
      await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      int? userId = reader.IsDBNull(reader.GetOrdinal("user_id"))
        ? null
        : reader.GetInt32(reader.GetOrdinal("user_id"));

      result.Add(new(
        reader.GetInt32("id"),
        reader.GetString("name"),
        reader.GetString("type"),
        reader.GetDecimal("total_price"),
        reader.GetInt32("duration_days"),
        reader.GetString("description"),
        userId
      ));
    }

    return Results.Ok(result);
  }

  // koppla amenity till package (admin)
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
                   INSERT INTO packages_amenities (package_id, amenity_id)
                   VALUES (@packageId, @amenityId)
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", link.PackageId),
      new("@amenityId", link.AmenityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity linked." });
  }

  // ta bort koppling (admin)
  public static async Task<IResult> Unlink(
    int packageId,
    int amenityId,
    Config config,
    HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
                   DELETE FROM packages_amenities
                   WHERE package_id = @packageId
                     AND amenity_id = @amenityId
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", packageId),
      new("@amenityId", amenityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity unlinked." });
  }
}
