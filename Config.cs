public class Config
{
    public string ConnectionString { get; set; }
    public Config(string conn) => ConnectionString = conn;
}
