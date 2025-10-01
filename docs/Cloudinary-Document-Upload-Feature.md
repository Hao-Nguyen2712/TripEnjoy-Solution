# Cloudinary Document Upload Feature

## Overview

The TripEnjoy Partner Document Upload feature enables secure, scalable file uploads using Cloudinary's cloud storage service with client-direct upload architecture. This implementation follows Clean Architecture principles and provides comprehensive security validation.

## Architecture

### Client-Direct Upload Flow
```
1. Partner Client requests upload URL from API
2. API generates signed Cloudinary upload parameters
3. Client uploads file directly to Cloudinary
4. Client sends upload confirmation to API
5. API validates upload and saves document record
```

### Security Benefits
- ✅ Server validates each upload request
- ✅ Signed uploads prevent unauthorized uploads
- ✅ File validation ensures only valid files are saved
- ✅ Business rules enforced (no duplicates, valid types)
- ✅ JWT authentication required for all operations

## API Endpoints

### 1. Generate Upload URL
**POST** `/api/v1/partner/documents/upload-url`

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "documentType": "BusinessLicense",
  "fileName": "business-license.pdf"
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Upload URL generated successfully",
  "data": {
    "uploadUrl": "https://api.cloudinary.com/v1_1/your-cloud/auto/upload",
    "publicId": "partner_documents/12345/BusinessLicense/business-license_1696176000",
    "uploadParameters": {
      "timestamp": "1696176000",
      "public_id": "partner_documents/12345/BusinessLicense/business-license_1696176000",
      "folder": "partner_documents/12345/BusinessLicense",
      "resource_type": "auto",
      "allowed_formats": "jpg,jpeg,png,pdf,doc,docx",
      "max_file_size": "10485760",
      "quality": "auto:best",
      "fetch_format": "auto",
      "signature": "abc123def456...",
      "api_key": "your-api-key"
    },
    "timestamp": 1696176000,
    "signature": "abc123def456..."
  }
}
```

### 2. Confirm Document Upload
**POST** `/api/v1/partner/documents`

**Headers:**
```
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "documentType": "BusinessLicense",
  "publicId": "partner_documents/12345/BusinessLicense/business-license_1696176000",
  "documentUrl": "https://res.cloudinary.com/your-cloud/auto/upload/partner_documents/12345/BusinessLicense/business-license_1696176000",
  "signature": "abc123def456...",
  "timestamp": 1696176000
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Document added successfully",
  "data": "550e8400-e29b-41d4-a716-446655440000"
}
```

## Valid Document Types

The system accepts the following document types:

| Document Type | Description |
|---------------|-------------|
| `BusinessLicense` | Business registration license |
| `TaxIdentification` | Tax ID or EIN certificate |
| `ProofOfAddress` | Business address verification |
| `CompanyRegistration` | Articles of incorporation |
| `BankStatement` | Business bank account statement |
| `IdentityDocument` | Owner/director ID verification |

## File Requirements

### Supported Formats
- **Images:** `.jpg`, `.jpeg`, `.png`
- **Documents:** `.pdf`, `.doc`, `.docx`

### File Size Limits
- **Maximum Size:** 10MB per file
- **Optimization:** Cloudinary automatically optimizes images

## Configuration

### 1. Cloudinary Settings

Add to `appsettings.json`:
```json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

Add to `appsettings.Development.json`:
```json
{
  "Cloudinary": {
    "CloudName": "your-dev-cloud-name",
    "ApiKey": "your-dev-api-key",
    "ApiSecret": "your-dev-api-secret"
  }
}
```

### 2. Environment Variables (Recommended)

For production, use environment variables:
```bash
CLOUDINARY__CLOUDNAME=your-cloud-name
CLOUDINARY__APIKEY=your-api-key
CLOUDINARY__APISECRET=your-api-secret
```

## Client Implementation Example

### JavaScript/TypeScript

```typescript
class DocumentUploadService {
    private apiBaseUrl = 'https://localhost:7199/api/v1';
    private authToken: string;

    constructor(authToken: string) {
        this.authToken = authToken;
    }

    async uploadDocument(documentType: string, file: File): Promise<string> {
        try {
            // Step 1: Get upload URL
            const uploadUrlResponse = await fetch(`${this.apiBaseUrl}/partner/documents/upload-url`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    documentType,
                    fileName: file.name
                })
            });

            const uploadUrlData = await uploadUrlResponse.json();
            if (!uploadUrlData.data) throw new Error('Failed to get upload URL');

            // Step 2: Upload to Cloudinary
            const formData = new FormData();
            Object.entries(uploadUrlData.data.uploadParameters).forEach(([key, value]) => {
                formData.append(key, value as string);
            });
            formData.append('file', file);

            const uploadResponse = await fetch(uploadUrlData.data.uploadUrl, {
                method: 'POST',
                body: formData
            });

            const uploadResult = await uploadResponse.json();
            if (!uploadResult.secure_url) throw new Error('Upload failed');

            // Step 3: Confirm upload
            const confirmResponse = await fetch(`${this.apiBaseUrl}/partner/documents`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    documentType,
                    publicId: uploadResult.public_id,
                    documentUrl: uploadResult.secure_url,
                    signature: uploadUrlData.data.signature,
                    timestamp: uploadUrlData.data.timestamp
                })
            });

            const confirmData = await confirmResponse.json();
            return confirmData.data; // Document ID
            
        } catch (error) {
            console.error('Document upload failed:', error);
            throw error;
        }
    }
}

// Usage
const uploadService = new DocumentUploadService('your-jwt-token');
const documentId = await uploadService.uploadDocument('BusinessLicense', fileInput.files[0]);
```

