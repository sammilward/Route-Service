using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQHelper;
using RabbitMQHelper.Models;
using RouteService.DataAccess;
using RouteService.Models;
using RouteService.RabbitMQ.Producer;
using RouteService.RabbitMQ.Requests;
using RouteService.RabbitMQ.Responses;
using System.Threading.Tasks;

namespace RouteService.RabbitMQ.Handlers
{
    public class DeleteRouteRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "DeleteRoute";

        private readonly ILogger<DeleteRouteRabbitHandler> _logger;
        private readonly IRouteRepository _routeRepository;


        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("DeleteRouteRabbitMessagesRecieved", "Number of rabbit messages recieved to delete route handler");
        private readonly Counter successfullyDeletedRoutesCounter = Metrics.CreateCounter("successfullyDeletedRoutes", "Number of successfully deleted routes");
        private readonly Counter unsucccessfulDeletedRoutesCounter = Metrics.CreateCounter("unsucccessfulDeletedRoutes", "Number of unsuccessfull deleted routes");

        public DeleteRouteRabbitHandler(ILogger<DeleteRouteRabbitHandler> logger, IRouteRepository routeRepository)
        {
            _logger = logger;
            _routeRepository = routeRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(DeleteRouteRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<DeleteRouteRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(DeleteRouteRabbitRequest deleteRouteRabbitRequest)
        {
            var route = await _routeRepository.GetAsync(deleteRouteRabbitRequest.Id, deleteRouteRabbitRequest.UserId);

            var deleteRouteRabbitResponse = new DeleteRouteRabbitResponse();

            if (route != null)
            {
                if (deleteRouteRabbitRequest.UserId == route.CreatorId)
                {
                    await _routeRepository.DeleteAsync(route.Id);
                    successfullyDeletedRoutesCounter.Inc();
                    deleteRouteRabbitResponse.Successful = true;
                }
                else deleteRouteRabbitResponse.Successful = false;
                unsucccessfulDeletedRoutesCounter.Inc();
            }
            else
            {
                deleteRouteRabbitResponse.Successful = false;
                unsucccessfulDeletedRoutesCounter.Inc();
            }

            return deleteRouteRabbitResponse;
        }
    }
}
