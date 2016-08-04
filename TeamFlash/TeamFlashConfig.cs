namespace TeamFlash
{
    public class TeamFlashConfig
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Verbose { get; set; }
        public string BuildTypeIds { get; set; }
        public string BuildTypeIdsExcluded { get; set; }
        public int IntervalSeconds { get; set; }
    }
}
