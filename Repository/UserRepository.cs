using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Models;
using System.Linq;

namespace BankingPaymentsAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public User? GetById(int id) =>
            _context.Users.FirstOrDefault(u => u.Id == id);

        public User? GetByUsernameOrEmail(string usernameOrEmail) =>
            _context.Users.FirstOrDefault(u =>
                u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        public User Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        public void SoftDelete(User user)
        {
            user.IsActive = false; 
            _context.Users.Update(user);
            _context.SaveChanges();
        }
    }
}
