using Dapper;
using Microsoft.Data.SqlClient;
using OralLink1.Models;
using System.Threading.Tasks;

namespace OralLink1.Services  
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> AuthenticateUser(string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                SELECT id as Id, name as Name, role as Role, 
                       email as Email, password as Password, 
                       createdAt as CreatedAt 
                FROM Users 
                WHERE email = @Email AND password = @Password";

            return await connection.QueryFirstOrDefaultAsync<User>(query, new
            {
                Email = email,
                Password = password
            });
        }

        public async Task<User> GetUserByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                SELECT id as Id, name as Name, role as Role, 
                       email as Email, password as Password, 
                       createdAt as CreatedAt 
                FROM Users 
                WHERE email = @Email";

            return await connection.QueryFirstOrDefaultAsync<User>(query, new
            {
                Email = email
            });
        }
    }
}