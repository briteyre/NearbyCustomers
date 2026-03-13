namespace CoreCodeCamp.Data
{
    public class Talk
    {
        public int TalkId { get; set; }
        public required Camp Camp { get; set; }
        public required string Title { get; set; }
        public required string Abstract { get; set; }
        public int Level { get; set; }
        public required Speaker Speaker { get; set; }
    }
}