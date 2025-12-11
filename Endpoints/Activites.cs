namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http; // för IResult and Results

class Activities
{
    // DTO för list results: GET /activities and /activities/search
    public record GetAll_Data(int Id, string Name, string Description);

    // GET /activities
    // Hämtar alla food-aktiviteter från databasen
    public static List<GetAll_Data> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = "SELECT id, name, description FROM activities";

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)
                ));
            }
        }

        return result;
    }


    // DTO för single result
    public record Get_Data(string Name, string Description);

    // GET /activities/{id}
    // Hämtar specifik aktivitet baserat på ID
    public static IResult Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT name, description FROM activities WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                result = new(
                    reader.GetString(0),
                    reader.GetString(1)
                );
            }
        }

        if (result is null)
        {
            return Results.NotFound(new
            {
                message = $"Activity with id {id} was not found."
            });
        }

        return Results.Ok(result);
    }


    // GET /activities/search?term=...
    // Söker efter aktiviteter baserat på namn eller beskrivning
    public static List<GetAll_Data> Search(string? term, Config config)
    {
        // Om ingen search term -> visa ALLT
        if (string.IsNullOrWhiteSpace(term))
        {
            return GetAll(config);
        }

        List<GetAll_Data> result = new();

        string query = """
            SELECT id, name, description
            FROM activities
            WHERE name        LIKE CONCAT('%', @term, '%')
               OR description LIKE CONCAT('%', @term, '%')
        """;

        var parameters = new MySqlParameter[]
        {
            new("@term", term)
        };

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)
                ));
            }
        }

        return result;
    }
}
