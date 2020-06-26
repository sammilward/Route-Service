using RouteService.Models;

namespace RouteService.RabbitMQ.Responses
{
    public class GetUserRabbitResponse
    {
        public bool FoundUser { get; set; } = true;
        public User User { get; set; }
    }
}
