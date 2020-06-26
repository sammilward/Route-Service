using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RouteService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteService.DataAccess
{
    public class MongoRouteRepository : IRouteRepository
    {
        private readonly ILogger<MongoRouteRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Route> _collection;

        public MongoRouteRepository(ILogger<MongoRouteRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _logger.LogInformation($"{nameof(MongoRouteRepository)}: Making connection to mongo server using: {configuration.GetSection("MongoConnection").Value}");
            var client = new MongoClient(configuration.GetSection("MongoConnection").Value);
            _logger.LogInformation($"{nameof(MongoRouteRepository)}: Fetching database: {configuration.GetSection("MongoDatabaseName").Value}");
            var database = client.GetDatabase(configuration.GetSection("MongoDatabaseName").Value);
            _logger.LogInformation($"{nameof(MongoRouteRepository)}: Fetching collection: {configuration.GetSection("MongoCollectionName").Value}");
            _collection = database.GetCollection<Route>(configuration.GetSection("MongoCollectionName").Value);
        }

        public async Task AddAsync(Route route)
        {
            _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(AddAsync)}: Adding new route.");
             await _collection.InsertOneAsync(route);
        }

        public async Task<Route> GetAsync(string id, string userId)
        {
            var filter = Builders<Route>.Filter.Eq(nameof(Route.Id), id);
            var route = await _collection.Find(filter).SingleOrDefaultAsync();

            _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(GetAsync)}: Found route with id: {route.Id}.");

            if (route.UsersLiked.Contains(userId)) route.UserLikes = true;
            else route.UserLikes = false;

            route.Rating = route.UsersLiked.Count();
            return route;
        }

        public async Task<List<GetAllRoute>> GetAllAsync(GetRoutesFilters getRoutesFilters)
        {
            IAsyncCursor<Route> routes;
            FilterDefinition<Route> filter;

            if (getRoutesFilters.Popular.GetValueOrDefault(false))
            {
                _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(GetAllAsync)}: Getting popular routes for user: {getRoutesFilters.UserId}");
                filter = Builders<Route>.Filter.Eq(nameof(Route.City), getRoutesFilters.City);
            }
            else if (!string.IsNullOrEmpty(getRoutesFilters.FriendId))
            {
                _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(GetAllAsync)}: Getting friends routes for user: {getRoutesFilters.UserId} for friend {getRoutesFilters.FriendId}");
                filter = Builders<Route>.Filter.Eq(nameof(Route.CreatorId), getRoutesFilters.FriendId);
            }
            else
            {
                _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(GetAllAsync)}: Getting users routes for user: {getRoutesFilters.UserId}");
                filter = Builders<Route>.Filter.Eq(nameof(Route.CreatorId), getRoutesFilters.UserId);
            }

            routes = await _collection.FindAsync(filter);

            _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(GetAllAsync)}: Converting routes to GetAllRoutes");

            var getAllRoutes = new List<GetAllRoute>();
            foreach (var route in routes.ToList())
            {
                var getAllPlaces = new List<GetAllPlace>();

                foreach (var place in route.Places)
                {
                    getAllPlaces.Add(new GetAllPlace()
                    {
                        PlaceId = place.PlaceId,
                        Name = place.Name,
                        Latitude = place.Latitude,
                        Longitude = place.Longitude,
                        Rating = place.Rating,
                        NumberOfRatings = place.NumberOfRatings,
                        PhotoReference = place.PhotoReference,
                        Types = place.Types
                    });
                }

                getAllRoutes.Add(new GetAllRoute()
                {
                    Id = route.Id,
                    Name = route.Name,
                    City = route.City,
                    Country = route.Country,
                    CreatorId = route.CreatorId,
                    CreatorUsername = route.CreatorUsername,
                    Rating = route.UsersLiked.Count,
                    Places = getAllPlaces
                });
            }

            if (getRoutesFilters.Popular.GetValueOrDefault(false)) getAllRoutes =
                getAllRoutes.OrderByDescending(x => x.Rating).Take(20).ToList();

            return getAllRoutes;
        }

        public async Task<DeleteResult> DeleteAsync(string id)
        {
            _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(DeleteAsync)}: Deleting route with id: {id}");
            var filter = Builders<Route>.Filter.Eq(nameof(Route.Id), id);
            return await _collection.DeleteOneAsync(filter);
        }

        public async Task<UpdateResult> UpdateAsync(UpdateRouteModel updateRouteModel)
        {
            _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(UpdateAsync)}: Updating route with id: {updateRouteModel.Id}");

            UpdateDefinition<Route> updateDefinition = null;

            var route = await GetAsync(updateRouteModel.Id, updateRouteModel.UserId);

            if (updateRouteModel.Like.HasValue && updateRouteModel.Like.Value)
            {
                _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(UpdateAsync)}: User {updateRouteModel.UserId} liking route {updateRouteModel.Id}");

                if (!route.UsersLiked.Contains(updateRouteModel.UserId))
                {
                    var usersLiked = route.UsersLiked;
                    usersLiked.Add(updateRouteModel.UserId);
                    updateDefinition = Builders<Route>.Update.Set(nameof(Route.UsersLiked), usersLiked);
                }
            }
            else if (updateRouteModel.Unlike.HasValue && updateRouteModel.Unlike.Value)
            {
                _logger.LogInformation($"{nameof(MongoRouteRepository)}.{nameof(UpdateAsync)}: User {updateRouteModel.UserId} unliking route {updateRouteModel.Id}");
                
                if (route.UsersLiked.Contains(updateRouteModel.UserId))
                {
                    var usersLiked = route.UsersLiked;
                    usersLiked.Remove(updateRouteModel.UserId);
                    updateDefinition = Builders<Route>.Update.Set(nameof(Route.UsersLiked), usersLiked);
                }
            }

            var filter = Builders<Route>.Filter.Eq(nameof(Route.Id), updateRouteModel.Id);
            return await _collection.UpdateOneAsync(filter, updateDefinition);
        }
    }
}
