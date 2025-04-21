import React, { useState } from 'react';
import { 
  Box, 
  Button, 
  Typography, 
  Paper, 
  Alert, 
  CircularProgress, 
  Divider
} from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import { apiService } from '../services/api';
import { ValidationContextView } from './ValidationContextView';
import { ComprehensiveWithdrawalProofResponse } from '../types/comprehensiveModels';

// Define interface for ValidationContext
interface ValidationContext {
  id: string;
  inputDocument: {
    fileName: string;
    fileSizeBytes: number;
    fileType: string;
    uploadTimestamp: string;
  };
  comprehensiveValidationResult: ComprehensiveWithdrawalProofResponse;
  overallOutcome: string;
  overallOutcomeSummary: string;
  processingSteps: Array<{
    stepName: string;
    description: string;
    status: string;
    timestamp: string;
  }>;
  issues: Array<{
    issueType: string;
    description: string;
    severity: string;
    field: string | null;
    timestamp: string;
    stackTrace: string | null;
  }>;
  aiModelsUsed: Array<{
    modelName: string;
    modelVersion: string;
    operation: string;
    tokenCount: number;
    timestamp: string;
  }>;
  validationResults: Array<{
    ruleId: string;
    ruleName: string;
    description: string;
    result: boolean;
    severity: string;
    message: string;
  }>;
  elapsedTime: string;
}

// Props interface
interface WithdrawalProofUploadProps {
  onValidationComplete?: (validationContext: ValidationContext) => void;
  onValidationStart?: () => void;
  onValidationError?: (errorMessage: string) => void;
}

/**
 * Component for uploading and validating withdrawal proof documents
 */
export const WithdrawalProofUpload: React.FC<WithdrawalProofUploadProps> = ({ 
  onValidationComplete,
  onValidationStart,
  onValidationError
}) => {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationContext, setValidationContext] = useState<ValidationContext | null>(null);

  /**
   * Handle file input change
   */
  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files && event.target.files.length > 0) {
      setFile(event.target.files[0]);
      setError(null);
    }
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async () => {
    if (!file) {
      const errorMsg = 'Please select a file to upload';
      setError(errorMsg);
      if (onValidationError) {
        onValidationError(errorMsg);
      }
      return;
    }

    setLoading(true);
    setError(null);
    
    // Notify parent component that validation has started
    if (onValidationStart) {
      onValidationStart();
    }
    
    try {
      const result = await apiService.validateWithdrawalProof(file);
      console.log('Withdrawal proof validation result:', result);
      
      // Set local state
      setValidationContext(result);
      
      // Pass result to parent component if callback provided
      if (onValidationComplete) {
        onValidationComplete(result);
      }
    } catch (err) {
      console.error('Validation error:', err);
      const errorMsg = 'An error occurred during validation. Please try again.';
      setError(errorMsg);
      
      // Pass error to parent component if callback provided
      if (onValidationError) {
        onValidationError(errorMsg);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', p: 3 }}>
      <Typography variant="h4" gutterBottom>Construction Fund Withdrawal Validation</Typography>
      
      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
      
      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h5" gutterBottom>Upload Withdrawal Proof Document</Typography>
        <Divider sx={{ mb: 3 }} />
        
        <Typography variant="body1" sx={{ mb: 2 }}>
          Submit a construction-related invoice, receipt, or quotation for analysis.
          The system will determine eligibility for construction fund withdrawal.
        </Typography>
        
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', my: 3 }}>
          <input
            accept="application/pdf,image/*"
            style={{ display: 'none' }}
            id="file-upload"
            type="file"
            onChange={handleFileChange}
          />
          <label htmlFor="file-upload">
            <Button variant="contained" component="span" startIcon={<UploadFileIcon />}>
              Select Document
            </Button>
          </label>
          
          {file && (
            <Box sx={{ mt: 2, textAlign: 'center' }}>
              <Typography variant="body1">Selected file: {file.name}</Typography>
              <Button 
                variant="contained" 
                color="primary" 
                onClick={handleSubmit} 
                disabled={loading}
                sx={{ mt: 2 }}
              >
                {loading ? <CircularProgress size={24} /> : 'Validate Document'}
              </Button>
            </Box>
          )}
        </Box>
        
        <Typography variant="body2" color="text.secondary">
          Note: Each submission must contain exactly one document. Multiple documents combined into a single file
          will be rejected. Supported formats include PDF and common image formats.
        </Typography>
      </Paper>
      
      {/* Only render the ValidationContextView if we're not using the parent's callback */}
      {validationContext && !onValidationComplete && (
        <ValidationContextView validationContext={validationContext} />
      )}
    </Box>
  );
};
