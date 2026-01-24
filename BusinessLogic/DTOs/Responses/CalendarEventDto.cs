namespace BusinessLogic.DTOs.Responses
{
    public class CalendarEventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public Dictionary<string, object> ExtendedProps { get; set; } = new();
    }
}

