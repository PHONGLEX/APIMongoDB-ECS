using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIMongoDB.Services
{
    public interface IWeatherForecastService
    {
        Task<List<WeatherForecast>> GetAsync();

        Task<WeatherForecast> GetAsync(string id);

        Task CreateAsync(WeatherForecast newWeatherForecast);

        Task UpdateAsync(string id, WeatherForecast updatedWeatherForecast);

        Task RemoveAsync(string id);
    }
}
