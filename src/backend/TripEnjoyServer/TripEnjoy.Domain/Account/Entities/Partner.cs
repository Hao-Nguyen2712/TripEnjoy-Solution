using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities
{
    public class Partner : Entity<PartnerId>
    {

        public AccountId AccountId { get; private set; }
        public string? CompanyName { get; private set; }

        public string? ContactNumber { get; private set; }

        public string? Address { get; private set; }
        public string Status { get; private set; }

        public readonly List<PartnerDocument> _partnerDocuments = new();
        public IReadOnlyList<PartnerDocument> PartnerDocuments => _partnerDocuments.AsReadOnly();

        private Partner() : base(PartnerId.CreateUnique())
        {
            AccountId = null!;
            Status = null!;
        }

        public Partner(PartnerId id, AccountId accountId, string? companyName, string? contactNumber, string? address) : base(id)
        {

            AccountId = accountId;
            CompanyName = companyName;
            ContactNumber = contactNumber;
            Address = address;
            Status = PartnerStatusEnum.Pending.ToString();
        }

        public static Result<Partner> Create(AccountId accountId, string? companyName, string? contactNumber, string? address)
        {
            var partner = new Partner(PartnerId.CreateUnique(), accountId, companyName, contactNumber, address);
            return Result<Partner>.Success(partner);
        }
    }
}   
