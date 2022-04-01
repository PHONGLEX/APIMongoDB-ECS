using APIMongoDB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIMongoDB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForcastService;
        private readonly Serilog.ILogger _logger;

        public WeatherForecastController(IWeatherForecastService weatherForcastService, Serilog.ILogger logger)
        {
            _weatherForcastService = weatherForcastService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<List<WeatherForecast>> Get() =>
            await _weatherForcastService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<WeatherForecast>> Get(string id)
        {
            var book = await _weatherForcastService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(WeatherForecast newWeatherForecast)
        {
            await _weatherForcastService.CreateAsync(newWeatherForecast);

            return CreatedAtAction(nameof(Get), new { id = newWeatherForecast.Id }, newWeatherForecast);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, WeatherForecast updatedWeatherForecast)
        {
            var book = await _weatherForcastService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            updatedWeatherForecast.Id = book.Id;

            await _weatherForcastService.UpdateAsync(id, updatedWeatherForecast);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _weatherForcastService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _weatherForcastService.RemoveAsync(id);

            return NoContent();
        }
    }
}
