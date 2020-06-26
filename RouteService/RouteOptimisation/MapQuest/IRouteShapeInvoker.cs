using System.Threading.Tasks;

namespace RouteService.RouteOptimisation.MapQuest
{
    public interface IRouteShapeInvoker
    {
        Task<RouteShapeResponse> MakeOptimisedRouteRequestAndWaitAsync(OptimisedRouteResponse optimisedRouteResponse);
    }
}
