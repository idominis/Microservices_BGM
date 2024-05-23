namespace OrderManagementService.DTO
{
    public class FetchSummariesRequestDto
    {
        public HashSet<int> AlreadyGeneratedIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
