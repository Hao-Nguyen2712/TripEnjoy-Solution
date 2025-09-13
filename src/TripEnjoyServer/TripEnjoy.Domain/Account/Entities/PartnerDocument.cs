using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities
{
    public class PartnerDocument : Entity<PartnerDocumentId>
    {
        public PartnerId PartnerId { get; private set; }
        public string DocumentType { get; private set; }
        public string DocumentUrl { get; private set; }

        public string Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ReviewedAt { get; private set; }

        private PartnerDocument() : base(PartnerDocumentId.CreateUnique())
        {
            PartnerId = null!;
            DocumentType = null!;
            DocumentUrl = null!;
            Status = null!;
        }
        public PartnerDocument(PartnerDocumentId id, PartnerId partnerId, string documentType, string documentUrl, string status) : base(id)
        {
            PartnerId = partnerId;
            DocumentType = documentType;
            DocumentUrl = documentUrl;
            Status = status;
            CreatedAt = DateTime.UtcNow;
            ReviewedAt = null;
        }
        
        public void MarkAsReviewed()
        {
            ReviewedAt = DateTime.UtcNow;
        }
    }
}