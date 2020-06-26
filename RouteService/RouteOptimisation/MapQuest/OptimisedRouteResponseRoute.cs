namespace RouteService.RouteOptimisation.MapQuest
{
    public class OptimisedRouteResponseRoute
    {
        public double distance { get; set; }
        public string sessionId { get; set; }
        public int[] locationSequence { get; set; }
        public int time { get; set; }
    }
}
