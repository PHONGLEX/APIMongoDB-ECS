using APIMongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIMongoDB.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IMongoCollection<WeatherForecast> _booksCollection;
        private readonly Serilog.ILogger _logger;

        public WeatherForecastService(
            IOptions<WeatherForecastDatabaseConnectionSettings> weatherForecastDatabaseSettings, Serilog.ILogger logger)
        {
            _logger = logger;

            _logger.Information(weatherForecastDatabaseSettings.Value.ConnectionString);
            _logger.Information(weatherForecastDatabaseSettings.Value.DatabaseName);
            _logger.Information(weatherForecastDatabaseSettings.Value.WeatherCollectionName);
            var mongoClient = new MongoClient(weatherForecastDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                weatherForecastDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<WeatherForecast>(
                weatherForecastDatabaseSettings.Value.WeatherCollectionName);
        }

        public async Task<List<WeatherForecast>> GetAsync()
        {
            try
            {
                return await _booksCollection.Find(_ => true).ToListAsync();
            }
            catch (System.Exception e)
            {
                _logger.Information(e.Message);
                throw;
            }
        }

        public async Task<WeatherForecast> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(WeatherForecast newWeatherForecast) =>
            await _booksCollection.InsertOneAsync(newWeatherForecast);

        public async Task UpdateAsync(string id, WeatherForecast updatedWeatherForecast) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedWeatherForecast);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);
    }
}
