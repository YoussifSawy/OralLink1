using OralLink1.Models;
using System.Threading.Tasks;

namespace OralLink1.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateUser(string email, string password);
        Task<User> GetUserByEmail(string email);
    }
}