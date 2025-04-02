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
import { ValidationResult, ValidationSeverity } from '../types/models';

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
  
  // Extract key data for display
  const invoice = validationResult.extractedInvoice;
  const isApproved = validationResult.isBouwdepotCompliant ?? false;
  const confidenceScore = validationResult.weightedScore || 0;
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
            label={`${confidenceScore ?? 0}% Confidence`}
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
          {validationResult.auditJustification ||
            (isApproved 
              ? "This invoice meets the requirements for Bouwdepot funding." 
              : "This invoice does not meet the requirements for Bouwdepot funding."
            )
          }
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          <Chip 
            icon={<DescriptionIcon />} 
            label={validationResult.isValid ? "Valid Document" : "Invalid Document"}
            color={validationResult.isValid ? "success" : "error"}
            variant="outlined"
          />
          <Chip 
            icon={<HomeIcon />} 
            label={validationResult.isHomeImprovement ? "Home Improvement" : "Not Home Improvement"}
            color={validationResult.isHomeImprovement ? "success" : "error"}
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

          {/* Validation Messages */}
          <Paper elevation={2} sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
              VALIDATION NOTES
            </Typography>
            
            {validationResult.issues.length > 0 ? (
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
                No issues were detected with this invoice.
              </Typography>
            )}
          </Paper>
        </Grid>
      </Grid>

      {/* Additional Details Section - Full Width */}
      {validationResult.criteriaAssessments && validationResult.criteriaAssessments.length > 0 && (
        <Paper elevation={2} sx={{ p: 3, mt: 3 }}>
          <Typography variant="h6" sx={{ mb: 2, color: staterColors.primary, fontWeight: 600 }}>
            DETAILED ASSESSMENT
          </Typography>
          <TableContainer>
            <Table size="small">
              <TableHead sx={{ bgcolor: staterColors.lightPurple }}>
                <TableRow>
                  <TableCell>Criterion</TableCell>
                  <TableCell>Score</TableCell>
                  <TableCell>Reasoning</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {validationResult.criteriaAssessments && validationResult.criteriaAssessments.map((criterion, index) => (
                  <TableRow key={index}>
                    <TableCell>{criterion.criterionName}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Box sx={{ width: '50px', mr: 1 }}>
                          <LinearProgress 
                            variant="determinate" 
                            value={criterion.score} 
                            color={criterion.score >= 70 ? "success" : criterion.score >= 50 ? "warning" : "error"} 
                          />
                        </Box>
                        <Typography variant="body2">{criterion.score}%</Typography>
                      </Box>
                    </TableCell>
                    <TableCell>{criterion.reasoning}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}

      {/* Validation Timestamp - Using auditInfo if available */}
      <Box sx={{ mt: 3, textAlign: 'right' }}>
        <Typography variant="caption" color="textSecondary">
          Validation performed: {validationResult.auditInfo?.timestampUtc 
            ? new Date(validationResult.auditInfo.timestampUtc).toLocaleString() 
            : 'Unknown'}
        </Typography>
      </Box>
    </Box>
  );
};

export default ComprehensiveValidationView;
