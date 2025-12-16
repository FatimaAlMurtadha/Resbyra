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

app.MapGet("/packages/search", Packages.Search);

// PACKAGES
app.MapGet("/packages", Packages.Get);
app.MapGet("/packages/{id:int}", Packages.GetMore);

// CRUD Cities
app.MapGet("/cities", Cities.GetAll);
app.MapGet("/cities/search", Cities.Search);
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

// HOTELS
app.MapGet("/hotels", Hotels.GetAll);
app.MapGet("/hotels/{id:int}", Hotels.Get);
app.MapGet("/hotels/search", Hotels.Search);

// using proper verbs >_>
app.MapPost("/hotels", Hotels.Post);
app.MapPut("/hotels", Hotels.Put);
app.MapDelete("/hotels/{id:int}", Hotels.Delete);

//BookingRoom
app.MapGet("/booking-rooms", BookRoom.GetAll);
app.MapGet("/booking-rooms/{id:int}", BookRoom.Get);
app.MapPost("/booking-rooms", BookRoom.Post);
app.MapPut("/booking-rooms/{id:int}", BookRoom.Put);
app.MapDelete("/booking-rooms/{id:int}", BookRoom.Delete);


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
app.MapGet("/package_destinations/package/{packageId:int}", PackagesDestinations.GetByPackage);
app.MapGet("/package_destinations/destination/{destinationId:int}", PackagesDestinations.GetByDestination);
app.MapPost("/package_destinations", PackagesDestinations.Post);
app.MapDelete("/package_destinations/{packageId:int}/{destinationId:int}", PackagesDestinations.Delete);

// Hotels <-> Amenities
app.MapGet("/hotels/{hotelId:int}/amenities", AmenitiesHotel.ByHotel);
app.MapGet("/amenities/{amenityId:int}/hotels", AmenitiesHotel.ByAmenity);

app.MapPost("/hotels/amenities/link", AmenitiesHotel.Link);
app.MapDelete("/hotels/{hotelId:int}/amenities/{amenityId:int}", AmenitiesHotel.Unlink);

// Destinations <-> Activities
app.MapGet("/destinations/{destinationId:int}/activities", DestinationActivites.ByDestination);
app.MapGet("/activities/{activityId:int}/destinations", DestinationActivites.ByActivity);

app.MapPost("/destinations/activities/link", DestinationActivites.Link);
app.MapDelete("/destinations/{destinationId:int}/activities/{activityId:int}", DestinationActivites.Unlink);

// Packages <-> Activities
app.MapGet("/packages/{packageId:int}/activities", PackageActivities.ByPackage);

// app.MapGet("/activities/{activityId:int}/packages", PackageActivities.ByActivity);

app.MapPost("/packages/activities/link", PackageActivities.Link);
app.MapDelete("/packages/{packageId:int}/activities/{activityId:int}", PackageActivities.Unlink);

// CUSTOM CARDS ROUTES
app.MapGet("/custom-cards", CustomCards.GetAll);
app.MapGet("/custom-cards/{id}", CustomCards.Get);
app.MapGet("/custom-cards/search", CustomCards.Search);
app.MapPost("/custom-cards", CustomCards.Post);
app.MapPut("/custom-cards/{id}", CustomCards.Put);
app.MapDelete("/custom-cards/{id}", CustomCards.Delete);

// CustomCardsActivities ROUTES
app.MapGet("/custom-cards-activities/card/{cardId}", CustomCardsActivities.ByCard);
app.MapGet("/custom-cards-activities/activity/{activityId}", CustomCardsActivities.ByActivity);
app.MapPost("/custom-cards-activities", CustomCardsActivities.Link);
app.MapDelete("/custom-cards-activities/{cardId}/{activityId}", CustomCardsActivities.Unlink);
app.MapGet("/custom-cards-activities/search", CustomCardsActivities.Search);

// Packages <-> Amenities
app.MapGet("/packages/{packageId:int}/amenities", PackagesAmenities.ByPackage);
app.MapGet("/amenities/{amenityId:int}/packages", PackagesAmenities.ByAmenity);

app.MapPost("/packages/amenities/link", PackagesAmenities.Link);
app.MapDelete("/packages/{packageId:int}/amenities/{amenityId:int}", PackagesAmenities.Unlink);


