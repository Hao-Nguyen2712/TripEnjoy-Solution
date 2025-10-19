namespace TripEnjoy.ShareKernel.Dtos;

public class PartnerDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
}