namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class AmenitiesHotel
{
  // POST body for linkar anemetiy till hotell
  public record Link_Args(int HotelId, int AmenityId);

  // GET /hotels/{hotelId}/amenities
  // Returns alla amenities for a specific hotelll
  public static async Task<IResult> ByHotel(int hotelId, Config config)
  {
    var result = new List<Amenities.GetAll_Data>();

    string query = """
      SELECT a.id, a.amenity_name
      FROM amenities a
      JOIN amenities_hotels ah ON ah.amenity_id = a.id
      WHERE ah.hotel_id = @hotelId
      ORDER BY a.amenity_name ASC
    """;

    var parameters = new MySqlParameter[]
    {
      new("@hotelId", hotelId)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new Amenities.GetAll_Data(
        reader.GetInt32("id"),
        reader.GetString("amenity_name")
      ));
    }

    return Results.Ok(result);
  }

  // GET /amenities/{amenityId}/hotels
  public static async Task<IResult> ByAmenity(int amenityId, Config config)
  {
    var result = new List<Hotels.GetAll_Data>();

    string query = """
      SELECT h.id, h.name, h.phone_number, h.rating, h.address, h.description, h.destination_id
      FROM hotels h
      JOIN amenities_hotels ah ON ah.hotel_id = h.id
      WHERE ah.amenity_id = @amenityId
      ORDER BY h.rating DESC, h.name ASC
    """;

    var parameters = new MySqlParameter[]
    {
      new("@amenityId", amenityId)
    };

    using var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new Hotels.GetAll_Data(
        reader.GetInt32("id"),
        reader.GetString("name"),
        reader.GetString("phone_number"),
        reader.GetDecimal("rating"),
        reader.GetString("address"),
        reader.GetString("description"),
        reader.GetInt32("destination_id")
      ));
    }

    return Results.Ok(result);
  }

  // POST /hotels/amenities/link
  // Links a hotell till anemety (endast admin) fatima rätta till om det är fel mer users koden du byggt :) /oskar
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    // optional men jag vill hindra dupliceringar
    string query = """
      INSERT INTO amenities_hotels (hotel_id, amenity_id)
      SELECT @hotelId, @amenityId
      WHERE NOT EXISTS (
        SELECT 1 FROM amenities_hotels
        WHERE hotel_id = @hotelId AND amenity_id = @amenityId
      )
    """;

    var parameters = new MySqlParameter[]
    {
      new("@hotelId", link.HotelId),
      new("@amenityId", link.AmenityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity linked to hotel." });
  }

  // DELETE /hotels/{hotelId}/amenities/{amenityId}
  // Unlink a hotel from amenity 
  public static async Task<IResult> Unlink(int hotelId, int amenityId, Config config, HttpContext ctx)
  {
    var adminAuth = Authentication.RequireAdmin(ctx);
    if (adminAuth is not null)
      return adminAuth;

    string query = """
      DELETE FROM amenities_hotels
      WHERE hotel_id = @hotelId AND amenity_id = @amenityId
    """;

    var parameters = new MySqlParameter[]
    {
      new("@hotelId", hotelId),
      new("@amenityId", amenityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Amenity unlinked from hotel." });
  }
}
