namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

class Travelers
{
 public record GetAll_Data(int Id,string FirstName,string LastName,string PassportNumber,int Age,int BookingId);
 