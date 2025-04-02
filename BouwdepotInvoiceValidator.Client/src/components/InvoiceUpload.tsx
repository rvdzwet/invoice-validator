import React, { useState, useRef } from 'react';
import { 
  Box, 
  Button, 
  Card, 
  CardContent, 
  CardHeader, 
  CircularProgress, 
  Alert, 
  Typography,
  useTheme
} from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { apiService } from '../services/api';
import { ValidationResult, ConsolidatedAuditReport } from '../types/models';

interface InvoiceUploadProps {
  onValidationComplete: (response: {
    validationResult: ValidationResult;
    auditReport: ConsolidatedAuditReport;
  }) => void;
  onValidationStart?: () => void;
  onValidationError?: (errorMessage: string) => void;
}

/**
 * Component for uploading and validating invoice files
 */
const InvoiceUpload: React.FC<InvoiceUploadProps> = ({ 
  onValidationComplete,
  onValidationStart,
  onValidationError 
}) => {
  const theme = useTheme();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Handle file selection
   */
  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files || files.length === 0) {
      setSelectedFile(null);
      return;
    }

    const file = files[0];
    
    // Validate file type
    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      const errorMsg = 'Only PDF files are accepted';
      setError(errorMsg);
      if (onValidationError) onValidationError(errorMsg);
      setSelectedFile(null);
      return;
    }
    
    // Clear previous errors
    setError(null);
    setSelectedFile(file);
  };

  /**
   * Handle file upload and validation
   */
  const handleValidate = async () => {
    if (!selectedFile) {
      const errorMsg = 'Please select a PDF file first';
      setError(errorMsg);
      if (onValidationError) onValidationError(errorMsg);
      return;
    }

    try {
      setIsUploading(true);
      setError(null);
      
      // Notify parent component that validation has started
      if (onValidationStart) onValidationStart();
      
      const response = await apiService.validateInvoice(selectedFile);
      onValidationComplete(response);
    } catch (err) {
      console.error('Error validating invoice:', err);
      const errorMsg = err instanceof Error 
        ? `Error: ${err.message}` 
        : 'An error occurred while validating the invoice';
      
      setError(errorMsg);
      if (onValidationError) onValidationError(errorMsg);
    } finally {
      setIsUploading(false);
    }
  };

  /**
   * Trigger file input click
   */
  const handleBrowseClick = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  return (
    <Card elevation={3} sx={{ bgcolor: '#ffffff' }}>
      <CardHeader 
        title="Upload Invoice" 
        sx={{ 
          bgcolor: theme.palette.primary.main, 
          color: '#ffffff',
          '& .MuiCardHeader-title': {
            fontWeight: 600
          }
        }} 
      />
      <CardContent>
        <Box sx={{ mb: 3 }}>
          <input
            ref={fileInputRef}
            accept=".pdf"
            style={{ display: 'none' }}
            id="invoice-file-upload"
            type="file"
            onChange={handleFileChange}
          />
          <Button
            variant="outlined"
            component="span"
            startIcon={<CloudUploadIcon />}
            fullWidth
            onClick={handleBrowseClick}
            sx={{ 
              p: 2, 
              border: '1px dashed grey',
              borderColor: theme.palette.grey[400],
              '&:hover': {
                borderColor: theme.palette.primary.main,
              }
            }}
          >
            Select PDF Invoice
          </Button>
          {selectedFile && (
            <Typography variant="body2" sx={{ mt: 1 }}>
              Selected: {selectedFile.name} ({Math.round(selectedFile.size / 1024)} KB)
            </Typography>
          )}
        </Box>
        
        <Button
          variant="contained"
          color="primary"
          onClick={handleValidate}
          disabled={!selectedFile || isUploading}
          startIcon={isUploading ? <CircularProgress size={20} color="inherit" /> : <CheckCircleIcon />}
          fullWidth
        >
          {isUploading ? 'Validating...' : 'Validate Invoice'}
        </Button>
        
        {isUploading && (
          <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
            <Typography variant="body2" color="text.secondary">
              Validating your invoice. This may take a moment as we perform a comprehensive analysis.
            </Typography>
          </Box>
        )}
        
        {error && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {error}
          </Alert>
        )}
      </CardContent>
    </Card>
  );
};

export default InvoiceUpload;
