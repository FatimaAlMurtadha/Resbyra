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
{
    public record GetAll_Data(int Id, string Name, string Number, decimal Rating, string Address, string Description, int Destination_Id);

    public static List<GetAll_Data> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = "SELECT id, name, phone_number, rating, address, description, destination FROM Hotels";

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0), //id
                    reader.GetString(1),  // name
                    reader.GetString(2), //Number
                    reader.GetDecimal(3),  //rating
                    reader.GetString(4),  // address
                    reader.GetString(5),  //description
                    reader.GetInt32(6)     //destination id

                ));
            }
        }

        return result;
    }


    
    public record Get_Data(string Name);

    
    public static IResult Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT name FROM Hotels WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                result = new(reader.GetString(0));
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



    public static List<GetAll_Data> Search(string? name, Config config)
    {
      
        if (string.IsNullOrWhiteSpace(name))
        {
            return GetAll(config);
        }

        List<GetAll_Data> result = new();

        string query = """
            SELECT id, name, phone_number, rating, address, description, destination FROM Hotels
            WHERE name LIKE CONCAT('%', @name, '%')
        """;

        var parameters = new MySqlParameter[]
        {
            new("@name", name)
        };

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0), //id
                    reader.GetString(1),  // name
                    reader.GetString(2), //Number
                    reader.GetDecimal(3),  //rating
                    reader.GetString(4),  // address
                    reader.GetString(5),  //description
                    reader.GetInt32(6)     //destination id
                ));
            }
        }

        return result;
    }
}
