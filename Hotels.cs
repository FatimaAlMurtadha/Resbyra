namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            phone_number VARCHAR(50),
            rating DECIMAL(3,1) NOT NULL,
            address VARCHAR(255) NOT NULL,
            description TEXT NOT NULL,
            destination_id INT NOT NULL,
            FOREIGN KEY (destination_id) REFERENCES destinations(id)
            */

class Hotels
{                                       //för alla hotel
    public record GetAll_Data(int Id, string Name, string PhoneNumber, decimal Rating, string Address, string Description, int DestinationId);

    public static async Task<IResult> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = "SELECT id, name, phone_number, rating, address, description, destination_id FROM Hotels";

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32("id"), //id
                    reader.GetString("name"),  // name
                    reader.GetString("phone_number"), //Number
                    reader.GetDecimal("rating"),  //rating
                    reader.GetString("address"),  // address
                    reader.GetString("description"),  //description
                    reader.GetInt32("destination_id")     //destination id

                ));
            }
        }

        return Results.Ok(result);
    }

                                            // för ett specefik hotel
    public record Get_Data(string Name, string PhoneNumber, decimal Rating, string Address, string Description, int Destination_Id);

    public static async Task<IResult> Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT name, phone_number, address, description, destination_id FROM Hotels WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader =await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                result = new(
                    reader.GetString("name"),
                    reader.GetString("phone_number"),
                    reader.GetDecimal("rating"),
                    reader.GetString("Address"),
                    reader.GetString("description"),
                    reader.GetInt32("destination_id")
                    );
            }
        }

        if (result is null)
        {
            
            return Results.NotFound(new
            {
                message = $"Hotel with id {id} was not found."
            });
        }

        
        return Results.Ok(result);
    }

                                        //Post nytt hotel
    public record CreateNew(string Name, string PhoneNumber, decimal Rating, string Address, string Description, int DestinationId);

    public static async Task<IResult> Post(CreateNew hotel, Config config)
        {
            string query = """
                INSERT INTO hotels (name, phone_number, rating, address, description, destination_id)
                VALUES (@name, @phone_number, @rating, @address, @description, @destination_id)
            """;

            var parameters = new MySqlParameter[]
            {
                new("@name", hotel.Name),
                new("@phone_number", hotel.PhoneNumber),
                new("@rating", hotel.Rating),
                new("@address", hotel.Address),
                new("@description", hotel.Description),
                new("@destination_id", hotel.DestinationId)
            };
            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

            return Results.Ok(new {message ="Hotel created."});
        }

    public record UpdateH(int Id, string Name, string PhoneNumber, decimal Rating, string Address, string Description, int DestinationId);

    public static async Task<IResult> Put(UpdateH hotel, Config config)
    {
        string query = """
            UPDATE hotels
                SET name = @name, phone_number = @phone_number, rating= @rating, address = @address, description = @description, destination_id = @destination_id 
                WHERE id = @id

        """;
        var parameters = new MySqlParameter[]
        {
            new("@id", hotel.Id),
            new("@name", hotel.Name),
            new("@phone_number", hotel.PhoneNumber),
            new("@rating", hotel.Rating),
            new("@address", hotel.Address),
            new("@description", hotel.Description),
            new("@destination_id", hotel.DestinationId)    
        };
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

        return Results.Ok(new {message = "Hotel Uppdated."});
    }

    public static async Task<IResult> Delete(int id, Config config)
    {
        string query = "DELETE FROM hotels WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok(new {message= "Hotel deleted."});
    }

    public static async Task<IResult> Search(string? sok, Config config)
    {
        List<GetAll_Data> result = new();

        if (string.IsNullOrWhiteSpace(sok))
        {
            return await GetAll(config);
        }

        string query ="""
            SELECT id, name, phone_number, rating, address, description, destination_id
            FROM hotels WHERE name LIKE CONCAT ('%', @sok, '%')
            OR description LIKE CONCAT ('%', @sok,'%')
            ORDER BY rating DESC, name ASC
        """;

        var parameters = new MySqlParameter[]
        {
            new("@sok", sok)
        };
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while(reader.Read())
            {
                result.Add(new(
                    reader.GetInt32("id"), 
                    reader.GetString("name"),  
                    reader.GetString("phone_number"), 
                    reader.GetDecimal("rating"),  
                    reader.GetString("address"), 
                    reader.GetString("description"), 
                    reader.GetInt32("destination_id")     
                ));
            }
        }
        return Results.Ok(result);
    }

    public static async Task<IResult> ByDestination(int destinationId, Config config)
    {
        List<GetAll_Data> result = new();

        string query ="""
            SELECT id, name, phone_number, rating, address, description, destination_id FROM hotels
            WHERE destination_id = @destinationId
            ORDEER BY reting DESC, name ASC
        """;

        var parameters = new MySqlParameter[]
        {
            new("@destinationId", destinationId)
        };
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {    
                result.Add(new(
                    reader.GetInt32("id"),
                    reader.GetString("name"),  
                    reader.GetString("phone_number"), 
                    reader.GetDecimal("rating"),  
                    reader.GetString("address"), 
                    reader.GetString("description"), 
                    reader.GetInt32("destination_id")

                ));
            }
        }
        return Results.Ok(result);
    }
}
