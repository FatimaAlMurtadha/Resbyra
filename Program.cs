using MySql.Data.MySqlClient;
using server;

var builder = WebApplication.CreateBuilder(args);
// 127.0.0.1:3306 (default port)
Config config = new("server=127.0.0.1;uid=resbyra;pwd=resbyra;database=resbyra");
builder.Services.AddSingleton<Config>(config);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

var app = builder.Build();
app.UseSession();

// REST routes
// session / login / logout examples (auth resource)
app.MapGet("/login", Login.Get);
app.MapPost("/login", Login.Post);
app.MapDelete("/login", Login.Delete);

// CRUD examples (user resource)
app.MapGet("/users", Users.GetAll);
app.MapGet("/users/{id}", Users.Get);
app.MapPost("/users", Users.Post);
app.MapPut("/users/{id}", Users.Put);
app.MapDelete("/users/{id}", Users.Delete);

app.MapGet("/countries", Countries.GetAll);
app.MapGet("/countries/{id:int}", Countries.Get);       // note :int constraint
app.MapGet("/countries/search", Countries.Search);

app.MapGet("/hotels", Hotels.GetAll);
app.MapGet("/hotels/{id:int}", Hotels.Get);     
app.MapGet("/hotels/search", Hotels.Search);


// special, reset db
app.MapDelete("/db", db_reset_to_default);






app.Run();




