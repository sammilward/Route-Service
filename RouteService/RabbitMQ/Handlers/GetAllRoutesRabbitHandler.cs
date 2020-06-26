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
    public class GetAllRoutesRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "GetAllRoutes";

        private const string GetUserMethod = "GetUser";

        private readonly ILogger<GetAllRoutesRabbitHandler> _logger;
        private readonly IRouteServiceRabbitRPCService _routeServiceRabbitRPCService;
        private readonly IRouteRepository _routeRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("GetAllRoutesRabbitMessagesRecieved", "Number of rabbit messages recieved to GetAll routes handler");
        private readonly Counter successfullyGetAllRoutesRequestsCounter = Metrics.CreateCounter("successfullyGetAllRoutes", "Number of successfully GetAll routes request");

        public GetAllRoutesRabbitHandler(ILogger<GetAllRoutesRabbitHandler> logger, IRouteServiceRabbitRPCService routeServiceRabbitRPCService, IRouteRepository routeRepository)
        {
            _logger = logger;
            _routeServiceRabbitRPCService = routeServiceRabbitRPCService;
            _routeRepository = routeRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(GetAllRoutesRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<GetAllRoutesRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(GetAllRoutesRabbitRequest getAllRoutesRabbitRequest)
        {
            _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Sending request to UserService for method {GetUserMethod}.");

            var getUserRabbitResponse = await _routeServiceRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetUserRabbitResponse>(GetUserMethod, new GetUserRabbitRequest() { Id = getAllRoutesRabbitRequest.UserId });

            var getAllRoutesRabbitResponse = new GetAllRoutesRabbitResponse();

            if (getUserRabbitResponse.FoundUser)
            {
                _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: User found with id: {getUserRabbitResponse.User.Id}.");

                var getRoutesFilters = new GetRoutesFilters()
                {
                    UserId = getUserRabbitResponse.User.Id,
                    Country = getUserRabbitResponse.User.CurrentCountry,
                    City = getUserRabbitResponse.User.CurrentCity,
                    FriendId = getAllRoutesRabbitRequest.FriendId,
                    Popular = getAllRoutesRabbitRequest.Popular
                };

                var routes = await _routeRepository.GetAllAsync(getRoutesFilters);

                if (routes.Count != 0)
                {
                    _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Routes found.");

                    getAllRoutesRabbitResponse.FoundRoutes = true;
                    getAllRoutesRabbitResponse.Routes = routes;
                }
                else
                {
                    _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: No routes found.");

                    getAllRoutesRabbitResponse.FoundRoutes = false;
                }
            }
            else
            {
                _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: User not found.");

                getAllRoutesRabbitResponse.FoundRoutes = false;
            }

            successfullyGetAllRoutesRequestsCounter.Inc();

            return getAllRoutesRabbitResponse;
        }
    }
}
