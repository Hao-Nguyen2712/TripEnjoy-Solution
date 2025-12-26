using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var accounts = _unitOfWork.Repository<Domain.Account.Account>().GetQueryable()
            .Include(a => a.User)
            .Include(a => a.Partner);
        
        var properties = _unitOfWork.Repository<Domain.Property.Property>().GetQueryable();
        var bookings = _unitOfWork.Repository<Domain.Booking.Booking>().GetQueryable()
            .Include(b => b.Payment);
        var vouchers = _unitOfWork.Repository<Domain.Voucher.Voucher>().GetQueryable();
        var reviews = _unitOfWork.Repository<Domain.Review.Review>().GetQueryable();

        var totalUsers = await accounts.Where(a => a.User != null).CountAsync(cancellationToken);
        var totalPartners = await accounts.Where(a => a.Partner != null).CountAsync(cancellationToken);
        var totalProperties = await properties.CountAsync(cancellationToken);
        var totalBookings = await bookings.CountAsync(cancellationToken);
        
        var totalRevenue = await bookings
            .Where(b => b.Payment != null && b.Payment.PaymentStatus == "Completed")
            .SumAsync(b => b.TotalAmount, cancellationToken);

        var pendingPartnerApprovals = await accounts
            .Where(a => a.Partner != null && a.Partner.Status == PartnerStatusEnum.Pending.ToString())
            .CountAsync(cancellationToken);

        var pendingPropertyApprovals = await properties
            .Where(p => p.Status == PropertyEnum.WaitingForApproval)
            .CountAsync(cancellationToken);

        var activeVouchers = await vouchers
            .Where(v => v.Status == "Active" && v.EndDate >= DateTime.UtcNow)
            .CountAsync(cancellationToken);

        var totalReviews = await reviews.CountAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var startOfToday = now.Date;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        var revenueStats = new RevenueStatsDto
        {
            Today = await bookings
                .Where(b => b.Payment != null && b.Payment.PaymentStatus == "Completed" && b.BookingDate >= startOfToday)
                .SumAsync(b => b.TotalAmount, cancellationToken),
            ThisWeek = await bookings
                .Where(b => b.Payment != null && b.Payment.PaymentStatus == "Completed" && b.BookingDate >= startOfWeek)
                .SumAsync(b => b.TotalAmount, cancellationToken),
            ThisMonth = await bookings
                .Where(b => b.Payment != null && b.Payment.PaymentStatus == "Completed" && b.BookingDate >= startOfMonth)
                .SumAsync(b => b.TotalAmount, cancellationToken),
            ThisYear = await bookings
                .Where(b => b.Payment != null && b.Payment.PaymentStatus == "Completed" && b.BookingDate >= startOfYear)
                .SumAsync(b => b.TotalAmount, cancellationToken)
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
