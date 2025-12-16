namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class PackagesRooms
{
  /*
  CREATE TABLE packages_rooms(
    package_id INT NOT NULL,
    room_id INT NOT NULL,
    PRIMARY KEY (package_id, room_id),
    FOREIGN KEY (package_id) REFERENCES packages(id),
    FOREIGN KEY (room_id) REFERENCES rooms(id)
  );
  */

  // används när man vill koppla package <-> room
  public record Link_Args(int PackageId, int RoomId);

  // alla rooms för ett package
  public static async Task<IResult> ByPackage(int packageId, Config config)
  {
    var result = new List<Rooms.GetAll_Data>();

    string query = """
                   SELECT r.id, r.room_number, r.type, r.price_per_night, r.capacity, r.hotel_id
                   FROM rooms r
                   JOIN packages_rooms pr ON pr.room_id = r.id
                   WHERE pr.package_id = @packageId
                   ORDER BY r.id ASC
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
        reader.GetInt32("room_number"),
        reader.GetString("type"),
        reader.GetDecimal("price_per_night"),
        reader.GetInt32("capacity"),
        reader.GetInt32("hotel_id")
      ));
    }

    return Results.Ok(result);
  }

  // alla packages som har ett visst room
  public record Package_Data(
    int Id,
    string Name,
    string Type,
    decimal TotalPrice,
    int DurationDays,
    string Description,
    int? UserId
  );

  public static async Task<IResult> ByRoom(int roomId, Config config)
  {
    var result = new List<Package_Data>();

    string query = """
                   SELECT p.id, p.name, p.type, p.total_price, p.duration_days, p.description, p.user_id
                   FROM packages p
                   JOIN packages_rooms pr ON pr.package_id = p.id
                   WHERE pr.room_id = @roomId
                   ORDER BY p.id ASC
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@roomId", roomId)
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

  // koppla room till package (admin)
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
                   INSERT INTO packages_rooms (package_id, room_id)
                   VALUES (@packageId, @roomId)
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", link.PackageId),
      new("@roomId", link.RoomId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Room linked." });
  }

  // ta bort koppling (admin)
  public static async Task<IResult> Unlink(
    int packageId,
    int roomId,
    Config config,
    HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
                   DELETE FROM packages_rooms
                   WHERE package_id = @packageId
                     AND room_id = @roomId
                   """;

    var parameters = new MySqlParameter[]
    {
      new("@packageId", packageId),
      new("@roomId", roomId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Room unlinked." });
  }
} 
