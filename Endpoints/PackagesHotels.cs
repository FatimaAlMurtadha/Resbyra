namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class PackagesHotels
{
  /*
  CREATE TABLE packages_hotels(
    package_id INT NOT NULL,
    hotel_id INT NOT NULL,
    PRIMARY KEY (package_id, hotel_id),
    FOREIGN KEY (package_id) REFERENCES packages(id),
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
  );
  */

  // används när man vill koppla package <-> hotel
  public record Link_Args(int PackageId, int HotelId);

  // alla hotell får ett package
  public static async Task<IResult> ByPackage(int packageId, Config config)
  {
    var result = new List<Hotels.GetAll_Data>();

    string query = """
                   SELECT h.id, h.name, h.phone_number, h.rating, h.address, h.description, h.destination_id
                   FROM hotels h
                   JOIN packages_hotels ph ON ph.hotel_id = h.id
                   WHERE ph.package_id = @packageId
                   ORDER BY h.id ASC
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
        reader.GetString("name"),
        reader.IsDBNull(reader.GetOrdinal("phone_number")) ? null : reader.GetString("phone_number"),
        reader.GetDecimal("rating"),
        reader.GetString("address"),
        reader.GetString("description"),
        reader.GetInt32("destination_id")
      ));
    }

    return Results.Ok(result);
  }

  // alla packages som har ett visst hotell
  public record Package_Data(
    int Id,
    string Name,
    string Type,
    decimal TotalPrice,
    int DurationDays,
    string Description,
    int? UserId
  );

  public static async Task<IResult> ByHotel(int hotelId, Config config)
  {
    var result = new List<Package_Data>();

    string query = """
                   SELECT p.id, p.name, p.type, p.total_price, p.duration_days, p.description, p.user_id
                   FROM packages p
                   JOIN packages_hotels ph ON ph.package_id = p.id
                   WHERE ph.hotel_id = @hotelId
                   ORDER BY p.id ASC
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@hotelId", hotelId)
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

  // koppla hotel till package (admin)
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
                   INSERT INTO packages_hotels (package_id, hotel_id)
                   VALUES (@packageId, @hotelId)
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", link.PackageId),
      new("@hotelId", link.HotelId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Hotel linked." });
  }

  // ta bort koppling (admin)
  public static async Task<IResult> Unlink(
    int packageId,
    int hotelId,
    Config config,
    HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
                   DELETE FROM packages_hotels
                   WHERE package_id = @packageId
                     AND hotel_id = @hotelId
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", packageId),
      new("@hotelId", hotelId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Hotel unlinked." });
  }
} 
