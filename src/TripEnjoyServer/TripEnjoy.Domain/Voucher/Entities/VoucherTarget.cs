using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;
using TripEnjoy.Domain.Voucher.Enums;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Domain.Voucher.Entities;

public class VoucherTarget : Entity<VoucherTargetId>
{
    public VoucherId VoucherId { get; private set; }
    public string TargetType { get; private set; }
    public AccountId? PartnerId { get; private set; }
    public PropertyId? PropertyId { get; private set; }
    public RoomTypeId? RoomTypeId { get; private set; }

    // Navigation property
    public Voucher Voucher { get; private set; }

    private VoucherTarget() : base(VoucherTargetId.CreateUnique())
    {
        VoucherId = null!;
        TargetType = null!;
        Voucher = null!;
    }

    public VoucherTarget(
        VoucherTargetId id,
        VoucherId voucherId,
        VoucherTargetTypeEnum targetType,
        AccountId? partnerId = null,
        PropertyId? propertyId = null,
        RoomTypeId? roomTypeId = null) : base(id)
    {
        VoucherId = voucherId;
        TargetType = targetType.ToString();
        PartnerId = partnerId;
        PropertyId = propertyId;
        RoomTypeId = roomTypeId;
    }

    public static VoucherTarget CreateGlobal(VoucherId voucherId)
    {
        return new VoucherTarget(
            VoucherTargetId.CreateUnique(),
            voucherId,
            VoucherTargetTypeEnum.Global);
    }

    public static VoucherTarget CreateForPartner(VoucherId voucherId, AccountId partnerId)
    {
        return new VoucherTarget(
            VoucherTargetId.CreateUnique(),
            voucherId,
            VoucherTargetTypeEnum.Partner,
            partnerId: partnerId);
    }

    public static VoucherTarget CreateForProperty(VoucherId voucherId, PropertyId propertyId)
    {
        return new VoucherTarget(
            VoucherTargetId.CreateUnique(),
            voucherId,
            VoucherTargetTypeEnum.Property,
            propertyId: propertyId);
    }

    public static VoucherTarget CreateForRoomType(VoucherId voucherId, RoomTypeId roomTypeId)
    {
        return new VoucherTarget(
            VoucherTargetId.CreateUnique(),
            voucherId,
            VoucherTargetTypeEnum.RoomType,
            roomTypeId: roomTypeId);
    }
}
