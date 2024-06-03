using Microsoft.AspNetCore.Mvc;

namespace S7Plc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult> Get()
        {
            return Ok(new
            {
                deneme1 = S7.Net.Types.Int.FromByteArray(MyBackgroundService.datablock2Bytes.Skip(0).Take(2).ToArray()),
                deneme2 = S7.Net.Types.Int.FromByteArray(MyBackgroundService.datablock2Bytes.Skip(2).Take(2).ToArray()),
                deneme3 = S7.Net.Types.Int.FromByteArray(MyBackgroundService.datablock2Bytes.Skip(4).Take(2).ToArray()),
                deneme4 = S7.Net.Types.Int.FromByteArray(MyBackgroundService.datablock2Bytes.Skip(6).Take(2).ToArray()),
            });
        }
      
    }
}
