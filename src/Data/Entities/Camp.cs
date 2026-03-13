namespace CoreCodeCamp.Data
{
    public class Camp
    {
        public int CampId { get; set; }
        public required string Name { get; set; }
        public required string City { get; set; }
        public Location Location { get; set; }
        public DateTime EventDate { get; set; } = DateTime.MinValue;
        public int Length { get; set; } = 1;
        public ICollection<Talk>? Talks { get; set; }
    }
}