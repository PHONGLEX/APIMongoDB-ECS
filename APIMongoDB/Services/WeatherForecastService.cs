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

        public WeatherForecastService(
            IOptions<WeatherForecastDatabaseConnectionSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<WeatherForecast>(
                bookStoreDatabaseSettings.Value.WeatherCollectionName);
        }

        public async Task<List<WeatherForecast>> GetAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

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
