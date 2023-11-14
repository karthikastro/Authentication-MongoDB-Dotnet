using Amazon.Runtime.Internal;
using Authentication.Dtos;
using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Cryptography;
using Authentication.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Services
{
    public class UserServices:IUserServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMongoCollection<ApplicationUser> _users;
        public UserServices(IMongoClient mongoClient, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            var database = mongoClient.GetDatabase("AuthenticateDb");
            _users = database.GetCollection<ApplicationUser>("users");
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public List<ApplicationUser> GetUsers()
        {
            var result = _users.Find(x=>true).ToList();
            return result;
        }
        public ApplicationUser GetUserById(string id)
        {
            Guid ID = new Guid(id);
            var result = _users.Find(x=>x.Id == ID).FirstOrDefault();
            return result;
        } 

       
        public void Delete(string id)
        {
            Guid ID = new Guid(id);
            _users.DeleteOne(x => x.Id == ID);
        }
    }
}
