namespace CMCSApplication.Models.ViewModels
{
    public class LecturerDashboardVM
    {
        public Lecturer? Lecturer { get; set; }
        public List<Claim> RecentClaims { get; set; } = new();
    }
}
