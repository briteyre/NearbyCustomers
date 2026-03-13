namespace CoreCodeCamp.Data
{
    public class Location
    {
        public int LocationId { get; set; }
        public required string VenueName { get; set; }
        public required string Address1 { get; set; }
        public required string Address2 { get; set; }
        public required string Address3 { get; set; }
        public required string CityTown { get; set; }
        public required string StateProvince { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
    }
}