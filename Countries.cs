namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http; // för IResult and Results

class Countries
{
    // DTO för list results: GET /countries and /countries/search
    public record GetAll_Data(int Id, string CountryName);

    // GET /countries<
    //Vi hämtar länder för databasen
    public static List<GetAll_Data> GetAll(Config config)
    {
        List<GetAll_Data> result = new();

        string query = "SELECT id, country_name FROM countries";

        using (var reader = MySqlHelper.ExecuteReader(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1)
                ));
            }
        }

        return result;
    }


    // DTO för single result
    public record Get_Data(string CountryName);

    // GET /countries/{id}
    //specifikt land baserat på ID
    public static IResult Get(int id, Config config)
    {
        Get_Data? result = null;

        string query = "SELECT country_name FROM countries WHERE id = @id";

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
            // Om inget land hittas returnera felkod 404
            return Results.NotFound(new
            {
                message = $"Country with id {id} was not found."
            });
        }

        // Om det är ok returnera 200 med landet
        return Results.Ok(result);
    }


    // GET /countries/search?name=...
    //Checkar efter länder baserat på namn, SQl
    public static List<GetAll_Data> Search(string? name, Config config)
    {
        // Om ingen search term -> visa ALLT
        if (string.IsNullOrWhiteSpace(name))
        {
            return GetAll(config);
        }

        List<GetAll_Data> result = new();

        string query = """
            SELECT id, country_name
            FROM countries
            WHERE country_name LIKE CONCAT('%', @name, '%')
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
                    reader.GetInt32(0),
                    reader.GetString(1)
                ));
            }
        }

        return result;
    }
}
