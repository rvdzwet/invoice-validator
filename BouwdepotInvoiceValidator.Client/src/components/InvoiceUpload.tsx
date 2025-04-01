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
import { ValidationResult } from '../types/models';

interface InvoiceUploadProps {
  onValidationComplete: (result: ValidationResult) => void;
}

/**
 * Component for uploading and validating invoice files
 */
const InvoiceUpload: React.FC<InvoiceUploadProps> = ({ onValidationComplete }) => {
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
      setError('Only PDF files are accepted');
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
      setError('Please select a PDF file first');
      return;
    }

    try {
      setIsUploading(true);
      setError(null);
      
      const result = await apiService.validateInvoice(selectedFile);
      onValidationComplete(result);
    } catch (err) {
      console.error('Error validating invoice:', err);
      setError(
        err instanceof Error 
          ? `Error: ${err.message}` 
          : 'An error occurred while validating the invoice'
      );
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
    <Card elevation={3}>
      <CardHeader title="Upload Invoice" />
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
          startIcon={<CheckCircleIcon />}
          fullWidth
        >
          Validate Invoice
        </Button>
        
        {isUploading && (
          <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
            <CircularProgress size={24} sx={{ mr: 1 }} />
            <Typography>Validating invoice, please wait...</Typography>
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
