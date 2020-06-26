namespace RouteService.RabbitMQ.Requests
{
    public class UpdateRouteRabbitRequest
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public bool? Like { get; set; }
        public bool? Unlike { get; set; }
    }
}
