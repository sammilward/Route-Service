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
using RouteService.RouteOptimisation.MapQuest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteService.RabbitMQ.Handlers
{
    public class GetRouteRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "GetRoute";

        private const string GetUserMethod = "GetUser";

        private readonly ILogger<GetRouteRabbitHandler> _logger;
        private readonly IRouteRepository _routeRepository;
        private readonly IRouteServiceRabbitRPCService _routeServiceRabbitRPCService;
        private readonly IOptimiseRouteInvoker _optimiseRouteInvoker;
        private readonly IRouteShapeInvoker _routeShapeInvoker;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("GetRouteRabbitMessagesRecieved", "Number of rabbit messages recieved to Get route handler");
        private readonly Counter successfullyGetRouteRequestsCounter = Metrics.CreateCounter("successfullyGetRoutes", "Number of successfully Get routes request");
        private readonly Counter unsucccessfulGetRouteRequestsCounter = Metrics.CreateCounter("unsucccessfulGetRoutes", "Number of unsuccessfull Get routes requests");

        public GetRouteRabbitHandler(ILogger<GetRouteRabbitHandler> logger, 
                                     IRouteRepository routeRepository, 
                                     IRouteServiceRabbitRPCService routeServiceRabbitRPCService,
                                     IOptimiseRouteInvoker optimiseRouteInvoker,
                                     IRouteShapeInvoker routeShapeInvoker)
        {
            _logger = logger;
            _routeRepository = routeRepository;
            _routeServiceRabbitRPCService = routeServiceRabbitRPCService;
            _optimiseRouteInvoker = optimiseRouteInvoker;
            _routeShapeInvoker = routeShapeInvoker;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(GetRouteRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<GetRouteRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(GetRouteRabbitRequest getRouteRabbitRequest)
        {
            _logger.LogInformation($"{nameof(GetRouteRabbitHandler)}.{nameof(HandleMessageAsync)}: Sending request to UserService for method {GetUserMethod}.");

            var getUserRabbitResponse = await _routeServiceRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetUserRabbitResponse>(GetUserMethod, new GetUserRabbitRequest() { Id = getRouteRabbitRequest.UserId });

            var getAllRoutesRabbitResponse = new GetRouteRabbitResponse();

            var route = await _routeRepository.GetAsync(getRouteRabbitRequest.Id, getRouteRabbitRequest.UserId);

            if (route != null)
            {
                getAllRoutesRabbitResponse.FoundRoute = true;

                var optimisedRouteResponse = await _optimiseRouteInvoker.MakeOptimisedRouteRequestAndWaitAsync(route, getUserRabbitResponse.User);

                optimisedRouteResponse.route.locationSequence = optimisedRouteResponse.route.locationSequence.Skip(1).ToArray();
                var orderedPlaces = new List<Place>();
                for (int i = 0; i < optimisedRouteResponse.route.locationSequence.Length; i++)
                {
                    orderedPlaces.Add(route.Places[optimisedRouteResponse.route.locationSequence[i]-1]);
                }
                route.Places = orderedPlaces;

                var shapeResponse = await _routeShapeInvoker.MakeOptimisedRouteRequestAndWaitAsync(optimisedRouteResponse);

                route.RouteCoords = shapeResponse.route.shape.shapePoints;

                getAllRoutesRabbitResponse.Route = route;

                successfullyGetRouteRequestsCounter.Inc();
            }
            else
            {
                getAllRoutesRabbitResponse.FoundRoute = false;

                unsucccessfulGetRouteRequestsCounter.Inc();
            }

            return getAllRoutesRabbitResponse;
        }
    }
}
