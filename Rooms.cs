namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http; 

/*
            id INT AUTO_INCREMENT PRIMARY KEY,'
            number INT NOT NULL,
            type VARCHAR(100) NOT NULL,
            price_per_night DECIMAL(10,2) NOT NULL,
            capacity INT NOT NULL,
            hotel_id INT NOT NULL,
            FOREIGN KEY (hotel_id) REFERENCES hotels(id)
            */

class Rooms
{
    public record GetAll_Data(int Id, int Number, string Type , decimal Price_Per_Night, int Capacity, int Hotel_Id);

    public static List<GetAll_Data> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = "SELECT id, number, type, price_per_night, capacity, hotel_id FROM Rooms";

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0), // id
                    reader.GetInt32(1),  //number
                    reader.GetString(2), //type
                    reader.GetDecimal(3), //price
                    reader.GetInt32(4),  //capacity
                    reader.GetInt32(5)  //hotel id

                ));
            }
        }

        return result;
    }
    
    public record Get_Data(int Number);

    
    public static IResult Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT number FROM Rooms WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                result = new(reader.GetInt32(0));
            }
        }

        if (result is null)
        {
            
            return Results.NotFound(new
            {
                message = $"Room with id {id} was not found."
            });
        }

        
        return Results.Ok(result);
    }



    public static List<GetAll_Data> Search(int? number, Config config)
    {
      
        if (number == null)
        {
            return GetAll(config);
        }

        List<GetAll_Data> result = new();

        string query = """
            SELECT id, number, type, price_per_night, capacity, hotel_id FROM Rooms
            WHERE name LIKE CONCAT('%', @name, '%')
        """;

        var parameters = new MySqlParameter[]
        {
            new("@number", number)
        };

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0), // id
                    reader.GetInt32(1),  //number
                    reader.GetString(2), //type
                    reader.GetDecimal(3), //price
                    reader.GetInt32(4),  //capacity
                    reader.GetInt32(5)  //hotel id
                ));
            }
        }

        return result;
    }
}
