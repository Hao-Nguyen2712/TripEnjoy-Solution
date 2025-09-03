using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Entities
{
    public class Account : Entity<Guid>
    {
        public string AspNetUserId { get; set; }
        public string AccountEmail { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Account(Guid id) : base(id)
        {
        }
        public Account(Guid id, string aspNetUserId, string accountEmail) : base(id)
        {
            AspNetUserId = aspNetUserId;
            AccountEmail = accountEmail;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateEmail(string newEmail)
        {
            AccountEmail = newEmail;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