// Packages <-> Hotels
app.MapGet("/packages/{packageId:int}/hotels", PackagesHotels.ByPackage);
app.MapGet("/hotels/{hotelId:int}/packages", PackagesHotels.ByHotel);

app.MapPost("/packages/hotels/link", PackagesHotels.Link);
app.MapDelete("/packages/{packageId:int}/hotels/{hotelId:int}", PackagesHotels.Unlink);

// Packages <-> Rooms
app.MapGet("/packages/{packageId:int}/rooms", PackagesRooms.ByPackage);
app.MapGet("/rooms/{roomId:int}/packages", PackagesRooms.ByRoom);

app.MapPost("/packages/rooms/link", PackagesRooms.Link);
app.MapDelete("/packages/{packageId:int}/rooms/{roomId:int}", PackagesRooms.Unlink);



// CustomCardsHotelss ROUTES  

app.MapGet("/custom-cards-hotels/card/{cardId}", CustomCardHotels.ByCard);

app.MapGet("/custom-cards-hotels/hotel/{hotelId}", CustomCardHotels.ByHotel);

app.MapPost("/custom-cards-hotels", CustomCardHotels.Link);

app.MapDelete("/custom-cards-hotels/{cardId}/{hotelId}", CustomCardHotels.Unlink);
// search method has cardId and *optional term*
app.MapGet("/custom-cards-hotels/search", CustomCardHotels.Search);

// CustomCardsRooms ROUTES

app.MapGet("/custom-cards-rooms/card/{cardId}", CustomCardsRooms.ByCard);
app.MapGet("/custom-cards-rooms/room/{roomId}", CustomCardsRooms.ByRoom);
app.MapPost("/custom-cards-rooms", CustomCardsRooms.Link);
app.MapDelete("/custom-cards-rooms/{cardId}/{roomId}", CustomCardsRooms.Unlink);
app.MapGet("/custom-cards-rooms/search", CustomCardsRooms.Search); // by card id 




// special, reset db
app.MapDelete("/db", db_reset_to_default);

app.Run();

