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
// Registeration feature
app.MapPost("/register", Register.Post);

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

// COUNTRIES
app.MapGet("/countries", Countries.GetAll);
app.MapGet("/countries/{id:int}", Countries.Get);
app.MapGet("/countries/search", Countries.Search);

app.MapPost("/countries", Countries.Post);
app.MapPut("/countries/{id:int}", Countries.Put);
app.MapDelete("/countries/{id:int}", Countries.Delete);


// CRUD Cities 

app.MapGet("/cities", Cities.GetAll);

// SEARCH
app.MapGet("/cities/search", Cities.Search);

// get by id, only int
app.MapGet("/cities/{id:int}", Cities.Get);
app.MapPost("/cities", Cities.Post);
app.MapPut("/cities/{id:int}", Cities.Put);
app.MapDelete("/cities/{id:int}", Cities.Delete);

// CRUD Destinations
app.MapGet("/destinations", Destinations.GetAll);
app.MapGet("/destinations/search", Destinations.Search); 
app.MapGet("/destinations/{id:int}", Destinations.Get);
app.MapPost("/destinations", Destinations.Post);
app.MapPut("/destinations/{id:int}", Destinations.Put);
app.MapDelete("/destinations/{id:int}", Destinations.Delete);

// Bookings
app.MapGet("/bookings", Bookings.GetAll);
app.MapGet("/bookings/search", Bookings.Search); 
app.MapGet("/bookings/{id:int}", Bookings.Get);
app.MapPost("/bookings", Bookings.Post);
app.MapPut("/bookings/{id:int}", Bookings.Put);
app.MapDelete("/bookings/{id:int}", Bookings.Delete);

//
// HOTELS
app.MapGet("/hotels", Hotels.GetAll);
app.MapGet("/hotels/{id:int}", Hotels.Get);
app.MapGet("/hotels/search", Hotels.Search);

// using proper verbs >_>
app.MapPost("/hotels", Hotels.Post);
app.MapPut("/hotels", Hotels.Put);
app.MapDelete("/hotels/{id:int}", Hotels.Delete);

// avoid route clash with /hotels/{id} which caused issue before I think
app.MapGet("/hotels/by-destination/{destinationId:int}", Hotels.ByDestination);


// ROOMS
app.MapGet("/rooms", Rooms.GetAll);
app.MapGet("/rooms/{id:int}", Rooms.Get);
app.MapGet("/rooms/search", Rooms.Search);
// filter endpoints
app.MapGet("/rooms/by-hotel/{hotelId:int}", Rooms.ByHotel);
app.MapGet("/rooms/by-hotel/{hotelId:int}/{roomNumber:int}", Rooms.ByRoomNumber);
app.MapPost("/rooms", Rooms.Post);
app.MapPut("/rooms/{id:int}", Rooms.Put);
app.MapDelete("/rooms/{id:int}", Rooms.Delete);


// Activities
app.MapGet("/activities", () => Activities.GetAll(config));
app.MapGet("/activities/{id:int}", (int id) => Activities.Get(id, config));
app.MapGet("/activities/search", (string? term) => Activities.Search(term, config));


// Amenities
app.MapGet("/amenities", Amenities.GetAll);
app.MapGet("/amenities/search", Amenities.Search);
app.MapGet("/amenities/{id:int}", Amenities.Get);
app.MapPost("/amenities", Amenities.Post);
app.MapPut("/amenities/{id:int}", Amenities.Put);
app.MapDelete("/amenities/{id:int}", Amenities.Delete);

