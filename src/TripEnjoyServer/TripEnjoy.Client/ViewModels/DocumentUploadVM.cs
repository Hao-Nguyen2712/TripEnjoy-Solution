using System.ComponentModel.DataAnnotations;

namespace TripEnjoy.Client.ViewModels
{
    public class UploadDocumentVM
    {
        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Document File")]
        public IFormFile DocumentFile { get; set; } = null!;

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public class DocumentUploadUrlVM
    {
        public string UploadUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public Dictionary<string, string> UploadParameters { get; set; } = new();
        public long Timestamp { get; set; }
        public string Signature { get; set; } = string.Empty;
    }

    public class ConfirmDocumentUploadVM
    {
        public string DocumentType { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }

    public class DocumentStatusVM
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}