using CSCMasterAPI.Persistence;
using CSCMasterAPI.Persistence.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSCMasterAPI.Persistence.Service
{
    public interface IUserService
    {
        public bool TryFeatchDetailForLogin(ref User user);
        public bool TryFeatchDetailForLogin(ref Member user);
    }
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserService(IServiceProvider serviceProvider)
        {
            _dbContext = (ApplicationDbContext)serviceProvider.GetService(typeof(ApplicationDbContext));
        }

        public bool TryFeatchDetailForLogin(ref User user)
        {
            var mobile = user.Phone;
            var password = user.Password;

            var userDetail = _dbContext.User.FirstOrDefault(u => u.Phone == mobile && u.Password == password);
            if (userDetail != null)
                user = userDetail;

            return (userDetail != null);
        }

        public bool TryFeatchDetailForLogin(ref Member user)
        {
            var mobile = user.Phone;
            var password = user.Password;

            var userDetail = _dbContext.Member.FirstOrDefault(u => u.Phone == mobile && u.Password == password);
            if (userDetail != null)
                user = userDetail;

            return (userDetail != null);
        }
    }
}
