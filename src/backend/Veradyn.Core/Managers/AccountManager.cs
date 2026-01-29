using System.Linq;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Managers
{
    public class AccountManager
    {
        private readonly IUnitOfWork _uow;
        private readonly ICryptoProvider _crypto;

        public AccountManager(IUnitOfWork uow, ICryptoProvider crypto)
        {
            _uow = uow;
            _crypto = crypto;
        }

        public User Authenticate(string username, string password)
        {
            var user = _uow.Users.Find(u => u.Username == username).FirstOrDefault();
            if (user == null || !user.IsActive) return null;

            if (_crypto.VerifyPassword(user.PasswordHash, password))
            {
                return user;
            }
            return null;
        }
    }
}
