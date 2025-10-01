namespace TripEnjoy.ShareKernel.Constant;

public static class DocumentTypeConstant
{
    public const string BusinessLicense = "BusinessLicense";
    public const string TaxIdentification = "TaxIdentification";
    public const string ProofOfAddress = "ProofOfAddress";
    public const string CompanyRegistration = "CompanyRegistration";
    public const string BankStatement = "BankStatement";
    public const string IdentityDocument = "IdentityDocument";

    public static readonly string[] ValidDocumentTypes = new[]
    {
        BusinessLicense,
        TaxIdentification,
        ProofOfAddress,
        CompanyRegistration,
        BankStatement,
        IdentityDocument
    };

    public static bool IsValidDocumentType(string documentType)
    {
        return ValidDocumentTypes.Contains(documentType, StringComparer.OrdinalIgnoreCase);
    }
}