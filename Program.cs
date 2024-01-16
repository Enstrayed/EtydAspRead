using System.Runtime.InteropServices;
using StackExchange.Redis;
using System.Text.Json;
internal class Program
{
    public class GlobalConfig { // This class has to match the exact structure of the config.json
        public required int appPort {get; set;} // Each entry needs the {get; set;} bit otherwise it wont work
        public required string redisHost {get; set;} 
    }
    private static void Main(string[] args)
    {
        string gloConfAsJson = File.ReadAllText("config.json"); // Load config.json
        GlobalConfig globalConfig = JsonSerializer.Deserialize<GlobalConfig>(gloConfAsJson) ?? throw new Exception("Empty setting in JSON"); // deserialize config.json as the specified class

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(globalConfig.redisHost);
        IDatabase db = redis.GetDatabase();

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => { // handle SIGTERMs when running in docker
            Environment.Exit(0);
        });

        app.MapGet("{*url}", (HttpContext context) => {
            var dbResult = db.StringGet(context.Request.Path.ToString());
            if (dbResult.IsNull == true) { // If request doesnt exist in Redis
                context.Response.StatusCode = 404; // Return 404 to upstream proxy or client
            } else {
                context.Response.Redirect(dbResult.ToString()); 
            }
        });

        app.Run($"http://0.0.0.0:{globalConfig.appPort}");
    }
}