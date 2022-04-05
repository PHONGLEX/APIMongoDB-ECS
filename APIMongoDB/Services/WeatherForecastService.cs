using APIMongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIMongoDB.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IMongoCollection<WeatherForecast> _weatherCollection;
        private readonly Serilog.ILogger _logger;

        public WeatherForecastService(
            IOptions<WeatherForecastDatabaseConnectionSettings> weatherForecastDatabaseSettings, Serilog.ILogger logger)
        {
            _logger = logger;
            //var mongoClient = new MongoClient(weatherForecastDatabaseSettings.Value.ConnectionString);

            //var mongoDatabase = mongoClient.GetDatabase(
            //    weatherForecastDatabaseSettings.Value.DatabaseName);

            //_weatherCollection = mongoDatabase.GetCollection<WeatherForecast>(
            //    weatherForecastDatabaseSettings.Value.WeatherCollectionName);
            var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("CONNECTION_STRING"));

            var mongoDatabase = mongoClient.GetDatabase(Environment.GetEnvironmentVariable("DATABASE_NAME"));

            _weatherCollection = mongoDatabase.GetCollection<WeatherForecast>(Environment.GetEnvironmentVariable("COLLECTION_NAME"));
        }

        public async Task<List<WeatherForecast>> GetAsync()
        {
            try
            {
                _logger.Information("Get all");
                return await _weatherCollection.Find(_ => true).ToListAsync();
            }
            catch (System.Exception e)
            {
                _logger.Error(e.Message);
                throw;
            }
        }

        public async Task<WeatherForecast> GetAsync(string id)
        {
            try
            {
                _logger.Information("Get by id");
                return await _weatherCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (System.Exception e)
            {
                _logger.Information(e.Message);
                throw;
            }
        }

        public async Task CreateAsync(WeatherForecast newWeatherForecast)
        {
            try
            {
                _logger.Information("Create");
                await _weatherCollection.InsertOneAsync(newWeatherForecast);
            }
            catch (System.Exception e)
            {
                _logger.Information(e.Message);
                throw;
            }

        }

        public async Task UpdateAsync(string id, WeatherForecast updatedWeatherForecast)
        {
            try
            {
                await _weatherCollection.ReplaceOneAsync(x => x.Id == id, updatedWeatherForecast);
            }
            catch (System.Exception e)
            {
                _logger.Information(e.Message);
                throw;
            }        
        }

        public async Task RemoveAsync(string id) 
        {
            try
            {
                await _weatherCollection.DeleteOneAsync(x => x.Id == id);

            }
            catch (System.Exception e)
            {
                _logger.Information(e.Message);
                throw;
            }        
        }
    }
}
