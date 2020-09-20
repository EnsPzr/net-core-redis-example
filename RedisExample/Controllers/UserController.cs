using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RedisExample.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace RedisExample.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IRedisCacheClient _redisCacheClient;
        private const string CacheName = "users";

        public UserController(IRedisCacheClient redisCacheClient)
        {
            _redisCacheClient = redisCacheClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await GetUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var users = await GetUsers();
            var user = users.FirstOrDefault(x => x.UserId == id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserModel model)
        {
            var users = await GetUsers();
            users.Add(model);
            var result = await _redisCacheClient.Db0.AddAsync(CacheName, users, DateTimeOffset.Now.AddDays(1));
            if (result) return Ok();
            return BadRequest();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserModel model)
        {
            var users = await GetUsers();

            var user = users.FirstOrDefault(x => x.UserId == id);
            if (user == null) return NotFound();

            users.Remove(user);

            user.Email = model.Email;
            user.Name = model.Name;
            user.Surname = model.Surname;
            user.UserId = model.UserId;

            users.Add(user);

            var result = await _redisCacheClient.Db0.AddAsync(CacheName, users, DateTimeOffset.Now.AddDays(1));
            if (result) return Ok();
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var users = await GetUsers();
            var user = users.FirstOrDefault(x => x.UserId == id);
            if (user == null) return NotFound();

            users.Remove(user);

            var result = await _redisCacheClient.Db0.AddAsync(CacheName, users, DateTimeOffset.Now.AddDays(1));
            if (result) return Ok();
            return BadRequest();
        }


        [NonAction]
        private async Task<List<UserModel>> GetUsers()
        {
            var users = await _redisCacheClient.Db0.GetAsync<List<UserModel>>(CacheName);
            return users ?? new List<UserModel>();
        }
    }
}