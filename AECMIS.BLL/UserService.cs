using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Helpers;
using AECMIS.DAL.Nhibernate.Repositories;


namespace AECMIS.Service
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly IRepository<Role, int> _roleRepository;
        public const int UserNameMinLength = 7;
        public const int PasswordMinLength = 5;

        public UserService():this(null,null)
        {
            
         
        }

        public UserService(UserRepository userRepository, IRepository<Role, int> roleRepository )
        {
            _userRepository = userRepository?? new UserRepository();
            _roleRepository = roleRepository?? new Repository<Role, int>() ;
        }

        public bool CreateUser(string userName, string password, List<int> roles)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
            if (userName.Trim().Length < UserNameMinLength || password.Trim().Length < PasswordMinLength) return false;

            var rolesToAdd = _roleRepository.QueryList(x => roles.Contains(x.Id));
            var existingUserName = _userRepository.QueryList(x => x.UserName == userName).FirstOrDefault();
            if (existingUserName != null) return false;
            

            var user = new User {UserName = userName, Password =  PasswordHash.CreateHash(password)};
            user.Roles = rolesToAdd.Select(x => new UserRole{User = user, Role = x}).ToList();

            _userRepository.Save(user);
            return true;
        }

        public bool IsValidUser(string userName, string password, out int userId, bool doCollate = true)
        {
            var user = _userRepository.GetUserByUserName(userName,doCollate);
            var isValid = user != null && PasswordHash.ValidatePassword(password, user.Password);
            userId =  isValid?user.Id:0;
            return isValid;
        }

        public User GetUser(string userName)
        {
             return _userRepository.GetUserByUserName(userName);
        }
    }
}
