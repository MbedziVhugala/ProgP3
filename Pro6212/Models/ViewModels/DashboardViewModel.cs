namespace Prog6212.ViewModels

{
    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public List<ClaimSummaryViewModel> RecentClaims { get; set; } = new List<ClaimSummaryViewModel>();
        public DashboardStatsViewModel Stats { get; set; }
        public List<ClaimSummaryViewModel> PendingApprovals { get; set; } = new List<ClaimSummaryViewModel>();
    }

    public class ClaimSummaryViewModel
    {
        public int Id { get; set; }
        public string LecturerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public bool HasDocument { get; set; }
    }

    public class DashboardStatsViewModel
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmountApproved { get; set; }
    }
}