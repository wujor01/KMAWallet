using APIServices.Security;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [JWTAuthorize]
    public class BaseController : ControllerBase
    {
    }
}