async Task db_reset_to_default(Config config)
{
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS booking_rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS travelers");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS bookings");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_card_destinations");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_card_amenities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS packages_rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_card_rooms");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_card_hotels");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_card_activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS custom_cards");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS package_activities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS package_destinations");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS packages_amenities");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS packages_hotels");
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS packages");
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

  await MySqlHelper.ExecuteNonQueryAsync(
    config.ConnectionString,
    "INSERT INTO users(email, password, role) VALUES ('fatima@gmail.com','123','admin')"
  );
  await MySqlHelper.ExecuteNonQueryAsync(
    config.ConnectionString,
    "INSERT INTO users(email, password, role) VALUES ('ahmed@gmail.com','123','user')"
  );

  string countries_table = """
    CREATE TABLE countries (
      id INT AUTO_INCREMENT PRIMARY KEY,
      country_name VARCHAR(150) NOT NULL
    );
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, countries_table);

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO countries (country_name)
    VALUES ('Sweden'), ('Japan'), ('Spain'), ('Egypt');
  """);

  string cities_table = """
    CREATE TABLE cities (
      id INT AUTO_INCREMENT PRIMARY KEY,
      name VARCHAR(200) NOT NULL,
      country_id INT NOT NULL,
      FOREIGN KEY (country_id) REFERENCES countries(id)
    );
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, cities_table);

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO cities (name, country_id)
    VALUES
      ('Stockholm', 1),
      ('Tokyo', 2),
      ('Barcelona', 3),
      ('Cairo', 4);
  """);

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

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO destinations (name, description, climate, average_cost, city_id)
    VALUES
      ('Gamla stan', 'Gammlastan + museum', 'kallt', 1200.00, 1),
      ('Shibuya', 'Neon nights + sushi fun', 'Mild', 1800.00, 2),
      ('Barca Beach', 'BästaStrand + tapas + fotboll', 'Warmt', 1400.00, 3),
      ('Giza', 'Pyramider + bazaars + Nilen tour', 'RIKTIGT varmt', 1100.00, 4);
  """);

  string activities_table = """
    CREATE TABLE activities (
      id INT AUTO_INCREMENT PRIMARY KEY,
      name VARCHAR(200) NOT NULL,
      description TEXT NOT NULL
    );
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, activities_table);

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO activities (name, description)
    VALUES
      ('Italian Pizza Tasting', 'Taste authentic Italian pizzas prepared using traditional regional techniques.'),
      ('Swedish Mushroom Bonanza', 'Join a guided forest tour and sample classic Swedish mushroom dishes.'),
      ('Indian Spice Feast', 'Experience a rich selection of Indian dishes featuring diverse spices and regional flavors.'),
      ('American 15kg Burger Buffet', 'A wildly irresponsible buffet featuring oversized burgers, fries, and enough calories to frighten your doctor.');
  """);

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

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO destinations_activities (destination_id, activity_id)
    VALUES
      (1, 2),
      (2, 1),
      (2, 3);
  """);

  string amenities_table = """
    CREATE TABLE amenities(
      id INT AUTO_INCREMENT PRIMARY KEY,
      amenity_name VARCHAR(300)
    );
  """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, amenities_table);

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO amenities (amenity_name)
    VALUES ('Free WiFi'), ('Swimming Pool');
  """);

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
  
  

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO hotels (name, phone_number, rating, address, description, destination_id)
    VALUES
      ('Grand Nordic Hotel', '+12345678', 4.5, 'Gammlastan 12, Stockholm', 'Elegant hotel close to the museum.', 1),
      ('Sakura Hotell', '+87654321', 4.7, 'Shibuya Crossing 3-1, Tokyo', 'Modern hotel surrounded by nightlife and world-class food.', 2);
  """);

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
  
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                    INSERT INTO rooms (id, room_number, type, price_per_night, capacity, hotel_id)
                                                                    VALUES
                                                                      (1, 101, 'Standard', 900.00, 2, 1),
                                                                      (2, 102, 'Deluxe', 1200.00, 2, 1),
                                                                      (3, 201, 'Standard', 950.00, 2, 2),
                                                                      (4, 202, 'Suite',   1500.00, 4, 2);
                                                                  """);

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

  // FIX: seed packages BEFORE package_destinations
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
    INSERT INTO packages (id, name, type, total_price, duration_days, description, user_id)
    VALUES
      (1, 'Stockholm Starter', 'ready', 3000.00, 3, 'Gamla stan + museum + chill', NULL),
      (2, 'Tokyo Nights', 'ready', 8000.00, 5, 'Neon + sushi + kaos', NULL),
      (3, 'Barcelona Beach', 'ready', 6000.00, 4, 'Strand + tapas + fotboll', NULL),
      (4, 'Cairo Pyramids', 'ready', 5500.00, 4, 'Pyramider + bazaar + Nilen', NULL);
  """);

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
  
  
  string packages_rooms_table = """
                                  CREATE TABLE packages_rooms(
                                    package_id INT NOT NULL,
                                    room_id INT NOT NULL,
                                    PRIMARY KEY (package_id, room_id),
                                    FOREIGN KEY (package_id) REFERENCES packages(id),
                                    FOREIGN KEY (room_id) REFERENCES rooms(id)
                                  );
                                """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, packages_rooms_table);

