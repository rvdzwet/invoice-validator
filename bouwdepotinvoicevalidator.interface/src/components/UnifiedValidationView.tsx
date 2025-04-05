import React, { useState } from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  CardHeader, 
  Chip, 
  Typography, 
  Divider,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  Tabs,
  Tab,
  Stack,
  List,
  ListItem,
  ListItemText,
  Alert
} from '@mui/material';
import VerifiedIcon from '@mui/icons-material/Verified';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import HomeIcon from '@mui/icons-material/Home';
import GavelIcon from '@mui/icons-material/Gavel';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import HomeWorkIcon from '@mui/icons-material/HomeWork';
// import AssessmentIcon from '@mui/icons-material/Assessment';
import FormatListBulletedIcon from '@mui/icons-material/FormatListBulleted';
import BuildIcon from '@mui/icons-material/Build';
import SummarizeIcon from '@mui/icons-material/Summarize';
import { 
  ValidationResult, 
  ConsolidatedAuditReport,
  ValidationSeverity 
} from '../types/models';

// Tab panel container for the tabbed interface
function TabPanel(props: {
  children?: React.ReactNode;
  index: number;
  value: number;
}) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`validation-tabpanel-${index}`}
      aria-labelledby={`validation-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ pt: 2 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

interface UnifiedValidationViewProps {
  validationResult: ValidationResult;
  auditReport?: ConsolidatedAuditReport;
  onRequestPayment?: () => void;
  onDownloadPdf?: () => void;
  isLoadingAudit?: boolean;
  onLoadAudit?: () => void;
}

/**
 * Unified component that combines validation result and audit report
 */
const UnifiedValidationView: React.FC<UnifiedValidationViewProps> = ({ 
  validationResult, 
  auditReport,
  onRequestPayment,
  onDownloadPdf,
  isLoadingAudit = false,
  // onLoadAudit - currently unused
}) => {
  const [tabValue, setTabValue] = useState(0);

  // Get color based on approval status
  const getStatusColor = (isPositive: boolean) => 
    isPositive ? 'success.main' : 'error.main';
  
  // Get icon for status
  const getStatusIcon = (isPositive: boolean) => 
    isPositive ? <CheckCircleIcon color="success" /> : <CancelIcon color="error" />;
  
  // Get color for score
  const getScoreColor = (score: number) => {
    if (score >= 70) return 'success';
    if (score >= 50) return 'warning';
    return 'error';
  };

  // Get color for issue severity
  const getIssueSeverityColor = (severity: ValidationSeverity) => {
    switch (severity) {
      case ValidationSeverity.Error:
        return 'error.main';
      case ValidationSeverity.Warning:
        return 'warning.main';
      case ValidationSeverity.Info:
        return 'info.main';
      default:
        return 'text.secondary';
    }
  };

  // Get the primary badge status
  const isApproved = auditReport?.isApproved ?? validationResult.isBouwdepotCompliant ?? false;
  
  // Helper to format bank account numbers securely (mask middle digits)
  const maskIBAN = (iban: string) => {
    if (!iban || iban.length < 14) return iban;
    // Show first 4 and last 2 digits, mask the middle
    return `${iban.substring(0, 4)} •••• •••• ${iban.substring(iban.length - 2)}`;
  };

  // Handle tab change
  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  // Derive approved items if we have audit report
  const approvedItems = auditReport?.lineItems?.filter(item => item.isApproved) || [];

  // Safe weighted score
  const weightedScore = validationResult.weightedScore || 0;
  
  return (
    <Card 
      elevation={3} 
      sx={{ 
        mt: { xs: 2, md: 0 },
        borderTop: 6, 
        borderColor: getStatusColor(isApproved) 
      }}
    >
      <CardHeader 
        title="Invoice Validation Report" 
        sx={{ bgcolor: getStatusColor(isApproved), color: 'white' }}
      />
      <CardContent>
        {/* Summary Section */}
        <Paper elevation={1} sx={{ p: 2, mb: 3, bgcolor: 'grey.50' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ flexGrow: 1 }}>
              SUMMARY
            </Typography>
            <Chip 
              icon={getStatusIcon(isApproved)}
              label={isApproved ? "APPROVED" : "REJECTED"}
              color={isApproved ? 'success' : 'error'}
              variant="filled"
              size="medium"
            />
            {(weightedScore > 0 || auditReport?.overallScore) && (
              <Chip 
                label={`SCORE: ${auditReport?.overallScore || Math.round(weightedScore)}%`}
                color={getScoreColor(auditReport?.overallScore || weightedScore)}
                variant="outlined"
                sx={{ ml: 1 }}
              />
            )}
          </Box>
          
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Typography variant="body1">
                {auditReport?.summaryText || 
                  (validationResult.auditJustification ? 
                    validationResult.auditJustification.split('.')[0] + '.' : 
                    (isApproved ? 
                      "This invoice meets the requirements for bouwdepot funding." : 
                      "This invoice does not meet the requirements for bouwdepot funding."
                    )
                  )
                }
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                {validationResult.isValid ? (
                  <Chip 
                    icon={<VerifiedIcon />} 
                    label="Valid Document" 
                    color="success" 
                    size="small"
                    variant="outlined" 
                  />
                ) : (
                  <Chip 
                    icon={<ErrorIcon />} 
                    label="Invalid Document" 
                    color="error" 
                    size="small"
                    variant="outlined" 
                  />
                )}
                
                {validationResult.possibleTampering && (
                  <Chip 
                    icon={<WarningIcon />} 
                    label="Tampering Detected" 
                    color="warning" 
                    size="small"
                    variant="outlined" 
                  />
                )}
                
                {validationResult.isHomeImprovement ? (
                  <Chip 
                    icon={<HomeIcon />} 
                    label="Home Improvement" 
                    color="info" 
                    size="small"
                    variant="outlined" 
                  />
                ) : (
                  <Chip 
                    label="Not Home Improvement" 
                    color="default" 
                    size="small"
                    variant="outlined" 
                  />
                )}
                
                {validationResult.isBouwdepotCompliant !== undefined && (
                  validationResult.isBouwdepotCompliant ? (
                    <Chip 
                      icon={<HomeWorkIcon />} 
                      label="Bouwdepot Compliant" 
                      color="success" 
                      size="small"
                      variant="outlined" 
                    />
                  ) : (
                    <Chip 
                      icon={<HomeWorkIcon />} 
                      label="Not Bouwdepot Compliant" 
                      color="error" 
                      size="small"
                      variant="outlined" 
                    />
                  )
                )}
              </Stack>
            </Grid>
          </Grid>
          
          {/* Loading indicator for audit report */}
          {isLoadingAudit && (
            <Box sx={{ mt: 2, textAlign: 'right' }}>
              <Typography variant="body2" color="text.secondary">
                Loading comprehensive audit report...
              </Typography>
            </Box>
          )}
        </Paper>

        {/* Main Tabs */}
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs 
            value={tabValue} 
            onChange={handleTabChange} 
            aria-label="validation tabs"
            variant="scrollable"
            scrollButtons="auto"
          >
            <Tab icon={<SummarizeIcon />} label="Overview" id="validation-tab-0" />
            <Tab icon={<FormatListBulletedIcon />} label="Line Items" id="validation-tab-1" />
            {auditReport && <Tab icon={<GavelIcon />} label="Audit Information" id="validation-tab-2" />}
            {validationResult.issues.length > 0 && (
              <Tab icon={<WarningIcon />} label="Issues" id="validation-tab-3" />
            )}
            <Tab icon={<BuildIcon />} label="Technical Details" id="validation-tab-4" />
          </Tabs>
        </Box>

        {/* Tab 1: Overview */}
        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            {/* Invoice Info */}
            <Grid item xs={12} md={6}>
              <Paper elevation={1} sx={{ p: 2 }}>
                <Typography variant="subtitle1" fontWeight="medium" gutterBottom>
                  INVOICE DETAILS
                </Typography>
                <Box sx={{ mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">Invoice Number</Typography>
                  <Typography fontWeight="medium">{validationResult.extractedInvoice?.invoiceNumber || 'Unknown'}</Typography>
                </Box>
                <Box sx={{ mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">Invoice Date</Typography>
                  <Typography>
                    {validationResult.extractedInvoice?.invoiceDate ? 
                      new Date(validationResult.extractedInvoice.invoiceDate).toLocaleDateString() : 
                      'Unknown'
                    }
                  </Typography>
                </Box>
                <Box sx={{ mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">Total Amount</Typography>
                  <Typography>
                    €{validationResult.extractedInvoice?.totalAmount?.toFixed(2) || '0.00'}
                  </Typography>
                </Box>
              </Paper>
            </Grid>
            
            {/* Vendor Info */}
            <Grid item xs={12} md={6}>
              <Paper elevation={1} sx={{ p: 2 }}>
                <Typography variant="subtitle1" fontWeight="medium" gutterBottom>
                  VENDOR DETAILS
                </Typography>
                <Box sx={{ mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">Vendor Name</Typography>
                  <Typography fontWeight="medium">
                    {validationResult.extractedInvoice?.vendorName || 'Unknown Vendor'}
                  </Typography>
                </Box>
                <Box sx={{ mb: 1 }}>
                  <Typography variant="body2" color="text.secondary">Vendor Address</Typography>
                  <Typography>
                    {validationResult.extractedInvoice?.vendorAddress || 'Address not available'}
                  </Typography>
                </Box>
                {auditReport?.bankDetails && (auditReport.bankDetails.iban || auditReport.bankDetails.bankName) && (
                  <>
                    <Divider sx={{ my: 1 }} />
                    <Box sx={{ mb: 1 }}>
                      <Typography variant="body2" color="text.secondary">Bank Details</Typography>
                      {auditReport.bankDetails.bankName && (
                        <Typography>{auditReport.bankDetails.bankName}</Typography>
                      )}
                      {auditReport.bankDetails.iban && (
                        <Typography>IBAN: {maskIBAN(auditReport.bankDetails.iban)}</Typography>
                      )}
                    </Box>
                  </>
                )}
              </Paper>
            </Grid>
          </Grid>
        </TabPanel>

        {/* Tab 2: Line Items */}
        <TabPanel value={tabValue} index={1}>
          <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
            <Typography variant="h6" gutterBottom>LINE ITEM VALIDATION</Typography>
            
            {/* Approved items */}
            {approvedItems.length > 0 ? (
              <>
                <Typography variant="subtitle1" gutterBottom>APPROVED ITEMS:</Typography>
                <TableContainer component={Paper} variant="outlined" sx={{ mb: 3 }}>
                  <Table size="small">
                    <TableHead sx={{ bgcolor: 'success.light' }}>
                      <TableRow>
                        <TableCell>Description</TableCell>
                        <TableCell align="right">Amount</TableCell>
                        <TableCell>Status</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {approvedItems.map((item, index) => (
                        <TableRow key={index}>
                          <TableCell>{item.description}</TableCell>
                          <TableCell align="right">€{item.amount.toFixed(2)}</TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <CheckCircleIcon color="success" sx={{ mr: 1 }} fontSize="small" />
                              <Typography variant="body2">Approved</Typography>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </>
            ) : null}
            
            {/* No line items case */}
            {(!validationResult.extractedInvoice?.lineItems || validationResult.extractedInvoice.lineItems.length === 0) && (
              <Alert severity="info">No line items were detected in this invoice.</Alert>
            )}
          </Paper>
        </TabPanel>

        {/* Tab 3: Audit Information (only if audit report is available) */}
        {auditReport && (
          <TabPanel value={tabValue} index={2}>
            <Paper elevation={1} sx={{ p: 2, mb: 3, bgcolor: 'grey.50' }}>
              <Typography variant="h6" gutterBottom>LEGAL AUDIT RECORD</Typography>
              
              {/* Audit Identification */}
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" color="text.secondary">Legal Audit Record ID</Typography>
                <Typography variant="body2" fontFamily="monospace">{auditReport.auditInformation.auditIdentifier}</Typography>
                <Typography variant="caption" display="block">
                  Created: {new Date(auditReport.auditInformation.validationTimestamp).toLocaleString()} UTC
                </Typography>
              </Box>
            </Paper>
          </TabPanel>
        )}

        {/* Tab 4: Issues (only if there are issues) */}
        {validationResult.issues.length > 0 && (
          <TabPanel value={tabValue} index={3}>
            <Paper elevation={1} sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>VALIDATION ISSUES</Typography>
              <List>
                {validationResult.issues.map((issue, index) => (
                  <ListItem key={index} disablePadding sx={{ py: 0.5 }}>
                    <ListItemText 
                      primary={issue.message}
                      primaryTypographyProps={{
                        color: getIssueSeverityColor(issue.severity),
                        fontWeight: issue.severity === ValidationSeverity.Error ? 'medium' : 'normal'
                      }}
                    />
                  </ListItem>
                ))}
              </List>
            </Paper>
          </TabPanel>
        )}

        {/* Tab 5: Technical Details */}
        <TabPanel value={tabValue} index={4}>
          <Paper elevation={1} sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>TECHNICAL DETAILS</Typography>
            
            {/* Processing notes */}
            <Typography variant="subtitle1" gutterBottom>Processing Notes</Typography>
            <List dense>
              {(auditReport?.technicalDetails?.processingNotes || []).map((note, idx) => (
                <ListItem key={idx}>
                  <Typography variant="body2">{note}</Typography>
                </ListItem>
              ))}
            </List>
          </Paper>
        </TabPanel>

        {/* Action buttons */}
        {auditReport && (
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3, gap: 2 }}>
            {onDownloadPdf && (
              <Button variant="outlined" onClick={onDownloadPdf}>
                Download PDF Report
              </Button>
            )}
            {onRequestPayment && (
              <Button 
                variant="contained" 
                color="primary" 
                onClick={onRequestPayment}
                disabled={!auditReport.isApproved}
              >
                Proceed with Payment
              </Button>
            )}
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default UnifiedValidationView;
