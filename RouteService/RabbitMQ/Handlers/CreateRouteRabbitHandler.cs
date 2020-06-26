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
using System;
using System.Threading.Tasks;

namespace RouteService.RabbitMQ.Handlers
{
    public class CreateRouteRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "CreateRoute";

        private const string GetUserMethod = "GetUser";

        private readonly ILogger<CreateRouteRabbitHandler> _logger;
        private readonly IRouteServiceRabbitRPCService _routeServiceRabbitRPCService;
        private readonly IRouteRepository _routeRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("CreateRouteRabbitMessagesRecieved", "Number of rabbit messages recieved to create routes handler");
        private readonly Counter successfullyCreatedRoutesCounter = Metrics.CreateCounter("successfullyCreatedRoutes", "Number of successfully created routes");
        private readonly Counter unsucccessfulCreatedRoutesCounter = Metrics.CreateCounter("unsucccessfulCreatedRoutes", "Number of unsuccessfull created routes");

        public CreateRouteRabbitHandler(ILogger<CreateRouteRabbitHandler> logger, IRouteServiceRabbitRPCService routeServiceRabbitRPCService, IRouteRepository routeRepository)
        {
            _logger = logger;
            _routeServiceRabbitRPCService = routeServiceRabbitRPCService;
            _routeRepository = routeRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<CreateRouteRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(CreateRouteRabbitRequest createRouteRabbitRequest)
        {
            _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Sending request to UserService for method {GetUserMethod}.");

            var getUserRabbitResponse = await _routeServiceRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetUserRabbitResponse>(GetUserMethod, new GetUserRabbitRequest() { Id = createRouteRabbitRequest.UserId });

            var createRouteRabbitResponse = new CreateRouteRabbitResponse();

            if (getUserRabbitResponse.FoundUser)
            {
                _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Retrieved user {getUserRabbitResponse.User.Id}.");

                var user = getUserRabbitResponse.User;

                var route = new Route()
                {
                    Name = createRouteRabbitRequest.RouteName,
                    CreatorId = user.Id,
                    CreatorUsername = user.Username,
                    City = user.CurrentCity,
                    Country = user.CurrentCountry,
                    Places = createRouteRabbitRequest.Places
                };

                try
                {
                    await _routeRepository.AddAsync(route);
                    _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: New route created {route.Id}.");
                    successfullyCreatedRoutesCounter.Inc();
                    createRouteRabbitResponse.Successful = true;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"{nameof(CreateRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Route creation failed. {ex.Message}.");
                    unsucccessfulCreatedRoutesCounter.Inc();
                    createRouteRabbitResponse.Successful = false;
                }
            }
            return createRouteRabbitResponse;
        }
    }
}
