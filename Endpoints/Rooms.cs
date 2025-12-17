namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http; 

/*
            id INT AUTO_INCREMENT PRIMARY KEY,'
            room_number INT NOT NULL,
            type VARCHAR(100) NOT NULL,
            price_per_night DECIMAL(10,2) NOT NULL,
            capacity INT NOT NULL,
            hotel_id INT NOT NULL,
            FOREIGN KEY (hotel_id) REFERENCES hotels(id)
            */

class Rooms
{
    public record GetAll_Data(int Id, int RoomNumber, string Type , decimal PricePerNight, int Capacity, int HotelId);

    public static async Task<IResult> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = "SELECT id, room_number, type, price_per_night, capacity, hotel_id FROM rooms";

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32("id"), // id
                    reader.GetInt32("room_number"),  //room number
                    reader.GetString("type"), //type
                    reader.GetDecimal("price_per_night"), //price
                    reader.GetInt32("capacity"),  //capacity
                    reader.GetInt32("hotel_id")  //hotel id

                ));
            }
        }
        return Results.Ok(result);
    }

    public static async Task<IResult> Get(int id, Config config)
    {

        string query = "SELECT room_number, type, price_per_night, capacity, hotel_id FROM rooms WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                var result = new
                {
                    RoomNumber = reader.GetInt32("room_number"),
                    Type = reader.GetString("type"),
                    PricePerNight = reader.GetDecimal("price_per_night"),
                    Capacity = reader.GetInt32("capacity"),
                    HotelId = reader.GetInt32("hotel_id")

                };
                return Results.Ok(result);
                
            }
        }
        return Results.NotFound(new {message = $"Room was not found."});        
    }

    public record RoomCreate(string RoomNumber, string Type, decimal PricePerNight, int Capacity, int HotelId);

    public static async Task<IResult> Post(RoomCreate room, Config config, HttpContext ctx)
    {

        // To post a room "Add" is admin's feature 
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
            INSERT INTO rooms (room_number, type, price_per_night, capacity, hotel_id)
            VALUES (@room_number, @type, @price_per_night, @capacity, @hotel_id)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@room_number", room.RoomNumber), 
            new("@type", room.Type),
            new("@price_per_night", room.PricePerNight),
            new("@capacity", room.Capacity),
            new("@hotel_id", room.HotelId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

        return Results.Ok(new { message = "Room created." });

    }

    public record RoomUpdate(int Id, string RoomNumber, string Type, decimal PricePerNight, int Capacity, int HotelId);

    public static async Task<IResult> Put(RoomUpdate room, Config config, HttpContext ctx)
    {
        // To put a room "Add" is admin's feature 
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
            UPDATE rooms
            SET 
            room_number = @room_number, type = @type, price_per_night = @price_per_night, capacity = @capacity, hotel_id = @hotel_id
            WHERE id = @id
        """;

        var parameters = new MySqlParameter[]
        {
            new("@id", room.Id),
            new("@room_number", room.RoomNumber),
            new("@type", room.Type),
            new("@price_per_night", room.PricePerNight),
            new("@capacity", room.Capacity),
            new("@hotel_id", room.HotelId)
        };
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

        return Results.Ok(new { message = "Room updated." });
    }

    public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
    {
        // To delete a room "Add" is admin's feature 
        // So we need to make it (only Admin) access
        // Throug calling our authentication function or method

        var admin_authentication = Authentication.RequireAdmin(ctx);

        // Check
        if (admin_authentication is not null)
        {
            return admin_authentication;
        }
        // End othorization

        string query = "DELETE FROM rooms WHERE id = @id";


        var parameters = new MySqlParameter[] { new("@id", id) };


        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);


        return Results.Ok(new { message = "Room deleted." });
    }
    public static async Task<IResult> Search(string? sok, Config config)
    {
        List<GetAll_Data> result = new();


        if (string.IsNullOrWhiteSpace(sok))
        {
            return await GetAll(config);
        }


        string query = """
            SELECT id, room_number, type, price_per_night, capacity, hotel_id
            FROM rooms WHERE room_number LIKE CONCAT('%', @sok, '%') OR type LIKE CONCAT('%', @sok, '%')
            ORDER BY room_number ASC
        """;


        var parameters = new MySqlParameter[] { new("@sok", sok) };


        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
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
        }
        return Results.Ok(result);
    }


    public static async Task<IResult> ByHotel(int hotelId, Config config, HttpContext ctx)
    {
        // To search by id a room "Add" is admin's feature 
        // So we need to make it (only Admin) access
        // Throug calling our authentication function or method

        var admin_authentication = Authentication.RequireAdmin(ctx);

        // Check
        if (admin_authentication is not null)
        {
            return admin_authentication;
        }
        // End othorization

        List<GetAll_Data> result = new();

        string query = """
            SELECT id, room_number, type, price_per_night, capacity, hotel_id
            FROM rooms WHERE hotel_id = @hotelId
            ORDER BY room_number ASC
        """;

        var parameters = new MySqlParameter[] { new("@hotelId", hotelId) };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
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
        }
        return Results.Ok(result);
    }

    public static async Task<IResult> ByRoomNumber(int hotelId, string roomNumber, Config config, HttpContext ctx)
    {

        // To search by id a room "Add" is admin's feature 
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
            SELECT id, room_number, type, price_per_night, capacity, hotel_id FROM rooms
            WHERE hotel_id = @hotelId AND room_number = @roomNumber
        """;

        var parameters = new MySqlParameter[]
        {
            new("@hotelId", hotelId),
            new("@roomNumber", roomNumber)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                var result = new
                {
                    Id = reader.GetInt32("id"),
                    RoomNumber = reader.GetInt32("room_number"),
                    Type = reader.GetString("type"),
                    PricePerNight = reader.GetDecimal("price_per_night"),
                    Capacity = reader.GetInt32("capacity"),
                    HotelId = reader.GetInt32("hotel_id")
                };
                return Results.Ok(result);
            }
        }
        return Results.NotFound(new { message = $"Room not found." });
    }
}
