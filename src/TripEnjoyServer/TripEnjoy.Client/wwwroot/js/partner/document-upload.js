/**
 * Partner Document Upload JavaScript
 * Handles file upload to Cloudinary with progress tracking
 */

class DocumentUploader {
    constructor() {
        this.initializeEventListeners();
        this.currentUpload = null;
    }

    initializeEventListeners() {
        const form = document.getElementById('documentUploadForm');
        const fileInput = document.getElementById('documentFile');

        form.addEventListener('submit', this.handleFormSubmit.bind(this));
        fileInput.addEventListener('change', this.validateFile.bind(this));
    }

    async handleFormSubmit(event) {
        event.preventDefault();
        
        const formData = this.getFormData();
        if (!this.validateFormData(formData)) {
            return;
        }

        try {
            this.showProgress();
            this.updateProgress(0, 'Preparing upload...');

            // Step 1: Get upload URL from server
            const uploadUrlData = await this.getUploadUrl(formData.documentType, formData.file.name);
            this.updateProgress(20, 'Upload URL received...');

            // Step 2: Upload file to Cloudinary
            const cloudinaryResult = await this.uploadToCloudinary(uploadUrlData, formData.file);
            this.updateProgress(80, 'Confirming upload...');

            // Step 3: Confirm upload with server
            await this.confirmUpload({
                documentType: formData.documentType,
                publicId: cloudinaryResult.public_id,
                documentUrl: cloudinaryResult.secure_url,
                signature: uploadUrlData.signature,
                timestamp: uploadUrlData.timestamp
            });

            this.updateProgress(100, 'Upload complete!');
            setTimeout(() => this.showSuccess(), 500);

        } catch (error) {
            console.error('Upload failed:', error);
            this.showError(error.message || 'Upload failed. Please try again.');
        }
    }

    getFormData() {
        const documentType = document.getElementById('documentType').value;
        const fileInput = document.getElementById('documentFile');
        const description = document.getElementById('description').value;

        return {
            documentType,
            file: fileInput.files[0],
            description
        };
    }

    validateFormData(formData) {
        this.clearValidationErrors();

        let isValid = true;

        // Validate document type
        if (!formData.documentType) {
            this.showFieldError('documentType', 'Please select a document type.');
            isValid = false;
        }

        // Validate file
        if (!formData.file) {
            this.showFieldError('documentFile', 'Please select a file to upload.');
            isValid = false;
        } else {
            // Validate file size (10MB limit)
            const maxSize = 10 * 1024 * 1024; // 10MB in bytes
            if (formData.file.size > maxSize) {
                this.showFieldError('documentFile', 'File size must be less than 10MB.');
                isValid = false;
            }

            // Validate file type
            const allowedTypes = [
                'application/pdf',
                'application/msword',
                'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
                'image/jpeg',
                'image/jpg',
                'image/png'
            ];

            if (!allowedTypes.includes(formData.file.type)) {
                this.showFieldError('documentFile', 'Please upload a valid file type (PDF, DOC, DOCX, JPG, PNG).');
                isValid = false;
            }
        }

        return isValid;
    }

    validateFile(event) {
        const file = event.target.files[0];
        if (file) {
            this.clearFieldError('documentFile');
            
            // Show file info
            const fileInfo = `${file.name} (${this.formatFileSize(file.size)})`;
            this.showFileInfo(fileInfo);
        }
    }

    async getUploadUrl(documentType, fileName) {
        const headers = {
            'Content-Type': 'application/json'
        };

        // Add CSRF token to headers
        const csrfToken = this.getCsrfToken();
        if (csrfToken) {
            headers['RequestVerificationToken'] = csrfToken;
        }

        const response = await fetch('/partner/documents/generateuploadurl', {
            method: 'POST',
            headers: headers,
            credentials: 'include', // Include cookies for authentication
            body: JSON.stringify({
                documentType,
                fileName
            })
        });

        // Log response details for debugging
        console.log('Response status:', response.status);
        console.log('Response headers:', response.headers);
        
        const responseText = await response.text();
        console.log('Response text:', responseText);

        // Check if response is HTML (redirect to login)
        if (responseText.trimStart().startsWith('<!DOCTYPE') || responseText.trimStart().startsWith('<html')) {
            throw new Error('Authentication failed. Please sign in again.');
        }

        let result;
        try {
            result = JSON.parse(responseText);
        } catch (error) {
            console.error('Failed to parse JSON:', error);
            throw new Error('Invalid server response format');
        }
        
        if (!result.success) {
            throw new Error(result.message || 'Failed to get upload URL');
        }

        return result.data;
    }

