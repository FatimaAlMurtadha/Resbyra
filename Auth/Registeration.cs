namespace server;

using MySql.Data.MySqlClient;
// import the library of crybtography in order to secure the registeration 
using System.Security.Cryptography;
// import the library of text encoding utilities
using System.Text;

class Register // Matching the class name / Fatima
{
  // DTO for registration data
  public record Register_Data(string Email, string Password, string? Role); // Matching with the class name / Fatima

  // POST /register
  public static async Task<bool> Post(Register_Data data, Config config) // Matching with the class name / Fatima
  {
    // Check if email already exists

    string checkQuery = "SELECT id FROM users WHERE email = @Email";

    var checkParameters = new MySqlParameter[]
    {
            new("@Email", data.Email)
    };

    //  Here ScalarAsync is used in order to get that single value "Email" which is unique

    object? exists = await MySqlHelper.ExecuteScalarAsync(
        config.ConnectionString,
        checkQuery,
        checkParameters
    );

    if (exists != null)
    {
      // email already exists

      return false;
    }

    // Secure password

    string hashedPassword = HashPassword(data.Password);

    // Decide role (if null → user)
    string role = string.IsNullOrWhiteSpace(data.Role) ? "user" : data.Role!;

    // Insert new user

    string insertQuery = """
            INSERT INTO users(email, password, role)
            VALUES (@Email, @Password, @Role)
        """;

    var insertParameters = new MySqlParameter[]
    {
            new("@Email", data.Email),
            new("@Password", hashedPassword),
            new("@Role", role)
    };

// A NonQueryAsync is used in order to insert ("It is basically used with INSERT, UPDATE and delete")
    await MySqlHelper.ExecuteNonQueryAsync(
        config.ConnectionString,
        insertQuery,
        insertParameters
    );

    // Done
    return true;
  }

  // This function is a simple function in order to secure the password with SHA256
  private static string HashPassword(string password) // only password
  {
    // create the hashing object of SHA256
    using var sha_registeration = SHA256.Create();

    // Convert the the password from input "string" into bytes and count the hash
    byte[] bytes = sha_registeration.ComputeHash(Encoding.UTF8.GetBytes(password));

    // convert the result of hash-bytes into a base64 string in order to store the password 
    return Convert.ToBase64String(bytes);
  }
}