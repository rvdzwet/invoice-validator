import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Divider,
  Grid,
  Chip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  LinearProgress,
  Stack,
  List,
  ListItem,
  ListItemIcon,
  ListItemText
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import WarningIcon from '@mui/icons-material/Warning';
import InfoIcon from '@mui/icons-material/Info';
import HomeIcon from '@mui/icons-material/Home';
import DescriptionIcon from '@mui/icons-material/Description';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import BusinessIcon from '@mui/icons-material/Business';
import PercentIcon from '@mui/icons-material/Percent';
import SecurityIcon from '@mui/icons-material/Security';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import VisibilityIcon from '@mui/icons-material/Visibility';
import ReportProblemIcon from '@mui/icons-material/ReportProblem';
import { ValidationResult, ValidationSeverity, FraudRiskLevel } from '../types/models';

// Stater Theme colors based on provided website
const staterColors = {
  primary: '#562178', // Deep purple from Stater
  secondary: '#6c757d',
  success: '#28a745',
  warning: '#ffc107',
  error: '#dc3545',
  info: '#17a2b8',
  lightPurple: '#f4ebfa',
  background: '#ffffff'
};

interface ComprehensiveValidationViewProps {
  validationResult: ValidationResult;
  isLoading?: boolean;
}

const ComprehensiveValidationView: React.FC<ComprehensiveValidationViewProps> = ({
  validationResult,
  isLoading = false
}) => {
  // Helper functions for formatting and display
  
  const getStatusColor = (isPositive: boolean) => 
    isPositive ? staterColors.success : staterColors.error;
  
  const getStatusIcon = (isPositive: boolean) => 
    isPositive ? <CheckCircleIcon color="success" /> : <CancelIcon color="error" />;
  
  const getScoreColor = (score: number) => {
    if (score >= 70) return staterColors.success;
    if (score >= 50) return staterColors.warning;
    return staterColors.error;
  };
  
  const getSeverityIcon = (severity: ValidationSeverity) => {
    switch (severity) {
      case ValidationSeverity.Error:
        return <CancelIcon sx={{ color: staterColors.error }} />;
      case ValidationSeverity.Warning:
        return <WarningIcon sx={{ color: staterColors.warning }} />;
      case ValidationSeverity.Info:
      default:
        return <InfoIcon sx={{ color: staterColors.info }} />;
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <Card sx={{ mt: 3, p: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>Analyzing invoice...</Typography>
        <LinearProgress sx={{ my: 3 }} />
        <Typography variant="body2" color="textSecondary">
          This may take a few moments. We're comprehensively analyzing your invoice.
        </Typography>
      </Card>
    );
  }
  
  // Helper function to format duration
  const formatDuration = (seconds: number | undefined): string => {
    if (seconds === undefined || seconds === null) return 'N/A';
    if (seconds < 60) return `${seconds.toFixed(1)}s`;
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = (seconds % 60).toFixed(1);
    return `${minutes}m ${remainingSeconds}s`;
  };

  // Helper function for Fraud Risk Level
  const getFraudRiskChip = (level: FraudRiskLevel | undefined) => {
    switch (level) {
      case FraudRiskLevel.Low: return <Chip label="Low Risk" color="success" size="small" variant="outlined" />;
      case FraudRiskLevel.Medium: return <Chip label="Medium Risk" color="warning" size="small" variant="outlined" />;
      case FraudRiskLevel.High: return <Chip label="High Risk" color="error" size="small" variant="outlined" />;
      case FraudRiskLevel.Critical: return <Chip label="Critical Risk" color="error" size="small" />;
      default: return <Chip label="Unknown" size="small" variant="outlined" />;
    }
  };

  // Extract key data for display
  const invoice = validationResult.extractedInvoice;
  const isApproved = validationResult.isValid ?? false; // Use overall isValid from comprehensive response
  const confidenceScore = validationResult.confidenceScore || 0; // Use confidenceScore
  const lineItemDetails = validationResult.purchaseAnalysis?.lineItemDetails || [];

  return (
    <Box sx={{ 
      my: 3, 
      "& .MuiPaper-root": { 
        borderRadius: "8px" 
      }
    }}>
      {/* Primary Status Banner */}
      <Paper 
        elevation={3} 
        sx={{ 
          backgroundColor: getStatusColor(isApproved),
          color: '#fff',
          py: 2,
          px: 3,
          mb: 3,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          {getStatusIcon(isApproved)}
          <Typography variant="h5" sx={{ ml: 1, fontWeight: 600 }}>
            {isApproved ? 'APPROVED' : 'NOT APPROVED'}
          </Typography>
        </Box>
        <Box>
          <Chip
            icon={<PercentIcon />}
            label={`${confidenceScore}% Confidence`}
            sx={{
              backgroundColor: 'rgba(255,255,255,0.2)',
              color: '#fff',
              fontWeight: 500
            }}
          />
        </Box>
      </Paper>

      {/* Overall Summary Section */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, backgroundColor: staterColors.lightPurple }}>
        <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
          VALIDATION SUMMARY
        </Typography>
        <Typography variant="body1" sx={{ mb: 2 }}>
          {/* Use detailedReasoning if available, otherwise fallback */}
          {validationResult.detailedReasoning ||
            (isApproved
              ? "The invoice appears valid and meets the necessary criteria."
              : "The invoice validation raised concerns. Please review the details below."
            )
          }
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          <Chip
            icon={<DescriptionIcon />}
            label={validationResult.isVerifiedInvoice ? "Verified Document" : "Document Issues"}
            color={validationResult.isVerifiedInvoice ? "success" : "warning"}
            variant="outlined"
          />
          <Chip
            icon={<HomeIcon />}
            label={validationResult.isHomeImprovement ? "Home Improvement Related" : "Not Home Improvement"}
            color={validationResult.isHomeImprovement ? "success" : "warning"}
            variant="outlined"
          />
          {validationResult.purchaseAnalysis?.primaryPurpose && (
            <Chip 
              label={validationResult.purchaseAnalysis.primaryPurpose}
              color="primary"
              variant="outlined"
            />
          )}
        </Box>
      </Paper>

      {/* Two-column layout for invoice details and validation results */}
      <Grid container spacing={3}>
        {/* Left Column: Invoice Details */}
        <Grid item xs={12} md={5}>
          {/* Invoice Information */}
          <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
              INVOICE DETAILS
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={5}>
                  <Typography variant="body2" color="textSecondary">Invoice Number</Typography>
                </Grid>
                <Grid item xs={7}>
                  <Typography variant="body1" fontWeight="medium">{invoice?.invoiceNumber || 'Not available'}</Typography>
                </Grid>
                
                <Grid item xs={5}>
                  <Typography variant="body2" color="textSecondary">Invoice Date</Typography>
                </Grid>
                <Grid item xs={7}>
                  <Typography variant="body1">
                    {invoice?.invoiceDate ? new Date(invoice.invoiceDate).toLocaleDateString() : 'Not available'}
                  </Typography>
                </Grid>
                
                <Grid item xs={5}>
                  <Typography variant="body2" color="textSecondary">Total Amount</Typography>
                </Grid>
                <Grid item xs={7}>
                  <Typography variant="body1" fontWeight="medium">
                    €{invoice?.totalAmount?.toFixed(2) || 'Not available'}
                  </Typography>
                </Grid>
              </Grid>
            </Box>
          </Paper>

          {/* Vendor Information */}
          <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
              VENDOR DETAILS
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={5}>
                  <Typography variant="body2" color="textSecondary">Vendor Name</Typography>
                </Grid>
                <Grid item xs={7}>
                  <Typography variant="body1" fontWeight="medium">{invoice?.vendorName || 'Not available'}</Typography>
                </Grid>
                
                <Grid item xs={5}>
                  <Typography variant="body2" color="textSecondary">Vendor Address</Typography>
                </Grid>
                <Grid item xs={7}>
                  <Typography variant="body1">
                    {invoice?.vendorAddress || 'Not available'}
                  </Typography>
                </Grid>
                
                {invoice?.vendorKvkNumber && (
                  <>
                    <Grid item xs={5}>
                      <Typography variant="body2" color="textSecondary">KvK Number</Typography>
                    </Grid>
                    <Grid item xs={7}>
                      <Typography variant="body1">{invoice.vendorKvkNumber}</Typography>
                    </Grid>
                  </>
                )}
                
                {invoice?.vendorBtwNumber && (
                  <>
                    <Grid item xs={5}>
                      <Typography variant="body2" color="textSecondary">BTW Number</Typography>
                    </Grid>
                    <Grid item xs={7}>
                      <Typography variant="body1">{invoice.vendorBtwNumber}</Typography>
                    </Grid>
                  </>
                )}
              </Grid>
            </Box>
          </Paper>
        </Grid>

        {/* Right Column: Validation Results */}
        <Grid item xs={12} md={7}>
          {/* Bouwdepot Criteria Assessment */}
          <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
              BOUWDEPOT CRITERIA
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={7}>
                  <Typography variant="body2" color="textSecondary">Permanent Attachment Rule</Typography>
                </Grid>
                <Grid item xs={5}>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    {getStatusIcon(validationResult.bouwdepotValidation?.permanentAttachmentRule ?? false)}
                    <Typography variant="body1" sx={{ ml: 1 }}>
                    {validationResult.bouwdepotValidation?.permanentAttachmentRule ? "Passed" : "Failed"}
                  </Typography>
                </Box>
              </Grid>

              <Grid item xs={7}>
                <Typography variant="body2" color="textSecondary">Quality Improvement Rule</Typography>
              </Grid>
              <Grid item xs={5}>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  {getStatusIcon(validationResult.bouwdepotValidation?.qualityImprovementRule ?? false)}
                  <Typography variant="body1" sx={{ ml: 1 }}>
                    {validationResult.bouwdepotValidation?.qualityImprovementRule ? "Passed" : "Failed"}
                  </Typography>
                </Box>
              </Grid>

              {validationResult.bouwdepotValidation?.generalValidationNotes && (
                <Grid item xs={12}>
                  <Box sx={{ mt: 2, p: 2, backgroundColor: '#f5f5f5', borderRadius: 1 }}>
                    <Typography variant="body2" color="textSecondary">
                      {validationResult.bouwdepotValidation.generalValidationNotes}
                    </Typography>
                  </Box>
                </Grid>
              )}
            </Grid>
          </Box>
        </Paper>

        {/* Document Analysis Details */}
        <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            DOCUMENT ANALYSIS
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Document Type:</Typography></Grid>
            <Grid item xs={6}><Typography variant="body1">{validationResult.detectedDocumentType || 'N/A'}</Typography></Grid>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Verification Confidence:</Typography></Grid>
            <Grid item xs={6}><Typography variant="body1">{validationResult.documentVerificationConfidence?.toFixed(1) ?? 'N/A'}%</Typography></Grid>
            {validationResult.presentInvoiceElements && validationResult.presentInvoiceElements.length > 0 && (
              <Grid item xs={12}>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>Present Elements:</Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {validationResult.presentInvoiceElements.map(el => <Chip key={el} label={el} size="small" color="success" variant="outlined" />)}
                </Box>
              </Grid>
            )}
            {validationResult.missingInvoiceElements && validationResult.missingInvoiceElements.length > 0 && (
              <Grid item xs={12}>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>Missing Elements:</Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {validationResult.missingInvoiceElements.map(el => <Chip key={el} label={el} size="small" color="warning" variant="outlined" />)}
                </Box>
              </Grid>
            )}
          </Grid>
        </Paper>

        {/* Home Improvement Details */}
        <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            HOME IMPROVEMENT ANALYSIS
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Is Home Improvement:</Typography></Grid>
            <Grid item xs={6}>{getStatusIcon(validationResult.isHomeImprovement)}</Grid>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Confidence:</Typography></Grid>
            <Grid item xs={6}><Typography variant="body1">{validationResult.homeImprovementConfidence?.toFixed(1) ?? 'N/A'}%</Typography></Grid>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Category:</Typography></Grid>
            <Grid item xs={6}><Typography variant="body1">{validationResult.homeImprovementCategory || 'N/A'}</Typography></Grid>
            {validationResult.homeImprovementExplanation && (
              <Grid item xs={12}>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>Explanation:</Typography>
                <Typography variant="body2">{validationResult.homeImprovementExplanation}</Typography>
              </Grid>
            )}
            {validationResult.homeImprovementKeywords && validationResult.homeImprovementKeywords.length > 0 && (
              <Grid item xs={12}>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>Keywords:</Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {validationResult.homeImprovementKeywords.map(kw => <Chip key={kw} label={kw} size="small" variant="outlined" />)}
                </Box>
              </Grid>
            )}
          </Grid>
        </Paper>

        {/* Fraud Detection Details */}
        <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            FRAUD DETECTION
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Possible Fraud:</Typography></Grid>
            <Grid item xs={6}>{getStatusIcon(!validationResult.fraudDetection?.requiresManualReview)}</Grid> {/* Assuming requiresManualReview indicates fraud */}
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Risk Level:</Typography></Grid>
            <Grid item xs={6}>{getFraudRiskChip(validationResult.fraudDetection?.riskLevel)}</Grid>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Confidence:</Typography></Grid>
            <Grid item xs={6}><Typography variant="body1">{validationResult.fraudDetectionConfidence?.toFixed(1) ?? 'N/A'}%</Typography></Grid>
            {validationResult.fraudAssessmentExplanation && (
              <Grid item xs={12}>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>Assessment:</Typography>
                <Typography variant="body2">{validationResult.fraudAssessmentExplanation}</Typography>
              </Grid>
            )}
            {validationResult.fraudDetection?.detectedIndicators && validationResult.fraudDetection.detectedIndicators.length > 0 && (
              <Grid item xs={12}>
                <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>Indicators:</Typography>
                <List dense disablePadding>
                  {validationResult.fraudDetection.detectedIndicators.map((ind, i) => (
                    <ListItem key={i} disableGutters>
                      <ListItemIcon sx={{ minWidth: '24px' }}><ReportProblemIcon fontSize="small" color="warning" /></ListItemIcon>
                      <ListItemText primary={ind.indicatorName} secondary={ind.description} />
                    </ListItem>
                  ))}
                </List>
              </Grid>
            )}
          </Grid>
        </Paper>

        {/* Line Items Analysis */}
        <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            LINE ITEM VALIDATION
          </Typography>

          {lineItemDetails.length > 0 ? (
            <TableContainer>
              <Table size="small">
                <TableHead sx={{ bgcolor: staterColors.lightPurple }}>
                  <TableRow>
                    <TableCell>Description</TableCell>
                    <TableCell>Category</TableCell>
                    <TableCell align="center">Eligible</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {lineItemDetails.map((item, index) => (
                    <TableRow key={index}>
                      <TableCell>{item.description}</TableCell>
                      <TableCell>{item.category}</TableCell>
                      <TableCell align="center">
                        {getStatusIcon(item.isHomeImprovement)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Typography variant="body2" color="textSecondary">
              No line items were analyzed for this invoice.
            </Typography>
          )}
        </Paper>

        {/* Multi-modal Analysis */}
        <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            VISUAL ANALYSIS
          </Typography>
          <Grid container spacing={1}>
            <Grid item xs={6}><Typography variant="body2" color="textSecondary">Visual Anomalies Detected:</Typography></Grid>
            <Grid item xs={6}>{getStatusIcon(!validationResult.hasVisualAnomalies)}</Grid>
            {/* Add more details from visualAnalysis if needed */}
          </Grid>
        </Paper>

        {/* Validation Messages */}
        <Paper elevation={2} sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            VALIDATION ISSUES
          </Typography>

          {validationResult.issues && validationResult.issues.length > 0 ? (
            <List>
              {validationResult.issues.map((issue, index) => (
                <ListItem key={index} alignItems="flex-start" sx={{ py: 0.5 }}>
                  <ListItemIcon sx={{ minWidth: '36px' }}>
                    {getSeverityIcon(issue.severity)}
                  </ListItemIcon>
                  <ListItemText primary={issue.message} />
                </ListItem>
              ))}
            </List>
          ) : (
            <Typography variant="body2" color="textSecondary">
              No specific validation issues were logged.
            </Typography>
          )}
        </Paper>
      </Grid>
    </Grid>

    {/* Analysis Timing Information */}
    <Paper elevation={2} sx={{ p: 3, mt: 3 }}>
      <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
        ANALYSIS TIMING
      </Typography>
      <Grid container spacing={1}>
        <Grid item xs={4}><Typography variant="body2" color="textSecondary">Start Time:</Typography></Grid>
        <Grid item xs={8}><Typography variant="body1">{validationResult.analysisStartTime ? new Date(validationResult.analysisStartTime).toLocaleString() : 'N/A'}</Typography></Grid>
        <Grid item xs={4}><Typography variant="body2" color="textSecondary">Completion Time:</Typography></Grid>
        <Grid item xs={8}><Typography variant="body1">{validationResult.analysisCompletionTime ? new Date(validationResult.analysisCompletionTime).toLocaleString() : 'N/A'}</Typography></Grid>
        <Grid item xs={4}><Typography variant="body2" color="textSecondary">Duration:</Typography></Grid>
        <Grid item xs={8}><Typography variant="body1">{formatDuration(validationResult.analysisDurationSeconds)}</Typography></Grid>
      </Grid>
    </Paper>

    {/* Deprecated Detailed Assessment - Keep for now if needed, but prefer specific sections */}
    {validationResult.criteriaAssessments && validationResult.criteriaAssessments.length > 0 && (
      <Paper elevation={2} sx={{ p: 3, mt: 3, opacity: 0.7 }}>
        <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
          DETAILED ASSESSMENT (Legacy)
        </Typography>
        {/* ... existing table code ... */}
      </Paper>
    )}

    {/* Validation Timestamp - Use analysisCompletionTime */}
    <Box sx={{ mt: 3, textAlign: 'right' }}>
      <Typography variant="caption" color="textSecondary">
        Validation completed: {validationResult.analysisCompletionTime
          ? new Date(validationResult.analysisCompletionTime).toLocaleString()
          : 'Unknown'}
      </Typography>
    </Box>
  </Box>
);
};

export default ComprehensiveValidationView;
