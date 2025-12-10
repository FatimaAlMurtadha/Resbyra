using MySql.Data.MySqlClient;

namespace server;

 /*
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            type ENUM('ready','custom') NOT NULL DEFAULT 'ready',
            total_price DECIMAL(10,2) NOT NULL,
            duration_days INT NOT NULL,
            description TEXT NOT NULL,
            user_id INT,
            FOREIGN KEY (user_id) REFERENCES users(id)

    */

class Packages
{
   public record GetAll_Data(int Id, string Name, string type, decimal TotalPrice, int Duration, int UserId)

    public static async Task<IResult>GetAll(Config config)
    {
        List<GetAll_Data>result = new();

        string query = "SELECT id, name, type, total_price, duration, userid FROM packages";

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while(reader.Read())
            {
                result.Add(new(
                    reader.GetInt32("id"),
                    reader.GetString("name"),
                    reader.GetString("type"),
                    reader.GetDecimal("total_price"),
                    reader.GetInt32("duration"),
                    reader.GetInt32("user_id")

                ));
            }
        }
        return Results.Ok(result);
    }

    public record Get_Data(string Name, string Type, decimal Price, int Duration, int UserId);

    public static async Task<IResult> Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT name, type, price, duration, user_id FROM packages WHERE id = @id";

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
                    reader.GetString("type"),
                    reader.GetDecimal("price"),
                    reader.GetInt32("duration"),
                    reader.GetInt32("user_id")
                    );
            }
        }

        if (result is null)
        {
            
            return Results.NotFound(new
            {
                message = $"not found."
            });
        }

        
        return Results.Ok(result);
    }

    public record CreateNew(string Name, string Type, decimal Price, int Duration, int UserId);

    public static async Task<IResult>Post(CreateNew packages, Config config)
    {
        string query = """
            INSERT INTO packages (name, type, price, duration, user_id)
            VALUES (@name, @type, @price, @duration, @user_id)
        """;
        var parameters = new MySqlParameter[]
            {
                new("@name", packages.Name),
                new("@type", packages.Type),
                new("@price", packages.Price),
                new("@duration", packages.Duration),
                new("@user_id", packages.UserId),
                
            };
            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

            return Results.Ok(new {message ="Package created."});
    }
   

    

}