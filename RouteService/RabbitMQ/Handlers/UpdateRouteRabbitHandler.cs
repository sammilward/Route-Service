using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQHelper;
using RabbitMQHelper.Models;
using RouteService.DataAccess;
using RouteService.Models;
using RouteService.RabbitMQ.Requests;
using RouteService.RabbitMQ.Responses;
using System.Threading.Tasks;

namespace RouteService.RabbitMQ.Handlers
{
    public class UpdateRouteRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "UpdateRoute";

        private readonly ILogger<UpdateRouteRabbitHandler> _logger;
        private readonly IRouteRepository _routeRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("UpdateRouteRabbitMessagesRecieved", "Number of rabbit messages recieved to update route handler");
        private readonly Counter successfullyUpdateRoutesRequestsCounter = Metrics.CreateCounter("successfullyUpdateRoutes", "Number of successfully update routes request");
        private readonly Counter unsucccessfulUpdateRoutesRequestsCounter = Metrics.CreateCounter("unsucccessfulUpdateRoutes", "Number of unsuccessful update routes requests");

        public UpdateRouteRabbitHandler(ILogger<UpdateRouteRabbitHandler> logger, IRouteRepository routeRepository)
        {
            _logger = logger;
            _routeRepository = routeRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<UpdateRouteRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(UpdateRouteRabbitRequest updateRouteRabbitRequest)
        {
            var route = await _routeRepository.GetAsync(updateRouteRabbitRequest.Id, updateRouteRabbitRequest.UserId);

            var updateRouteRabbitResponse = new UpdateRouteRabbitResponse();

            if (route != null)
            {
                _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Found route with id: {route.Id}.");

                if (route.CreatorId != updateRouteRabbitRequest.UserId)
                {
                    if (((updateRouteRabbitRequest.Like.HasValue && updateRouteRabbitRequest.Like.Value) && route.UserLikes == false) || (updateRouteRabbitRequest.Unlike.HasValue && updateRouteRabbitRequest.Unlike.Value) && route.UserLikes == true)
                    {
                        _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Updating route: {route.Id}.");

                        var updateRouteModel = new UpdateRouteModel()
                        {
                            Id = updateRouteRabbitRequest.Id,
                            UserId = updateRouteRabbitRequest.UserId,
                            Like = updateRouteRabbitRequest.Like,
                            Unlike = updateRouteRabbitRequest.Unlike
                        };

                        await _routeRepository.UpdateAsync(updateRouteModel);

                        updateRouteRabbitResponse.Successful = true;

                        _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Updating route successful.");
                        successfullyUpdateRoutesRequestsCounter.Inc();

                        return updateRouteRabbitResponse;
                    }
                    else _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Invalid action, user already liked/unliked this route.");
                }
                else _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Invalid action, user can not like/unlike their own route.");
            }
            else _logger.LogInformation($"{nameof(UpdateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Invalid action, no route found with id: {updateRouteRabbitRequest.Id}");

            unsucccessfulUpdateRoutesRequestsCounter.Inc();

            return updateRouteRabbitResponse;
        }
    }
}
