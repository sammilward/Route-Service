using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace RouteService.Models
{
    public class Route
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUsername { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        [BsonIgnore]
        public int Rating { get; set; }
        public List<Place> Places { get; set; }
        [BsonIgnore]
        public double[] RouteCoords { get; set; }
        public List<string> UsersLiked { get; set; } = new List<string>();
        [BsonIgnore]
        public bool UserLikes { get; set; }
    }   
}
