using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using APIServer.Services;

namespace APIServer
{
    //This will be replaced by MiniServer
    public class Server
    {
        private int defaultPort = 2048;
        private WebApplication App;
        public Server(string address, int port = 2048)
        {
            //Creates the builder and assign an HTTP address to the WebHost
            var builder = WebApplication.CreateBuilder();

            //Port used by the server
            int internalPort;
            
            //Checks if the internal port is not the same as the default port
            if (port != defaultPort)
            {
                Console.WriteLine($"Assigning server to port - {port}");
                internalPort = port;
            }
            else
            {
                Console.WriteLine($"The selected port: {port}, conflicts with the Default Port, we will use that instead");
                internalPort = defaultPort;
            }
            
            //Overrides the Default IP Address, Port and Protocol used by the ASP NET Application
            builder.WebHost.UseUrls(urls: $"http://{address}:{internalPort}");
            builder.Services.AddControllers() //Adds management for Controllers and its actions
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null); //?????, is this really necessary? Remove in case of regret

            //Defines the entrypoint for the Database Service
            //May replace the IndexService with a more complex DatabaseService
            builder.Services.AddSingleton<DatabaseService>();

            //Allows us to set a version for the API
            builder.Services.AddApiVersioning();

            //Builds the Web Application and parse it to a variable
            var app = builder.Build();

            //Automatically redirects HTTP requests through HTTPS
            app.UseHttpsRedirection();
            app.UseAuthorization();

            //Same thing as Adds Controllers but now it maps the controllers
            app.MapControllers();

            //Defines a global variable so we can manipulate the application with other functions
            App = app;
        }

        //Starts the server on its own thread
        public void Start()
        {
            Console.WriteLine("Starting server");
            App.Run();
            Console.WriteLine("Server started");
        }

        //Stops said server
        public void Stop()
        {
            Console.WriteLine("Stopping server");
            App.StopAsync().Wait();
            Console.WriteLine("Server stopped");
        }
    }
}