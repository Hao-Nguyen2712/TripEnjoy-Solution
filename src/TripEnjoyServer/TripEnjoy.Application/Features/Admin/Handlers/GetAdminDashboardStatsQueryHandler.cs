using MediatR;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.Enums;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, Result<AdminDashboardStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminDashboardStatsDto>> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Repository<Domain.Account.Account>().GetAllAsync();
        var properties = await _unitOfWork.Repository<Domain.Property.Property>().GetAllAsync();
        var bookings = await _unitOfWork.Repository<Domain.Booking.Booking>().GetAllAsync();
        var vouchers = await _unitOfWork.Repository<Domain.Voucher.Voucher>().GetAllAsync();
        var reviews = await _unitOfWork.Repository<Domain.Review.Review>().GetAllAsync();

        var totalUsers = accounts.Count(a => a.User != null);
        var totalPartners = accounts.Count(a => a.Partner != null);
        var totalProperties = properties.Count();
        var totalBookings = bookings.Count();
        
        // Simplified revenue calculation
        var totalRevenue = bookings
            .Where(b => b.Status == "Completed" || b.Status == "Confirmed")
            .Sum(b => b.TotalPrice);

        var pendingPartnerApprovals = accounts
            .Count(a => a.Partner != null && a.Partner.Status == PartnerStatusEnum.Pending.ToString());

        var pendingPropertyApprovals = properties
            .Count(p => p.Status == PropertyEnum.WaitingForApproval);

        var activeVouchers = vouchers
            .Count(v => v.Status == "Active" && v.EndDate >= DateTime.UtcNow);

        var totalReviews = reviews.Count();

        var now = DateTime.UtcNow;
        var startOfToday = now.Date;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        var revenueStats = new RevenueStatsDto
        {
            Today = bookings
                .Where(b => (b.Status == "Completed" || b.Status == "Confirmed") && b.CreatedAt >= startOfToday)
                .Sum(b => b.TotalPrice),
            ThisWeek = bookings
                .Where(b => (b.Status == "Completed" || b.Status == "Confirmed") && b.CreatedAt >= startOfWeek)
                .Sum(b => b.TotalPrice),
            ThisMonth = bookings
                .Where(b => (b.Status == "Completed" || b.Status == "Confirmed") && b.CreatedAt >= startOfMonth)
                .Sum(b => b.TotalPrice),
            ThisYear = bookings
                .Where(b => (b.Status == "Completed" || b.Status == "Confirmed") && b.CreatedAt >= startOfYear)
                .Sum(b => b.TotalPrice)
        };

        var dashboardStats = new AdminDashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalPartners = totalPartners,
            TotalProperties = totalProperties,
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            PendingPartnerApprovals = pendingPartnerApprovals,
            PendingPropertyApprovals = pendingPropertyApprovals,
            ActiveVouchers = activeVouchers,
            TotalReviews = totalReviews,
            RevenueStats = revenueStats,
            RecentActivities = new List<RecentActivityDto>()
        };

        return Result<AdminDashboardStatsDto>.Success(dashboardStats);
    }
}
