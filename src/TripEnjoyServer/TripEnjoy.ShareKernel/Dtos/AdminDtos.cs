namespace TripEnjoy.ShareKernel.Dtos;

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalPartners { get; set; }
    public int TotalProperties { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingPartnerApprovals { get; set; }
    public int PendingPropertyApprovals { get; set; }
    public int ActiveVouchers { get; set; }
    public int TotalReviews { get; set; }
    public RevenueStatsDto RevenueStats { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RevenueStatsDto
{
    public decimal Today { get; set; }
    public decimal ThisWeek { get; set; }
    public decimal ThisMonth { get; set; }
    public decimal ThisYear { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class UserManagementDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class PartnerApprovalDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public List<PartnerDocumentDto> Documents { get; set; } = new();
}

public class PropertyApprovalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartnerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PropertyTypeName { get; set; } = string.Empty;
}

public class SystemSettingsDto
{
    public decimal PlatformCommissionRate { get; set; }
    public decimal MinimumBookingAmount { get; set; }
    public int MaxBookingDaysAdvance { get; set; }
    public int AutoCancelHours { get; set; }
    public bool MaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = string.Empty;
}