## Error Handling

### Common Error Responses

#### Invalid Document Type
```json
{
  "status": "error",
  "message": "Validation failed",
  "errors": [
    {
      "code": "Partner.InvalidDocumentType",
      "detail": "Invalid document type. Valid types are: BusinessLicense, TaxIdentification, ProofOfAddress, CompanyRegistration, BankStatement, IdentityDocument",
      "field": "DocumentType"
    }
  ]
}
```

#### Duplicate Document Type
```json
{
  "status": "error",
  "message": "Validation failed",
  "errors": [
    {
      "code": "Partner.DuplicateDocumentType",
      "detail": "A document of this type has already been uploaded.",
      "field": "DocumentType"
    }
  ]
}
```

#### Upload Validation Failed
```json
{
  "status": "error",
  "message": "Validation failed",
  "errors": [
    {
      "code": "Partner.InvalidUpload",
      "detail": "File upload could not be validated.",
      "field": "PublicId"
    }
  ]
}
```

## Testing

### 1. Unit Tests
Test the core business logic:
- Document type validation
- Duplicate prevention
- File upload validation
- Cloudinary signature generation

### 2. Integration Tests
Test the full upload flow:
- Generate upload URL
- Mock Cloudinary upload
- Confirm document creation

### 3. Manual Testing
1. **Valid Upload:**
   - Use valid document type
   - Upload supported file format
   - Verify document is saved

2. **Invalid Scenarios:**
   - Try duplicate document type
   - Use invalid file format
   - Test expired signatures

### 4. Postman Collection

Import this collection for testing:

```json
{
  "info": {
    "name": "Partner Document Upload",
    "description": "Test collection for partner document upload feature"
  },
  "auth": {
    "type": "bearer",
    "bearer": [
      {
        "key": "token",
        "value": "{{partner_jwt_token}}",
        "type": "string"
      }
    ]
  },
  "item": [
    {
      "name": "Generate Upload URL",
      "request": {
        "method": "POST",
        "url": "{{base_url}}/api/v1/partner/documents/upload-url",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"documentType\": \"BusinessLicense\",\n  \"fileName\": \"business-license.pdf\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        }
      }
    },
    {
      "name": "Confirm Document Upload",
      "request": {
        "method": "POST",
        "url": "{{base_url}}/api/v1/partner/documents",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"documentType\": \"BusinessLicense\",\n  \"publicId\": \"partner_documents/12345/BusinessLicense/business-license_1696176000\",\n  \"documentUrl\": \"https://res.cloudinary.com/your-cloud/auto/upload/partner_documents/12345/BusinessLicense/business-license_1696176000\",\n  \"signature\": \"abc123def456\",\n  \"timestamp\": 1696176000\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        }
      }
    }
  ],
  "variable": [
    {
      "key": "base_url",
      "value": "https://localhost:7199"
    },
    {
      "key": "partner_jwt_token",
      "value": "your-jwt-token-here"
    }
  ]
}
```

## Security Considerations

### 1. Authentication
- All endpoints require valid JWT token
- Partner role validation enforced
- Account ownership verified

### 2. File Validation
- File type restrictions enforced
- File size limits applied
- Malicious file detection via Cloudinary

### 3. Upload Integrity
- Signed upload parameters prevent tampering
- Upload validation ensures file exists
- Timestamp validation prevents replay attacks

### 4. Business Rules
- Duplicate document prevention
- Document type validation
- Partner profile requirement

## Monitoring & Logging

### Key Metrics to Track
- Upload success/failure rates
- File size distributions
- Upload duration
- Error frequency by type

### Log Events
- Upload URL generation
- Successful uploads
- Validation failures
- Security violations

## Troubleshooting

### Common Issues

1. **"Cloudinary settings not configured"**
   - Verify appsettings.json has correct Cloudinary configuration
   - Check environment variables in production

2. **"Upload validation failed"**
   - Ensure file was actually uploaded to Cloudinary
   - Check if signature and timestamp match

3. **"Document type already exists"**
   - Partner has already uploaded this document type
   - Use replacement flow if needed

4. **"File too large"**
   - File exceeds 10MB limit
   - Compress file or use different format

### Support

For technical support or questions:
- Check server logs for detailed error information
- Verify Cloudinary dashboard for upload status
- Test with Postman collection
- Review API documentation

## Future Enhancements

### Planned Features
1. Document review/approval workflow
2. Document replacement capability
3. Bulk upload support
4. Progress tracking for large files
5. Document expiration management
6. Automated document processing

This implementation provides a robust, secure, and scalable foundation for partner document management in the TripEnjoy platform.