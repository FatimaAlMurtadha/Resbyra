namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
CREATE TABLE custom_card_activities (
            card_id INT NOT NULL,
            activity_id INT NOT NULL,
            PRIMARY KEY (card_id, activity_id),
            FOREIGN KEY (card_id) REFERENCES custom_cards(id),
            FOREIGN KEY (activity_id) REFERENCES activities(id)
        );
*/
class CustomCardsActivities
{
  // Link activity <-> custom card
  public record Link_Args(int CardId, int ActivityId);

  // All activities on one card
  public static async Task<IResult> ByCard(int cardId, Config config)
  {
    var result = new List<Activities.GetAll_Data>();

    string query = """
                         SELECT a.id, a.name, a.description
                         FROM activities AS a
                         INNER JOIN custom_card_activities AS ca ON ca.activity_id = a.id
                         WHERE ca.card_id = @cardId
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId)
    };

    using var reader =
        await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),
          reader.GetString(1),
          reader.GetString(2)
      ));
    }

    return Results.Ok(result);
  }

  // All custom cards that has an activity
  public static async Task<IResult> ByActivity(int activityId, Config config)
  {
    var result = new List<CustomCards.GetAll_Data>();

    string query = """
                         SELECT c.id, c.user_id, c.title, c.budget, c.start_date, c.end_date
                         FROM custom_cards AS c
                         JOIN custom_card_activities AS ca ON ca.card_id = c.id
                         WHERE ca.activity_id = @activityId
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@activityId", activityId)
    };

    using var reader =
        await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),// id
          reader.GetInt32(1), //user_id
          reader.GetString(2),//title
          reader.IsDBNull(3) ? null : reader.GetDecimal(3),// budget
          reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),//start_date
          reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5))// end_date
      ));
    }

    return Results.Ok(result);
  }

  // connect or link activity with custom card
  public static async Task<IResult> Link(Link_Args link, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
    {
      return userAuth;
    }
      

    string query = """
                         INSERT INTO custom_card_activities (card_id, activity_id)
                         VALUES (@cardId, @activityId)
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", link.CardId),
            new("@activityId", link.ActivityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Activity linked to custom card." });
  }

  // Delete the link
  public static async Task<IResult> Unlink(int cardId, int activityId, Config config, HttpContext ctx)
  {
    var userAuth = Authentication.RequireUser(ctx);
    if (userAuth is not null)
    {
      return userAuth;
    }


    string query = """
                         DELETE FROM custom_card_activities
                         WHERE card_id = @cardId
                           AND activity_id = @activityId
                       """;

    var parameters = new MySqlParameter[]
    {
            new("@cardId", cardId),
            new("@activityId", activityId)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

    return Results.Ok(new { message = "Activity unlinked from custom card." });
  }

  public static async Task<IResult> Search(int cardId, string? term, Config config)
  {
    var result = new List<Activities.GetAll_Data>();

    string query;
    MySqlParameter[] parameters;

    if (string.IsNullOrWhiteSpace(term))
    {
      // No term -> return all activities for this card
      query = """
                SELECT a.id, a.name, a.description
                FROM activities AS a
                INNER JOIN custom_card_activities AS ca ON ca.activity_id = a.id
                WHERE ca.card_id = @cardId
                """;

      parameters = new MySqlParameter[]
      {
            new("@cardId", cardId)
      };
    }
    else
    {
      // Search by activity name
      query = """
                SELECT a.id, a.name, a.description
                FROM activities AS a
                INNER JOIN custom_card_activities AS ca ON ca.activity_id = a.id
                WHERE ca.card_id = @cardId
                  AND a.name LIKE @term
                """;

      parameters = new MySqlParameter[]
      {
            new("@cardId", cardId),
            new("@term", "%" + term + "%")
      };
    }

    using var reader =
        await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters);

    while (reader.Read())
    {
      result.Add(new(
          reader.GetInt32(0),
          reader.GetString(1),
          reader.GetString(2)
      ));
    }

    return Results.Ok(result);
  }
  
} //fatima