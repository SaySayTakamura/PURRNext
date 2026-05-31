using APIServer.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [Route("api/{api:ApiVersion}/[Controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class IndexController : ControllerBase
    {
        private string _response;
        private readonly DatabaseService Database;

        public IndexController(DatabaseService db_service)
        {
            //Defines the database service upon Controller Initialization
            Console.WriteLine("Defining DATABASE");
            Database = db_service;
            Console.WriteLine("Done!");

            Console.WriteLine("Initializing Default Response");
            _response = "<h1>IT WORKS!!!!</h1>";
            Console.WriteLine("Done!");
        }
        //Defines a Test Method for returning HTML into the pipeline
        [HttpGet("Test")]
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