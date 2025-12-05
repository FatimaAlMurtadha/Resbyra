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


// special, reset db
app.MapDelete("/db", db_reset_to_default);






app.Run();




async Task db_reset_to_default(Config config)
{

  // Drop all tables from database
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS users");

  // Create all tables
  string users_table = """
        CREATE TABLE users
        (
            id INT PRIMARY KEY AUTO_INCREMENT,
            email varchar(256) NOT NULL UNIQUE,
            password TEXT
        )
    """;
  await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, users_table);
}