using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace ASP.NETCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public WeatherForecast Get(string location)
        {
            var forecast = new WeatherForecast
            {
                Date = DateTime.Now.AddDays(1),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
            HttpRequestLogContext.PushProperty("Location", location);
            HttpRequestLogContext.PushProperty("PredictedTemp", forecast.TemperatureC);

            _logger.LogInformation("Returning weather forecast");

            if (forecast.TemperatureC < -10)
                throw new Exception("It's going to be freezing cold!");

            return forecast;
        }
    }
}
