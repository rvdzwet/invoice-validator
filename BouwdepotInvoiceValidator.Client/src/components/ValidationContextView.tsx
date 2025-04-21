import React from 'react';
import { 
  Box, 
  Typography, 
  Paper, 
  Divider, 
  Chip, 
  Grid, 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableRow,
  Accordion,
  AccordionSummary,
  AccordionDetails
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { ComprehensiveValidationView } from './ComprehensiveValidationView';

// Define interface for ValidationContext
interface ValidationContext {
  id: string;
  inputDocument: {
    fileName: string;
    fileSizeBytes: number;
    fileType: string;
    uploadTimestamp: string;
  };
  comprehensiveValidationResult: any;
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

interface ValidationContextViewProps {
  validationContext: ValidationContext;
}

/**
 * Component to display the entire ValidationContext including processing steps, issues, AI models used,
 * validation results, and the comprehensive validation result.
 */
export const ValidationContextView: React.FC<ValidationContextViewProps> = ({ validationContext }) => {
  // Helper function to format file size
  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + ' bytes';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
  };

  // Helper function to format time for display
  const formatTimestamp = (timestamp: string): string => {
    return new Date(timestamp).toLocaleString();
  };

  // Helper function to get status color
  const getStatusColor = (status: string | null | undefined): 'success' | 'warning' | 'error' | 'default' => {
    if (typeof status !== 'string') {
      return 'default'; // Handle non-string inputs gracefully
    }
    switch (status.toLowerCase()) {
      case 'success':
      case 'completed':
        return 'success';
      case 'warning':
      case 'partial':
        return 'warning';
      case 'error':
      case 'failed':
        return 'error';
      default:
        return 'default';
    }
  };

  // Helper function to get severity color
  const getSeverityColor = (severity: string | null | undefined): 'success' | 'warning' | 'error' | 'default' => {
    if (typeof severity !== 'string') {
      return 'default'; // Handle non-string inputs gracefully
    }
    switch (severity.toLowerCase()) {
      case 'low':
      case 'info':
        return 'success';
      case 'medium':
      case 'warning':
        return 'warning';
      case 'high':
      case 'critical':
      case 'error':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <Box sx={{ mt: 3, mb: 5 }}>

      {/* Comprehensive Validation Results - Moved to top */}
      {validationContext.comprehensiveValidationResult && (
        <ComprehensiveValidationView validation={validationContext.comprehensiveValidationResult} />
      )}
      
    </Box>
  );
};