// testdata
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                    INSERT INTO packages_rooms
                                                                    VALUES
                                                                      (1, 1),
                                                                      (1, 2),
                                                                      (2, 3),
                                                                      (3, 4);
                                                                  """);

  
  
  string packages_hotels_table = """
                                   CREATE TABLE packages_hotels(
                                     package_id INT NOT NULL,
                                     hotel_id INT NOT NULL,
                                     PRIMARY KEY (package_id, hotel_id),
                                     FOREIGN KEY (package_id) REFERENCES packages(id),
                                     FOREIGN KEY (hotel_id) REFERENCES hotels(id)
                                   );
                                 """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, packages_hotels_table);

  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                    INSERT INTO packages_hotels
                                                                    VALUES
                                                                      (1, 1),
                                                                      (2, 2),
                                                                      (3, 1),
                                                                      (4, 2);
                                                                  """);

  
  
  
  
  
  string packages_amenities_table = """
                                      CREATE TABLE packages_amenities(
                                        package_id INT NOT NULL,
                                        amenity_id INT NOT NULL,
                                        PRIMARY KEY (package_id, amenity_id),
                                        FOREIGN KEY (package_id) REFERENCES packages(id),
                                        FOREIGN KEY (amenity_id) REFERENCES amenities(id)
                                      );
                                    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, packages_amenities_table);

// lite testdata
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                    INSERT INTO packages_amenities
                                                                    VALUES (1, 1), (1, 2), (2, 1);
                                                                  """);


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
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  booking_id INT NOT NULL,
  room_id INT NOT NULL,
  check_in DATE NOT NULL,
  check_out DATE NOT NULL,
  guests INT NOT NULL,
  total_price DECIMAL(10,2) NOT NULL,

  FOREIGN KEY (user_id) REFERENCES users(id),
  FOREIGN KEY (booking_id) REFERENCES bookings(id),
  FOREIGN KEY (room_id) REFERENCES rooms(id)
);
""";
await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, booking_rooms_table);


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
  
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                    INSERT INTO custom_cards (id, user_id, title, budget, start_date, end_date)
                                                                    VALUES
                                                                      (1, 2, 'Fatima food tour', 5000.00, '2025-01-10', '2025-01-12'),
                                                                      (2, 2, 'Tokyo budget plan', 3000.00, '2025-02-01', '2025-02-05'),
                                                                      (3, 2, 'Barcelona chill', 4500.00, '2025-03-10', '2025-03-14'),
                                                                      (4, 2, 'Cairo chaos', 4000.00, '2025-04-01', '2025-04-04');
                                                                  """);
  
  

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

        // 
        // Add data
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
        INSERT INTO custom_card_activities
        VALUES (1, 1),(2, 2),(3, 3),(4, 4);
      """);

  // Create CustomCardsHotels table "The relation between them 2"
  string custom_card_hotels_table = """
        CREATE TABLE custom_card_hotels(
        card_id INT NOT NULL,
        hotel_id INT NOT NULL,
        PRIMARY KEY (card_id, hotel_id),
        FOREIGN KEY (card_id) REFERENCES custom_cards(id),
        FOREIGN KEY (hotel_id) REFERENCES hotels(id)
      );

      """;
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, custom_card_hotels_table);
      // Add Data
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                        INSERT INTO custom_card_hotels
                                                                        VALUES
                                                                          (1, 1),
                                                                          (2, 2),
                                                                          (3, 1),
                                                                          (4, 2);
                                                                      """);

  // Create CustomCardsRooms table "The relation between them 2"
  string custom_card_rooms_table = """
        CREATE TABLE custom_card_rooms(
        card_id INT NOT NULL,
        room_id INT NOT NULL,
        PRIMARY KEY (card_id, room_id),
        FOREIGN KEY (card_id) REFERENCES custom_cards(id),
        FOREIGN KEY (room_id) REFERENCES rooms(id)
      );

      """;
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, custom_card_rooms_table);
      // Add Data
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
        INSERT INTO custom_card_rooms
        VALUES (1, 1),(2, 2),(3, 3),(4, 4);
      """);

  // Create CustomCardsAmenities table "The relation between them 2"
  string custom_card_amenities_table = """
        CREATE TABLE custom_card_amenities(
        card_id INT NOT NULL,
        amenity_id INT NOT NULL,
        PRIMARY KEY (card_id, amenity_id),
        FOREIGN KEY (card_id) REFERENCES custom_cards(id),
        FOREIGN KEY (amenity_id) REFERENCES amenities(id)
      );

      """;
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, custom_card_amenities_table);
      // Add Data
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
                                                                        INSERT INTO custom_card_amenities
                                                                        VALUES
                                                                          (1, 1),
                                                                          (2, 2),
                                                                          (3, 1),
                                                                          (4, 2);
                                                                      """);


  // Create CustomCardDestinations table "The relation between them 2"
  string custom_card_destinations_table = """
        CREATE TABLE custom_card_destinations(
        card_id INT NOT NULL,
        destination_id INT NOT NULL,
        PRIMARY KEY (card_id, destination_id),
        FOREIGN KEY (card_id) REFERENCES custom_cards(id),
        FOREIGN KEY (destination_id) REFERENCES destinations(id)
      );

      """;
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, custom_card_destinations_table);
      // Add Data 
      await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, """
        INSERT INTO custom_card_destinations
        VALUES (1, 1),(2, 2),(3, 3),(4, 4);
      """);

}
