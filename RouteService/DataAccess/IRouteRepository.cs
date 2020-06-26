using MongoDB.Driver;
using RouteService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteService.DataAccess
{
    public interface IRouteRepository
    {
        Task AddAsync(Route route);
        Task<Route> GetAsync(string id, string userId);
        Task<List<GetAllRoute>> GetAllAsync(GetRoutesFilters getRoutesFilters);
        Task<DeleteResult> DeleteAsync(string id);
        Task<UpdateResult> UpdateAsync(UpdateRouteModel updateRouteModel);
    }
}
