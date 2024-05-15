namespace RateLimitApi
{
    public class RequestTracker() {
        public short Count { get; set; }
        public TimeSpan Time { get; set; }
    }
}