async Task db_reset_to_default(Config config)
{

  // Drop all tables from database
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS booking_rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS travelers");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS bookings");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_card_activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_cards");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS package_activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS package_destinations");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS packages");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS hotels");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS destinations_activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS destinations");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS cities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS countries");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS users");

  /////////////////////// Create all tables /////////////////////////
  // Users' table
  string users_table = """
        CREATE TABLE users
        (
            id INT PRIMARY KEY AUTO_INCREMENT,
            email varchar(256) NOT NULL UNIQUE,
            password TEXT
        )
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, users_table);
    // TEST
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "INSERT INTO users(email, password) VALUES ('fatima@gmail.com','123')");
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "INSERT INTO users(email, password) VALUES ('ahmed@gmail.com','123')");

    // Countries' table
    string countries_table = """
        CREATE TABLE countries (
            id INT AUTO_INCREMENT PRIMARY KEY,
            country_name VARCHAR(150) NOT NULL
            
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, countries_table);

  // Cities' table
  string cities_table = """
        CREATE TABLE cities (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            country_id INT NOT NULL,
            FOREIGN KEY (country_id) REFERENCES countries(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, cities_table);

  // Destinations' table
  string destinations_table = """
        CREATE TABLE destinations (
            id INT AUTO_INCREMENT PRIMARY KEY,
            description TEXT NOT NULL,
            climate VARCHAR(100) NOT NULL,
            average_cost DECIMAL(10,2) NOT NULL,
            city_id INT NOT NULL,
            FOREIGN KEY (city_id) REFERENCES cities(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, destinations_table);

  // Activities' table
  string activities_table = """
        CREATE TABLE activities (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            type VARCHAR(100) NOT NULL,
            price DECIMAL(10,2) NOT NULL,
            description TEXT NOT NULL
        );
    """;

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, activities_table);

  // The relation between the destinations and the activities M:N

  string destinations_activities_table = """
        CREATE TABLE destinations_activities (
            destination_id INT NOT NULL,
            activity_id INT NOT NULL,
            PRIMARY KEY (destination_id, activity_id),
            FOREIGN KEY (destination_id) REFERENCES destinations(id),
            FOREIGN KEY (activity_id) REFERENCES activities(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, destinations_activities_table);

  // Hotels' table
  string hotels_table = """
        CREATE TABLE hotels (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            phone_number VARCHAR(50),
            rating DECIMAL(3,1) NOT NULL,
            address VARCHAR(255) NOT NULL,
            description TEXT NOT NULL,
            destination_id INT NOT NULL,
            FOREIGN KEY (destination_id) REFERENCES destinations(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, hotels_table);

  // Rooms' table
  string rooms_table = """
        CREATE TABLE rooms (
            id INT AUTO_INCREMENT PRIMARY KEY,
            number INT NOT NULL,
            type VARCHAR(100) NOT NULL,
            price_per_night DECIMAL(10,2) NOT NULL,
            capacity INT NOT NULL,
            hotel_id INT NOT NULL,
            FOREIGN KEY (hotel_id) REFERENCES hotels(id)
        );
    """;

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, rooms_table);

  // Packages' table
  string packages_table = """
        CREATE TABLE packages (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            type ENUM('ready','custom') NOT NULL DEFAULT 'ready',
            total_price DECIMAL(10,2) NOT NULL,
            duration_days INT NOT NULL,
            description TEXT NOT NULL,
            user_id INT,
            FOREIGN KEY (user_id) REFERENCES users(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, packages_table);

  // The relation between the packages and the destinations M:N
  string package_destinations_table = """
        CREATE TABLE package_destinations (
            package_id INT NOT NULL,
            destination_id INT NOT NULL,
            PRIMARY KEY (package_id, destination_id),
            FOREIGN KEY (package_id) REFERENCES packages(id),
            FOREIGN KEY (destination_id) REFERENCES destinations(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, package_destinations_table);

  // The relation between the packages and the activities M:N
  string package_activities_table = """
        CREATE TABLE package_activities (
            package_id INT NOT NULL,
            activity_id INT NOT NULL,
            PRIMARY KEY (package_id, activity_id),
            FOREIGN KEY (package_id) REFERENCES packages(id),
            FOREIGN KEY (activity_id) REFERENCES activities(id)
        );
    """;

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, package_activities_table);

  // Bookings' table
  string bookings_table = """
        CREATE TABLE bookings (
            id INT AUTO_INCREMENT PRIMARY KEY,
            total_price DECIMAL(10,2) NOT NULL,
            date DATETIME NOT NULL,
            user_id INT NOT NULL,
            package_id INT NOT NULL,
            FOREIGN KEY (user_id) REFERENCES users(id),
            FOREIGN KEY (package_id) REFERENCES packages(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, bookings_table);

  // Travelers' table
  string travelers_table = """
        CREATE TABLE travelers (
            id INT AUTO_INCREMENT PRIMARY KEY,
            first_name VARCHAR(100) NOT NULL,
            last_name VARCHAR(100) NOT NULL,
            passport_number VARCHAR(100) NOT NULL,
            age INT NOT NULL,
            booking_id INT NOT NULL,
            FOREIGN KEY (booking_id) REFERENCES bookings(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, travelers_table);

  // The relation between the bookings and the rooms M:N
  string booking_rooms_table = """
        CREATE TABLE booking_rooms (
            booking_id INT NOT NULL,
            room_id INT NOT NULL,
            nights INT NOT NULL,
            price DECIMAL(10,2) NOT NULL,
            PRIMARY KEY (booking_id, room_id),
            FOREIGN KEY (booking_id) REFERENCES bookings(id),
            FOREIGN KEY (room_id) REFERENCES rooms(id)
        );
    """;

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, booking_rooms_table);

  // Custom cards' table
  string custom_cards_table = """
        CREATE TABLE custom_cards (
            id INT AUTO_INCREMENT PRIMARY KEY,
            user_id INT NOT NULL,
            title VARCHAR(200) NOT NULL,
            budget DECIMAL(10,2),
            start_date DATE,
            end_date DATE,
            FOREIGN KEY (user_id) REFERENCES users(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, custom_cards_table);

  // The relation between the custom cards and activities M:N
  string custom_card_activities_table = """
        CREATE TABLE custom_card_activities (
            card_id INT NOT NULL,
            activity_id INT NOT NULL,
            PRIMARY KEY (card_id, activity_id),
            FOREIGN KEY (card_id) REFERENCES custom_cards(id),
            FOREIGN KEY (activity_id) REFERENCES activities(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, custom_card_activities_table);
}