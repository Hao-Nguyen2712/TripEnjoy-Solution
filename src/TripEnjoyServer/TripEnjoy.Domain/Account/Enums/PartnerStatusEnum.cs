namespace TripEnjoy.Domain.Account.Enums
{
    public enum PartnerStatusEnum
    {
       /// <summary>
        /// The partner has registered and submitted documents, awaiting admin approval.
        /// </summary>
        Pending,

        /// <summary>
        /// The partner has been approved by an admin and can fully operate.
        /// </summary>
        Approved,

        /// <summary>
        /// The partner's application was rejected by an admin.
        /// </summary>
        Rejected,

        /// <summary>
        /// The partner's operating privileges have been temporarily revoked.
        /// </summary>
        Suspended
    }
}