    async uploadToCloudinary(uploadUrlData, file) {
        return new Promise((resolve, reject) => {
            // Debug logging
            console.log('Upload URL Data:', uploadUrlData);
            console.log('File info:', { name: file.name, size: file.size, type: file.type });
            
            const formData = new FormData();
            
            // Add all upload parameters
            Object.entries(uploadUrlData.uploadParameters).forEach(([key, value]) => {
                console.log(`Adding parameter: ${key} = ${value}`);
                formData.append(key, value);
            });
            
            // Add the file
            formData.append('file', file);

            const xhr = new XMLHttpRequest();

            // Track upload progress
            xhr.upload.addEventListener('progress', (event) => {
                if (event.lengthComputable) {
                    const percentComplete = Math.round((event.loaded / event.total) * 60) + 20; // 20-80%
                    this.updateProgress(percentComplete, 'Uploading file...');
                }
            });

            xhr.onload = () => {
                console.log('Upload response status:', xhr.status);
                console.log('Upload response:', xhr.responseText);
                
                if (xhr.status === 200) {
                    try {
                        const result = JSON.parse(xhr.responseText);
                        console.log('Parsed upload result:', result);
                        if (result.secure_url) {
                            resolve(result);
                        } else {
                            reject(new Error('Upload failed: No secure URL returned'));
                        }
                    } catch (error) {
                        console.error('Error parsing upload response:', error);
                        reject(new Error('Upload failed: Invalid response'));
                    }
                } else {
                    // Enhanced error handling for different status codes
                    let errorMessage = `Upload failed: HTTP ${xhr.status}`;
                    console.error('Upload failed with status:', xhr.status);
                    console.error('Error response:', xhr.responseText);
                    
                    try {
                        const errorResponse = JSON.parse(xhr.responseText);
                        console.error('Parsed error response:', errorResponse);
                        if (errorResponse.error && errorResponse.error.message) {
                            errorMessage = `Upload failed: ${errorResponse.error.message}`;
                        }
                    } catch (e) {
                        // Keep the default error message if parsing fails
                        console.error('Could not parse error response:', e);
                    }
                    reject(new Error(errorMessage));
                }
            };

            xhr.onerror = () => {
                reject(new Error('Upload failed: Network error'));
            };

            xhr.open('POST', uploadUrlData.uploadUrl);
            
            // Add CSRF token for requests to our own server endpoints
            // Note: This is NOT needed for direct Cloudinary uploads, but adding for completeness
            if (uploadUrlData.uploadUrl.includes(window.location.origin)) {
                const csrfToken = this.getCsrfToken();
                if (csrfToken) {
                    xhr.setRequestHeader('RequestVerificationToken', csrfToken);
                }
            }
            
            xhr.send(formData);
        });
    }

    async confirmUpload(uploadData) {
        const headers = {
            'Content-Type': 'application/json'
        };

        // Add CSRF token to headers
        const csrfToken = this.getCsrfToken();
        if (csrfToken) {
            headers['RequestVerificationToken'] = csrfToken;
        }

        const response = await fetch('/partner/documents/confirmupload', {
            method: 'POST',
            headers: headers,
            credentials: 'include', // Include cookies for authentication
            body: JSON.stringify(uploadData)
        });

        const result = await response.json();
        
        if (!result.success) {
            throw new Error(result.message || 'Failed to confirm upload');
        }

        return result.documentId;
    }

