import React, { useState } from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  CardHeader, 
  Chip, 
  Typography, 
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  List,
  ListItem,
  ListItemIcon,
  ListItemText
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import WarningIcon from '@mui/icons-material/Warning';
import VerifiedIcon from '@mui/icons-material/Verified';
import InfoIcon from '@mui/icons-material/Info';
import { ConsolidatedAuditReport as AuditReportType } from '../types/models';

interface AuditReportProps {
  report: AuditReportType;
  onRequestPayment?: () => void;
  onDownloadPdf?: () => void;
}

/**
 * Component that displays a consolidated audit report in a simplified structure
 */
const ConsolidatedAuditReport: React.FC<AuditReportProps> = ({ 
  report, 
  onRequestPayment,
  onDownloadPdf
}) => {
  const [showDetailedAnalysis, setShowDetailedAnalysis] = useState(false);
  
  const getStatusColor = (isPositive: boolean) => 
    isPositive ? 'success.main' : 'error.main';
  
  const getStatusIcon = (isPositive: boolean) => 
    isPositive ? <CheckCircleIcon color="success" /> : <CancelIcon color="error" />;
  
  const getScoreColor = (score: number) => {
    if (score >= 70) return 'success';
    if (score >= 50) return 'warning';
    return 'error';
  };

  // Helper to format bank account numbers securely (mask middle digits)
  const maskIBAN = (iban: string) => {
    if (!iban || iban.length < 14) return iban;
    // Show first 4 and last 2 digits, mask the middle
    return `${iban.substring(0, 4)} •••• •••• ${iban.substring(iban.length - 2)}`;
  };

  // Get approved and review items
  const approvedItems = report.lineItems.filter(item => item.isApproved);
  const reviewItems = report.lineItems.filter(item => !item.isApproved);
  
  return (
    <Card elevation={3} sx={{ mt: 3, borderTop: 6, borderColor: getStatusColor(report.isApproved) }}>
      <CardHeader 
        title="BOUWDEPOT AUDIT REPORT" 
        sx={{ bgcolor: getStatusColor(report.isApproved), color: 'white' }}
      />
      <CardContent>
        {/* Section 1: Summary with approval status */}
        <Paper elevation={1} sx={{ p: 2, mb: 3, bgcolor: 'grey.50' }}>
          <Typography variant="h6" gutterBottom>SUMMARY</Typography>
          <Typography variant="subtitle1" sx={{ mb: 1 }}>
            FUNDING ELIGIBILITY: {' '}
            <Chip 
              icon={getStatusIcon(report.isApproved)}
              label={report.isApproved ? "APPROVED" : "REJECTED"}
              color={report.isApproved ? 'success' : 'error'}
              variant="filled"
            />
            <Chip 
              label={`SCORE: ${report.overallScore}%`}
              color={getScoreColor(report.overallScore)}
              variant="outlined"
              sx={{ ml: 1 }}
            />
          </Typography>
          <Typography>{report.summaryText}</Typography>
        </Paper>

        {/* Section 2: Invoice & Vendor Information */}
        <Typography variant="h6" gutterBottom>INVOICE & VENDOR INFORMATION</Typography>
        <Grid container spacing={2} sx={{ mb: 3 }}>
          {/* Basic Invoice Info */}
          <Grid item xs={12} md={6}>
            <Paper elevation={1} sx={{ p: 2, height: '100%' }}>
              <Typography variant="subtitle1" fontWeight="medium" gutterBottom>
                INVOICE DETAILS
              </Typography>
              <Box sx={{ mb: 1 }}>
                <Typography variant="body2" color="text.secondary">Invoice Number</Typography>
                <Typography fontWeight="medium">{report.invoiceNumber}</Typography>
              </Box>
              <Box sx={{ mb: 1 }}>
                <Typography variant="body2" color="text.secondary">Invoice Date</Typography>
                <Typography>{new Date(report.invoiceDate).toLocaleDateString()}</Typography>
              </Box>
              <Box sx={{ mb: 1 }}>
                <Typography variant="body2" color="text.secondary">Total Amount</Typography>
                <Typography>€{report.totalAmount.toFixed(2)}</Typography>
              </Box>
            </Paper>
          </Grid>
          
          {/* Vendor Info */}
          <Grid item xs={12} md={6}>
            <Paper elevation={1} sx={{ p: 2, height: '100%' }}>
              <Typography variant="subtitle1" fontWeight="medium" gutterBottom>
                VENDOR DETAILS
              </Typography>
              <Box sx={{ mb: 1 }}>
                <Typography variant="body2" color="text.secondary">Vendor Name</Typography>
                <Typography fontWeight="medium">{report.vendorName}</Typography>
              </Box>
              <Box sx={{ mb: 1 }}>
                <Typography variant="body2" color="text.secondary">Vendor Address</Typography>
                <Typography>{report.vendorAddress || 'Not available'}</Typography>
              </Box>
            </Paper>
          </Grid>
        </Grid>

        {/* Section 2B: Bank Account Details */}
        <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
          <Typography variant="h6" gutterBottom>PAYMENT INFORMATION</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            {report.bankDetails.isVerified ? 
              <CheckCircleIcon color="success" sx={{ mr: 1 }} /> : 
              <InfoIcon color="warning" sx={{ mr: 1 }} />
            }
            <Typography>
              {report.bankDetails.isVerified ? 'Verified Bank Account' : 'Bank Account Pending Verification'}
            </Typography>
          </Box>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">Bank Name</Typography>
              <Typography>{report.bankDetails.bankName || 'Not provided'}</Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">IBAN</Typography>
              <Typography>{maskIBAN(report.bankDetails.iban) || 'Not provided'}</Typography>
            </Grid>
            {report.bankDetails.paymentReference && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary">Payment Reference</Typography>
                <Typography>{report.bankDetails.paymentReference}</Typography>
              </Grid>
            )}
          </Grid>
        </Paper>
        
        {/* Section 3: Validation Results */}
        <Typography variant="h6" gutterBottom>VALIDATION RESULTS</Typography>
        <Grid container spacing={2} sx={{ mb: 3 }}>
          {/* Document Integrity */}
          <Grid item xs={12} md={6}>
            <Paper elevation={1} sx={{ p: 2, height: '100%' }}>
              <Typography variant="subtitle1" fontWeight="medium" gutterBottom>
                DOCUMENT INTEGRITY
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                {getStatusIcon(report.documentValidation.isValid)}
                <Typography variant="subtitle1" sx={{ ml: 1 }}>
                  {report.documentValidation.isValid ? 'VALID' : 'INVALID'} ({report.documentValidation.score}%)
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                {report.documentValidation.primaryReason}
              </Typography>
              <Box>
                {report.documentValidation.validationDetails.map((detail, index) => (
                  <Box key={index} sx={{ display: 'flex', alignItems: 'flex-start', mb: 0.5 }}>
                    <CheckCircleIcon fontSize="small" sx={{ mr: 1, mt: 0.3 }} color="success" />
                    <Typography variant="body2">{detail}</Typography>
                  </Box>
                ))}
              </Box>
            </Paper>
          </Grid>
          
          {/* Construction Eligibility */}
          <Grid item xs={12} md={6}>
            <Paper elevation={1} sx={{ p: 2, height: '100%' }}>
              <Typography variant="subtitle1" fontWeight="medium" gutterBottom>
                CONSTRUCTION ELIGIBILITY
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                {getStatusIcon(report.bouwdepotEligibility.isEligible)}
                <Typography variant="subtitle1" sx={{ ml: 1 }}>
                  {report.bouwdepotEligibility.isEligible ? 'ELIGIBLE' : 'NOT ELIGIBLE'} ({report.bouwdepotEligibility.score}%)
                </Typography>
              </Box>
              <Box>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 0.5 }}>
                  {getStatusIcon(report.bouwdepotEligibility.meetsPermanentAttachment)}
                  <Typography variant="body2" sx={{ ml: 1 }}>
                    Items permanently attached to house
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 0.5 }}>
                  {getStatusIcon(report.bouwdepotEligibility.meetsQualityImprovement)}
                  <Typography variant="body2" sx={{ ml: 1 }}>
                    Improves home quality and value
                  </Typography>
                </Box>
              </Box>
            </Paper>
          </Grid>
        </Grid>
        
        {/* Section 3B: Payment details */}
        <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
          <Typography variant="h6" gutterBottom>BOUWDEPOT PAYMENT DETAILS</Typography>
          <Grid container spacing={2}>
            <Grid item xs={4}>
              <Typography variant="body2" color="text.secondary">Construction Type</Typography>
              <Typography>{report.bouwdepotEligibility.constructionType}</Typography>
            </Grid>
            <Grid item xs={4}>
              <Typography variant="body2" color="text.secondary">Payment Priority</Typography>
              <Typography>{report.bouwdepotEligibility.paymentPriority}</Typography>
            </Grid>
            <Grid item xs={4}>
              <Typography variant="body2" color="text.secondary">Special Conditions</Typography>
              <Typography>{report.bouwdepotEligibility.specialConditions || 'None'}</Typography>
            </Grid>
          </Grid>
        </Paper>
        
        {/* Section 4: Line Item Validation */}
        <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
          <Typography variant="h6" gutterBottom>LINE ITEM VALIDATION</Typography>
          
          {/* Approved items */}
          {approvedItems.length > 0 && (
            <>
              <Typography variant="subtitle1" gutterBottom>VALIDATED ITEMS:</Typography>
              {approvedItems.map((item, index) => (
                <Box key={index} sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
                  <CheckCircleIcon color="success" sx={{ mr: 1, mt: 0.3 }} />
                  <Typography>
                    {item.description} (€{item.amount.toFixed(2)})
                  </Typography>
                </Box>
              ))}
            </>
          )}
          
          {/* Items requiring review */}
          {reviewItems.length > 0 && (
            <>
              <Typography variant="subtitle1" gutterBottom sx={{ mt: 2 }}>
                ITEMS REQUIRING REVIEW:
              </Typography>
              {reviewItems.map((item, index) => (
                <Box key={index} sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
                  <WarningIcon color="warning" sx={{ mr: 1, mt: 0.3 }} />
                  <Typography>
                    {item.description} (€{item.amount.toFixed(2)}) - {item.validationNote}
                  </Typography>
                </Box>
              ))}
            </>
          )}
        </Paper>
        
        {/* Section 5: Legal Audit Trail */}
        <Paper elevation={1} sx={{ p: 2, mb: 3, bgcolor: 'grey.50' }}>
          <Typography variant="h6" gutterBottom>LEGAL AUDIT RECORD</Typography>
          
          {/* Audit Identification */}
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle2" color="text.secondary">Legal Audit Record ID</Typography>
            <Typography variant="body2" fontFamily="monospace">{report.auditInformation.auditIdentifier}</Typography>
            <Typography variant="caption" display="block">
              Created: {new Date(report.auditInformation.validationTimestamp).toLocaleString()} UTC
            </Typography>
          </Box>
          
          {/* Validator Information */}
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle2" color="text.secondary">Validator Information</Typography>
            <Typography variant="body2">
              Validated by: {report.auditInformation.validatedBy} 
              {report.auditInformation.validatorRole && ` (${report.auditInformation.validatorRole})`}
            </Typography>
            {report.auditInformation.validatorIdentifier && (
              <Typography variant="body2">
                Employee ID: {report.auditInformation.validatorIdentifier}
              </Typography>
            )}
          </Box>
          
          {/* Document Integrity */}
          {report.auditInformation.originalDocumentHash && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="subtitle2" color="text.secondary">Document Integrity</Typography>
              <Typography variant="body2" fontFamily="monospace" sx={{ wordBreak: 'break-all' }}>
                SHA-256: {report.auditInformation.originalDocumentHash}
              </Typography>
            </Box>
          )}
          
          {/* Conclusion and Key Factors */}
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle2" color="text.secondary">Conclusion</Typography>
            <Typography variant="body2">{report.auditInformation.conclusionExplanation}</Typography>
            
            {report.auditInformation.keyFactors && report.auditInformation.keyFactors.length > 0 && (
              <>
                <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 1 }}>Key Factors</Typography>
                <List dense disablePadding>
                  {report.auditInformation.keyFactors.map((factor, idx) => (
                    <ListItem key={idx} disableGutters>
                      <ListItemIcon sx={{ minWidth: 28 }}>
                        <InfoIcon fontSize="small" color="primary" />
                      </ListItemIcon>
                      <ListItemText primary={factor} />
                    </ListItem>
                  ))}
                </List>
              </>
            )}
          </Box>
          
          {/* Regulatory Compliance */}
          {report.auditInformation.legalBasis && report.auditInformation.legalBasis.length > 0 && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="subtitle2" color="text.secondary">Legal Basis</Typography>
              <List dense disablePadding>
                {report.auditInformation.legalBasis.map((ref, index) => (
                  <ListItem key={index} disableGutters>
                    <ListItemText
                      primary={ref.referenceCode}
                      secondary={ref.description}
                    />
                  </ListItem>
                ))}
              </List>
            </Box>
          )}
          
          {/* Verification Information */}
          <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
            <VerifiedIcon color="primary" sx={{ mr: 1 }} />
            <Typography variant="body2" color="primary">
              This audit record is digitally signed and tamper-evident
            </Typography>
          </Box>
        </Paper>
        
        {/* Section 6: Technical Details (Expandable) */}
        <Accordion 
          expanded={showDetailedAnalysis} 
          onChange={() => setShowDetailedAnalysis(!showDetailedAnalysis)}
        >
          <AccordionSummary 
            expandIcon={<ExpandMoreIcon />} 
            sx={{ bgcolor: 'primary.light', color: 'white' }}
          >
            <Typography fontWeight="medium">TECHNICAL DETAILS</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Grid container spacing={3}>
              {/* Technical Metrics */}
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Detailed Metrics</Typography>
                <TableContainer component={Paper} variant="outlined" sx={{ mb: 3 }}>
                  <Table size="small">
                    <TableHead sx={{ bgcolor: 'grey.100' }}>
                      <TableRow>
                        <TableCell>Metric</TableCell>
                        <TableCell>Value</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {Object.entries(report.technicalDetails.detailedMetrics).map(([key, value], idx) => (
                        <TableRow key={idx}>
                          <TableCell>{key}</TableCell>
                          <TableCell>{value}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Grid>
              
              {/* Processing Notes */}
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Processing Notes</Typography>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <List dense>
                    {report.technicalDetails.processingNotes.map((note, idx) => (
                      <ListItem key={idx}>
                        <Typography variant="body2">{note}</Typography>
                      </ListItem>
                    ))}
                  </List>
                </Paper>
              </Grid>
              
              {/* Rule Applications */}
              {report.auditInformation.appliedRules && report.auditInformation.appliedRules.length > 0 && (
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>Applied Rules</Typography>
                  <TableContainer component={Paper} variant="outlined">
                    <Table size="small">
                      <TableHead sx={{ bgcolor: 'grey.100' }}>
                        <TableRow>
                          <TableCell>Rule</TableCell>
                          <TableCell>Version</TableCell>
                          <TableCell align="center">Result</TableCell>
                          <TableCell>Details</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {report.auditInformation.appliedRules.map((rule, idx) => (
                          <TableRow key={idx}>
                            <TableCell>{rule.ruleName}</TableCell>
                            <TableCell>{rule.ruleVersion}</TableCell>
                            <TableCell align="center">
                              {rule.isSatisfied ? 
                                <Chip size="small" label="PASSED" color="success" /> : 
                                <Chip size="small" label="FAILED" color="error" />}
                            </TableCell>
                            <TableCell>{rule.applicationResult}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Grid>
              )}
              
              {/* Processing Steps */}
              {report.auditInformation.processingSteps && report.auditInformation.processingSteps.length > 0 && (
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>Processing Chain</Typography>
                  <TableContainer component={Paper} variant="outlined">
                    <Table size="small">
                      <TableHead sx={{ bgcolor: 'grey.100' }}>
                        <TableRow>
                          <TableCell>Timestamp</TableCell>
                          <TableCell>Process</TableCell>
                          <TableCell>Details</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {report.auditInformation.processingSteps.map((step, idx) => (
                          <TableRow key={idx}>
                            <TableCell>
                              {new Date(step.timestamp).toLocaleTimeString()}
                            </TableCell>
                            <TableCell>{step.processName}</TableCell>
                            <TableCell>
                              {step.inputState ? `${step.inputState} → ${step.outputState}` : step.outputState}
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Grid>
              )}
            </Grid>
          </AccordionDetails>
        </Accordion>
        
        {/* Action buttons */}
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3, gap: 2 }}>
          <Button variant="outlined" onClick={onDownloadPdf}>
            Download PDF Report
          </Button>
          <Button 
            variant="contained" 
            color="primary" 
            onClick={onRequestPayment}
            disabled={!report.isApproved}
          >
            Proceed with Payment
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};

export default ConsolidatedAuditReport;
