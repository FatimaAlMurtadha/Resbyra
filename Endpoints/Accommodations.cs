namespace server; 
using MySql.Data.MySqlClient;

using Microsoft.AspNetCore.Http;


class Accommodations
{
      // DTO for list results
      public record GetAll_Data(int Id, string Name, string Type, decimal PricePerNight, int DestinationId);

      // Get /accommodations
      // get all accommodations from database, no login (can consider create query that filters results)
      public static async Task <IResult> GetAll(Config config)
      {
            List<GetAll_Data> result = new();
            string query = "SELECT id, name, type, price_per_night, destionation_id FROM accommodations";

            using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
            {
                  while (reader.Read())
                  {
                        result.Add(new(
                              reader.GetInt32(0),
                              reader.GetString(1),
                              reader.GetString(2),
                              reader.GetDecimal(3),
                              reader.GetInt32(4)
                        ));
                  }
            }
            return Results.Ok(result);
      }
}