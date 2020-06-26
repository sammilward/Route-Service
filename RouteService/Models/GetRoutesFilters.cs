namespace RouteService.Models
{
    public class GetRoutesFilters
    {
        public string UserId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string FriendId { get; set; }
        public bool? Popular { get; set; }
    }
}
