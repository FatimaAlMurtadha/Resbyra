namespace server;

using MySql.Data.MySqlClient;

class Countries
{
    // DTO for list results: GET /countries and /countries/search
    public record GetAll_Data(int Id, string CountryName);

    // GET /countries
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


    // DTO for single result: GET /countries/{id}
    public record Get_Data(string CountryName);

    // GET /countries/{id}
    public static Get_Data? Get(int id, Config config)
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

        return result;
    }


    // GET /countries/search?name=...
    public static List<GetAll_Data> Search(string? name, Config config)
    {
        // If no search term → just return everything
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
