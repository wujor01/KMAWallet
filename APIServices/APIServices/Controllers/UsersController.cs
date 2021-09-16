using APIServices.Models;
using APIServices.Security;
using APIServices.Services;
using APIServices.Services.HubSignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IHubContext<ChatHub> _hubContext;

        public UsersController(IUserService userService, IHubContext<ChatHub> hubContext)
        {
            _userService = userService;
            _hubContext = hubContext;
        }
        [HttpPost]
        public IActionResult Login(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [HttpGet]
        //[JWTAuthorize]
        public async Task<IActionResult> GetAll()
        {
            var lstUser = _userService.GetAll();
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "hi", "hello");
            return Ok(lstUser);
        }
    }
}
