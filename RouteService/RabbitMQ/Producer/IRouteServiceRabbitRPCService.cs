using System.Threading.Tasks;

namespace RouteService.RabbitMQ.Producer
{
    public interface IRouteServiceRabbitRPCService
    {
        Task<T> PublishRabbitMessageWaitForResponseAsync<T>(string method, object requestModel);
    }
}
