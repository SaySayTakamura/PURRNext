using APIServer.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [Route("api/{api:ApiVersion}/[Controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class ScoreboardController : ControllerBase
    {
    }
}