    showProgress() {
        document.getElementById('uploadProgress').style.display = 'block';
        document.getElementById('uploadBtn').disabled = true;
        document.getElementById('uploadBtn').innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Uploading...';
    }

    updateProgress(percent, status) {
        const progressBar = document.getElementById('progressBar');
        const progressPercent = document.getElementById('progressPercent');
        const progressStatus = document.getElementById('progressStatus');

        progressBar.style.width = percent + '%';
        progressPercent.textContent = percent + '%';
        progressStatus.textContent = status;
    }

    showSuccess() {
        document.getElementById('documentUploadForm').style.display = 'none';
        document.getElementById('uploadProgress').style.display = 'none';
        document.getElementById('uploadStatus').style.display = 'block';
        document.getElementById('successStatus').style.display = 'block';
    }

    showError(message) {
        document.getElementById('uploadProgress').style.display = 'none';
        document.getElementById('uploadBtn').disabled = false;
        document.getElementById('uploadBtn').innerHTML = '<i class="fas fa-upload me-1"></i>Upload Document';
        
        document.getElementById('uploadStatus').style.display = 'block';
        document.getElementById('errorStatus').style.display = 'block';
        document.getElementById('errorMessage').textContent = message;
    }

    showFieldError(fieldId, message) {
        const field = document.getElementById(fieldId);
        const feedback = field.nextElementSibling;
        
        field.classList.add('is-invalid');
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = message;
        }
    }

    clearFieldError(fieldId) {
        const field = document.getElementById(fieldId);
        const feedback = field.nextElementSibling;
        
        field.classList.remove('is-invalid');
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = '';
        }
    }

    clearValidationErrors() {
        document.querySelectorAll('.is-invalid').forEach(field => {
            field.classList.remove('is-invalid');
        });
        document.querySelectorAll('.invalid-feedback').forEach(feedback => {
            feedback.textContent = '';
        });
    }

    showFileInfo(info) {
        // You can show file info in a small element near the file input
        let fileInfoElement = document.getElementById('fileInfo');
        if (!fileInfoElement) {
            fileInfoElement = document.createElement('small');
            fileInfoElement.id = 'fileInfo';
            fileInfoElement.className = 'text-muted';
            document.getElementById('documentFile').parentNode.appendChild(fileInfoElement);
        }
        fileInfoElement.textContent = info;
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    getCsrfToken() {
        // Try to get CSRF token from meta tag
        const metaToken = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (metaToken) {
            return metaToken.getAttribute('content');
        }

        // Try to get from hidden input field
        const hiddenToken = document.querySelector('input[name="__RequestVerificationToken"]');
        if (hiddenToken) {
            return hiddenToken.value;
        }

        // Try to get from cookie (if using cookie-based CSRF)
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === '__RequestVerificationToken' || name === 'XSRF-TOKEN') {
                return decodeURIComponent(value);
            }
        }

        return null;
    }
}

// Global functions called from HTML
function resetForm() {
    const form = document.getElementById('documentUploadForm');
    form.reset();
    
    document.getElementById('uploadProgress').style.display = 'none';
    document.getElementById('uploadStatus').style.display = 'none';
    document.getElementById('uploadBtn').disabled = false;
    document.getElementById('uploadBtn').innerHTML = '<i class="fas fa-upload me-1"></i>Upload Document';
    
    uploader.clearValidationErrors();
    const fileInfo = document.getElementById('fileInfo');
    if (fileInfo) fileInfo.remove();
}

function uploadAnother() {
    resetForm();
    document.getElementById('documentUploadForm').style.display = 'block';
}

function tryAgain() {
    document.getElementById('uploadStatus').style.display = 'none';
    document.getElementById('errorStatus').style.display = 'none';
    document.getElementById('uploadBtn').disabled = false;
    document.getElementById('uploadBtn').innerHTML = '<i class="fas fa-upload me-1"></i>Upload Document';
}

// Initialize uploader when DOM is ready
let uploader;
document.addEventListener('DOMContentLoaded', function() {
    uploader = new DocumentUploader();
});