// package_destinations
app.MapGet("/package_destinations", PackagesDestinations.GetAll);
app.MapGet("/package_destinations/package/{id}", PackagesDestinations.GetByPackage); // by packageId 
app.MapGet("/package_destinations/destination/{id}", PackagesDestinations.GetByDestination); // by DestinationId
app.MapPost("/package_destinations", PackagesDestinations.Post);
app.MapDelete("/package_destinations/{packageId}/{destinationId}", PackagesDestinations.Delete);

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
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS booking_rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS amenities_hotels");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS amenities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS hotels");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS destinations_activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS destinations");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS cities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS countries");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS users");


  /////////////////////// Create all tables //////////////////////////
  // Users' table
  string users_table = """
        CREATE TABLE users
        (
            id INT PRIMARY KEY AUTO_INCREMENT,
            email varchar(256) NOT NULL UNIQUE,
            password TEXT NOT NULL, 
            role VARCHAR(50) NOT NULL DEFAULT 'user'
        )
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, users_table);
  // TEST
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "INSERT INTO users(email, password, role) VALUES ('fatima@gmail.com','123, admin')");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "INSERT INTO users(email, password, role) VALUES ('ahmed@gmail.com','123, user')");

// Countries' table
  string countries_table = """
                               CREATE TABLE countries (
                                   id INT AUTO_INCREMENT PRIMARY KEY,
                                   country_name VARCHAR(150) NOT NULL
                               );
                           """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, countries_table);

  // Seed countries
  string insertCountry1 = """
                              INSERT INTO countries (country_name)
                              VALUES ('Sweden');
                          """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCountry1);

  string insertCountry2 = """
                              INSERT INTO countries (country_name)
                              VALUES ('Japan');
                          """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCountry2);

  string insertCountry3 = """
                              INSERT INTO countries (country_name)
                              VALUES ('Spain');
                          """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCountry3);

  string insertCountry4 = """
                              INSERT INTO countries (country_name)
                              VALUES ('Egypt');
                          """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCountry4);

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

  //
  string insertCity1 = """
                           INSERT INTO cities (name, country_id)
                           VALUES ('Stockholm', 1);
                       """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCity1);

  string insertCity2 = """
                           INSERT INTO cities (name, country_id)
                           VALUES ('Tokyo', 2);
                       """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCity2);

  string insertCity3 = """
                           INSERT INTO cities (name, country_id)
                           VALUES ('Barcelona', 3);
                       """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCity3);

  string insertCity4 = """
                           INSERT INTO cities (name, country_id)
                           VALUES ('Cairo', 4);
                       """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertCity4);

    // Destinations' table
    string destinations_table = """
        CREATE TABLE destinations (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            description TEXT NOT NULL,
            climate VARCHAR(100) NOT NULL,
            average_cost DECIMAL(10,2) NOT NULL,
            city_id INT NOT NULL,
            FOREIGN KEY (city_id) REFERENCES cities(id)
        );
    """;

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, destinations_table);
    // TEST destinations
    // 1 stockholm
    string insertDestination1 = """
                           INSERT INTO destinations (name, description, climate, average_cost, city_id)
                           VALUES ( 'Djurgården Island', 'A scenic island in Stockholm known for its museums, green parks, waterfront views, and family‑friendly attractions.', 
                           'Cold temperate', 1100.00, 1);
                       """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertDestination1);
    // 2 Tokyo
    string insertDestination2 = """
                           INSERT INTO destinations (name, description, climate, average_cost, city_id)
                           VALUES ('Old city','Historic old town known for its narrow cobblestone streets, colorful buildings, and medieval architecture.', 
                           'Cold temperate', 1200.00, 2);
                       """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertDestination2);

    // 3 Barcha
    string insertDestination3 = """
                           INSERT INTO destinations (name, description, climate, average_cost, city_id)
                           VALUES ('Park Güell', 'A world‑famous public park designed by Antoni Gaudí, featuring colorful mosaics, unique architecture, and panoramic views of Barcelona.','Mediterranean', 1500.00, 3);
                       """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertDestination3);

    // 4 Cairo
    string insertDestination4 = """
                           INSERT INTO destinations (name, description, climate, average_cost, city_id)
                           VALUES ( 'Khan el‑Khalili Bazaar', 'A historic marketplace in Cairo known for its vibrant shops, traditional crafts, spices, and rich cultural atmosphere.','Hot desert', 400.00, 4);
                       """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertDestination4);

    //

    // Food Activities' table
    string activities_table = """
        CREATE TABLE IF NOT EXISTS activities (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(200) NOT NULL,
            description TEXT NOT NULL
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, activities_table);

  string insertItalianPizza = """
      INSERT INTO activities (name, description)
      VALUES ('Italian Pizza Tasting', 'Taste authentic Italian pizzas prepared using traditional regional techniques.');
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertItalianPizza);

  string insertSwedishMushroom = """
      INSERT INTO activities (name, description)
      VALUES ('Swedish Mushroom Bonanza', 'Join a guided forest tour and sample classic Swedish mushroom dishes.');
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertSwedishMushroom);

  string insertIndianFeast = """
      INSERT INTO activities (name, description)
      VALUES ('Indian Spice Feast', 'Experience a rich selection of Indian dishes featuring diverse spices and regional flavors.');
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertIndianFeast);

  string insertAmericanBurger = """
      INSERT INTO activities (name, description)
      VALUES ('American 15kg Burger Buffet', 'A wildly irresponsible buffet featuring oversized burgers, fries, and enough calories to frighten your doctor.');
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertAmericanBurger);

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

  // Amenities' table
  string amenities_table = """
         CREATE TABLE amenities(
            id INT AUTO_INCREMENT PRIMARY KEY,
            amenity_name VARCHAR(300)
         );
  """;
  
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, amenities_table);

  // Seedar amenities (för testing)
  string insertAmenity1 = """
                              INSERT INTO amenities (amenity_name)
                              VALUES ('Free WiFi');
                          """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertAmenity1);

  string insertAmenity2 = """
                              INSERT INTO amenities (amenity_name)
                              VALUES ('Swimming Pool');
                          """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertAmenity2);

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
            room_number INT NOT NULL,
            type VARCHAR(100) NOT NULL,
            price_per_night DECIMAL(10,2) NOT NULL,
            capacity INT NOT NULL,
            hotel_id INT NOT NULL,
            FOREIGN KEY (hotel_id) REFERENCES hotels(id)
        );
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, rooms_table);

  // The relation between the amenities and hotels
  string amenities_hotels = """
         CREATE TABLE amenities_hotels(
            amenity_id INT NOT NULL,
            hotel_id INT NOT NULL,
            PRIMARY KEY (amenity_id, hotel_id),
            FOREIGN KEY (amenity_id) REFERENCES amenities(id),
            FOREIGN KEY (hotel_id) REFERENCES hotels(id) 
         );
  """;
  
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, amenities_hotels);

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

    // Seeds // Test
    // 1
    string insertPackageDestination1 = """
                              INSERT INTO package_destinations
                              VALUES (1, 1),(2, 2),(3, 3),(4,4);
                          """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertPackageDestination1);


    // Create a view in order to view all packages_destination information
    // a view would be faster and stable
    string create_view = """
         CREATE OR REPLACE VIEW view_package_destinations AS 
         SELECT p.id AS package_id, p.name AS package_name, p.type, p.total_price, 
                p.duration_days, p.description AS package_description, 
                d.id AS destination_id, d.name AS destination_name, d.climate,
                c.name AS city_name
         FROM package_destinations AS pd
         INNER JOIN packages AS p ON pd.package_id = p.id
         INNER JOIN destinations AS d ON pd.destination_id = d.id
         INNER JOIN cities AS c ON d.city_id = c.id;
  
  """;
    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, create_view);
    // end of the view


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
	