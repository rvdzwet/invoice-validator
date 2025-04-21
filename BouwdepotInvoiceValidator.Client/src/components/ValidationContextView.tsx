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
  const getStatusColor = (status: string): 'success' | 'warning' | 'error' | 'default' => {
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
  const getSeverityColor = (severity: string): 'success' | 'warning' | 'error' | 'default' => {
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
      <Typography variant="h4" gutterBottom>
        Validation Results
      </Typography>
      
      {/* Overall Status Section */}
      <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h5">
            Overall Status
          </Typography>
          <Chip 
            label={validationContext.overallOutcome} 
            color={getStatusColor(validationContext.overallOutcome)} 
            sx={{ fontSize: '1rem', fontWeight: 'bold' }} 
          />
        </Box>
        
        <Typography variant="body1" paragraph>
          {validationContext.overallOutcomeSummary}
        </Typography>
        
        <Grid container spacing={2} sx={{ mt: 2 }}>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle1">Document: {validationContext.inputDocument.fileName}</Typography>
            <Typography variant="subtitle1">Size: {formatFileSize(validationContext.inputDocument.fileSizeBytes)}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle1">Uploaded: {formatTimestamp(validationContext.inputDocument.uploadTimestamp)}</Typography>
            <Typography variant="subtitle1">Processing Time: {validationContext.elapsedTime}</Typography>
          </Grid>
        </Grid>
      </Paper>
      
      {/* Processing Steps Accordion */}
      <Accordion defaultExpanded={false} sx={{ mb: 3 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">Processing Steps</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Step</TableCell>
                <TableCell>Description</TableCell>
                <TableCell align="center">Status</TableCell>
                <TableCell align="right">Timestamp</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {validationContext.processingSteps.map((step, index) => (
                <TableRow key={index}>
                  <TableCell>{step.stepName}</TableCell>
                  <TableCell>{step.description}</TableCell>
                  <TableCell align="center">
                    <Chip 
                      label={step.status} 
                      color={getStatusColor(step.status)} 
                      size="small" 
                    />
                  </TableCell>
                  <TableCell align="right">{formatTimestamp(step.timestamp)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </AccordionDetails>
      </Accordion>
      
      {/* Issues Accordion */}
      {validationContext.issues.length > 0 && (
        <Accordion defaultExpanded={false} sx={{ mb: 3 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Issues ({validationContext.issues.length})</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Type</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="center">Severity</TableCell>
                  <TableCell>Field</TableCell>
                  <TableCell align="right">Timestamp</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {validationContext.issues.map((issue, index) => (
                  <TableRow key={index}>
                    <TableCell>{issue.issueType}</TableCell>
                    <TableCell>{issue.description}</TableCell>
                    <TableCell align="center">
                      <Chip 
                        label={issue.severity} 
                        color={getSeverityColor(issue.severity)} 
                        size="small" 
                      />
                    </TableCell>
                    <TableCell>{issue.field || 'N/A'}</TableCell>
                    <TableCell align="right">{formatTimestamp(issue.timestamp)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </AccordionDetails>
        </Accordion>
      )}
      
      {/* AI Models Used Accordion */}
      <Accordion defaultExpanded={false} sx={{ mb: 3 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h6">AI Models Used</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Model</TableCell>
                <TableCell>Version</TableCell>
                <TableCell>Operation</TableCell>
                <TableCell align="right">Token Count</TableCell>
                <TableCell align="right">Timestamp</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {validationContext.aiModelsUsed.map((model, index) => (
                <TableRow key={index}>
                  <TableCell>{model.modelName}</TableCell>
                  <TableCell>{model.modelVersion}</TableCell>
                  <TableCell>{model.operation}</TableCell>
                  <TableCell align="right">{model.tokenCount.toLocaleString()}</TableCell>
                  <TableCell align="right">{formatTimestamp(model.timestamp)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </AccordionDetails>
      </Accordion>
      
      {/* Validation Rules Accordion */}
      {validationContext.validationResults.length > 0 && (
        <Accordion defaultExpanded={false} sx={{ mb: 3 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">Validation Rules ({validationContext.validationResults.length})</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Rule</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell align="center">Result</TableCell>
                  <TableCell align="center">Severity</TableCell>
                  <TableCell>Message</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {validationContext.validationResults.map((rule, index) => (
                  <TableRow key={index}>
                    <TableCell>{rule.ruleName}</TableCell>
                    <TableCell>{rule.description}</TableCell>
                    <TableCell align="center">
                      <Chip 
                        label={rule.result ? 'Pass' : 'Fail'} 
                        color={rule.result ? 'success' : 'error'} 
                        size="small" 
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Chip 
                        label={rule.severity} 
                        color={getSeverityColor(rule.severity)} 
                        size="small" 
                      />
                    </TableCell>
                    <TableCell>{rule.message}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </AccordionDetails>
        </Accordion>
      )}
      
      {/* Comprehensive Validation Results */}
      {validationContext.comprehensiveValidationResult && (
        <ComprehensiveValidationView validation={validationContext.comprehensiveValidationResult} />
      )}
    </Box>
  );
};
