namespace RouteService.RabbitMQ.Requests
{
    public class GetAllRoutesRabbitRequest
    {
        public string UserId { get; set; }
        public string FriendId { get; set; }
        public bool? Popular { get; set; }
    }
}
