namespace RouteService.Models
{
    public class UpdateRouteModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public bool? Like { get; set; }
        public bool? Unlike { get; set; }
    }
}
