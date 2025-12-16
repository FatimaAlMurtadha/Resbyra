using MySql.Data.MySqlClient;

namespace server;

 /*
            +++ Packages
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            type ENUM('ready','custom') NOT NULL DEFAULT 'ready',
            total_price DECIMAL(10,2) NOT NULL,
            duration_days INT NOT NULL,
            description TEXT NOT NULL,
            user_id INT,
            FOREIGN KEY (user_id) REFERENCES users(id)


            +++ destinations 
            id INT AUTO_INCREMENT PRIMARY KEY,
            description TEXT NOT NULL,
            climate VARCHAR(100) NOT NULL,
            average_cost DECIMAL(10,2) NOT NULL,
            city_id INT NOT NULL,
            FOREIGN KEY (city_id) REFERENCES cities(id)

            +++Activities
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            description TEXT NOT NULL

    */

class Packages
{
    public record ThePackage(int Id, string Name, decimal TotalPrice, int DurationDays, string Description, List<TheActivity>Activities,
    List<TheDestination>Destinations);

    public record TheActivity(int Id, string Name, string Description);
    public record TheDestination(int Id, string Description, string Climate, decimal AverageCost);

    public static async Task<IResult> Get(Config config)
    {
        var packages = new List<object>();
        
        string query = """
            SELECT id, name, total_price, duration_days, description FROM packages WHERE type = 'ready' 
        """;   

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                int packageId = reader.GetInt32("id");
                
                var destinations = await GetDestinations(packageId, config);
                var activities = await GetActivities(packageId, config);

                packages.Add(new
                {
                    Id = packageId,
                    Name = reader.GetString("name"),
                    TotalPrice = reader.GetDecimal("total_price"),
                    DurationDays = reader.GetInt32("duration_days"),
                    Description = reader.GetString("description"),
                    Destinations = destinations,  
                    Activities = activities
                });
            }
        } 
        return Results.Ok(packages);
    }

    public static async Task<IResult> GetMore(int id, Config config)
    {
        string query = """
            SELECT id, name, total_price, duration_days, description FROM packages WHERE id = @id AND type = 'ready' 
        """;

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (!reader.Read())
            {
                return Results.NotFound(new { message = $"Not found." });
            }

            var packageName = reader.GetString("name");
            var totalPrice = reader.GetDecimal("total_price");
            var durationDays = reader.GetInt32("duration_days");
            var description = reader.GetString("description");

            var destinations = await GetDestinations(id, config);
            var activities = await GetActivities(id, config);

            var thepackage = new ThePackage(
                Id: id,
                Name: packageName,
                TotalPrice: totalPrice,
                DurationDays: durationDays,
                Description: description,
                Destinations: destinations,
                Activities: activities
            );

            return Results.Ok(thepackage);
        }
    }
    private static async Task<List<TheDestination>> GetDestinations(int packageId, Config config)
    {
        var destinations = new List<TheDestination>();

        string query = """
            SELECT a.id, a.description, a.climate, a.average_cost FROM destinations a
            INNER JOIN package_destinations b ON a.id = b.destination_id
            WHERE b.package_id = @packageId
        """;

        var parameters = new MySqlParameter[] 
        { 
            new("@packageId", packageId) 
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                destinations.Add(new TheDestination(
                    Id: reader.GetInt32("id"),
                    Description: reader.GetString("description"),
                    Climate: reader.GetString("climate"),
                    AverageCost: reader.GetDecimal("average_cost")
                ));
            }
        }

        return destinations;
    }

    private static async Task<List<TheActivity>> GetActivities(int packageId, Config config)
    {
        var activities = new List<TheActivity>();

        string query = """
            SELECT c.id, c.name, c.description FROM activities c
            INNER JOIN package_activities d ON c.id = d.activity_id
            WHERE d.package_id = @packageId
        """;

        var parameters = new MySqlParameter[] 
        { 
            new("@packageId", packageId) 
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                activities.Add(new TheActivity(
                    Id: reader.GetInt32("id"),
                    Name: reader.GetString("name"),
                    Description: reader.GetString("description")
                ));
            }
        }

        return activities;
    }
    
    
    // GET /packages/search?term=...
public static async Task<IResult> Search(string? term, Config config)
{
    var result = new List<object>();

    string query;
    MySqlParameter[]? parameters = null;

    if (string.IsNullOrWhiteSpace(term))
    {
        query = """
            SELECT id, name, total_price, duration_days, description
            FROM packages
            WHERE type = 'ready'
            ORDER BY id ASC
        """;
    }
    else
    {
        query = """
            SELECT id, name, total_price, duration_days, description
            FROM packages
            WHERE type = 'ready'
              AND (name LIKE @term OR description LIKE @term)
            ORDER BY id ASC
        """;

        parameters = new MySqlParameter[]
        {
            new("@term", "%" + term + "%")
        };
    }

    if (parameters is null)
    {
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                int packageId = reader.GetInt32("id");

                var destinations = await GetDestinations(packageId, config);
                var activities = await GetActivities(packageId, config);

                result.Add(new
                {
                    Id = packageId,
                    Name = reader.GetString("name"),
                    TotalPrice = reader.GetDecimal("total_price"),
                    DurationDays = reader.GetInt32("duration_days"),
                    Description = reader.GetString("description"),
                    Destinations = destinations,
                    Activities = activities
                });
            }
        }
    }
    else
    {
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                int packageId = reader.GetInt32("id");

                var destinations = await GetDestinations(packageId, config);
                var activities = await GetActivities(packageId, config);

                result.Add(new
                {
                    Id = packageId,
                    Name = reader.GetString("name"),
                    TotalPrice = reader.GetDecimal("total_price"),
                    DurationDays = reader.GetInt32("duration_days"),
                    Description = reader.GetString("description"),
                    Destinations = destinations,
                    Activities = activities
                });
            }
        }
    }

    return Results.Ok(result);
}


}