using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
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
            // Business rule: Company name is required for partners
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return Result<Partner>.Failure(DomainError.Partner.CompanyNameRequired);
            }

            // Business rule: Company name must be at least 2 characters
            if (companyName.Length < 2)
            {
                return Result<Partner>.Failure(new Error("Partner.CompanyNameTooShort", "Company name must be at least 2 characters long.", ErrorType.Validation));
            }

            var partner = new Partner(PartnerId.CreateUnique(), accountId, companyName, contactNumber, address);
            return Result<Partner>.Success(partner);
        }

        public Result AddDocument(string documentType, string documentUrl)
        {
            // Optional: Add validation to prevent duplicate document types if needed
            if (_partnerDocuments.Any(d => d.DocumentType.Equals(documentType, StringComparison.OrdinalIgnoreCase)))
            {
                // Decide if this should be an error or if you should replace the existing one.
                // For now, let's treat it as a failure.
                return Result.Failure(new Error("Partner.DuplicateDocumentType", "A document of this type has already been uploaded.", ErrorType.Conflict));
            }

            var newDocument = new PartnerDocument(
                PartnerDocumentId.CreateUnique(),
                Id,
                documentType,
                documentUrl,
                PartnerDocumentStatusEnum.PendingReview.ToString());

            _partnerDocuments.Add(newDocument);
            return Result.Success();
        }

        public Result Approve()
        {
            if (Status == PartnerStatusEnum.Approved.ToString())
            {
                return Result.Failure(new Error("Partner.AlreadyApproved", "Partner is already approved.", ErrorType.Conflict));
            }

            Status = PartnerStatusEnum.Approved.ToString();
            return Result.Success();
        }

        public Result Reject()
        {
            if (Status == PartnerStatusEnum.Rejected.ToString())
            {
                return Result.Failure(new Error("Partner.AlreadyRejected", "Partner is already rejected.", ErrorType.Conflict));
            }

            Status = PartnerStatusEnum.Rejected.ToString();
            return Result.Success();
        }

        public Result Suspend()
        {
            if (Status != PartnerStatusEnum.Approved.ToString())
            {
                return Result.Failure(new Error("Partner.CannotSuspend", "Only approved partners can be suspended.", ErrorType.Validation));
            }

            Status = PartnerStatusEnum.Suspended.ToString();
            return Result.Success();
        }
    }
}
