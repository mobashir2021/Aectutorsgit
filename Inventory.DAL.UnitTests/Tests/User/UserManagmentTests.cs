using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.Service;
using NUnit.Framework;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.DAL.Domain;
using AECMIS.DAL.UnitTests.Helpers;

namespace AECMIS.DAL.UnitTests.Tests.User
{
    [TestFixture]
    public class UserManagmentTests : BaseTest<AECMIS.DAL.Domain.User>
    {
        private UserService _userService;
        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _userService = new UserService(new  UserRepository(Session), new SqLiteRepository<Role, int>()
                
                );
        }

        public override void VerifyMapping()
        {
            throw new NotImplementedException();
        }
        
        [Test]
        public void CreateUserTest()
        {
            const int roleId = 1;
            //string userName = string.Format("Dappy1_{0}", DateTime.Now);
            string userName = "Aec12345";
            const string password = "easyt38ch";
            bool result = _userService.CreateUser(userName, password, new List<int> {roleId});

            Assert.IsTrue(result);
        }


        private void CreateDummyUser(string userName, string password)
        {
            const int roleId = 1;
            
             _userService.CreateUser(userName, password, new List<int> { roleId });
        }

       
        [Test]
        public void Verify_User_Validation_Works()
        {
            var userName = "Ayazaliuk";
            const string password = "Anewusertest54!";
            int userId;
            CreateDummyUser(userName,password);

            Assert.IsTrue(_userService.IsValidUser(userName, password, out userId, false));
            Assert.IsFalse(_userService.IsValidUser(userName, "tttt1", out userId, false));
        }
    }
}
