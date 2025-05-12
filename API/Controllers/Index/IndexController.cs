using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APIServer
{
    [Route("api/{api:ApiVersion}/[Controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class IndexController : ControllerBase
    {
        private string _response;

        public IndexController()
        {
            Console.WriteLine("Initializing Index Division");
            _response = "<h1>IT WORKS!!!!</h1>";
            Console.WriteLine("Done!");
        }
        [HttpGet]
        public ContentResult Get()
        {
            //Taken from:
            //https://stackoverflow.com/questions/26822277/return-html-from-asp-net-web-api
            //Content Type reference:
            //https://developer.mozilla.org/en-us/docs/web/http/guides/mime_types
            return new ContentResult
            {
                ContentType = "text/html",
                Content = _response
            };
        }
    }
}