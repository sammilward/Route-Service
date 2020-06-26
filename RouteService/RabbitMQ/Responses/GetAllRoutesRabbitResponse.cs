using RouteService.Models;
using System.Collections.Generic;

namespace RouteService.RabbitMQ.Responses
{
    public class GetAllRoutesRabbitResponse
    {
        public bool FoundRoutes { get; set; }
        public List<GetAllRoute> Routes { get; set; }
    }
}
