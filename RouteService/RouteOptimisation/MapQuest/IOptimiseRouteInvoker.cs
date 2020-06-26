using RouteService.Models;
using System.Threading.Tasks;

namespace RouteService.RouteOptimisation.MapQuest
{
    public interface IOptimiseRouteInvoker
    {
        Task<OptimisedRouteResponse> MakeOptimisedRouteRequestAndWaitAsync(Route route, User user);
    }
}
