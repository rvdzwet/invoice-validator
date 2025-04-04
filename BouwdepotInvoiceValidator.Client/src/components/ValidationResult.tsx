import React from 'react';
import FraudDetectionPanel from './visualization/FraudDetectionPanel';
import VendorInsightsPanel from './visualization/VendorInsightsPanel';
import AuditReportPanel from './visualization/AuditReportPanel';
import { 
  Box, 
  Card, 
  CardContent, 
  CardHeader, 
  Chip, 
  List, 
  ListItem, 
  ListItemText, 
  Typography, 
  Divider,
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
  LinearProgress,
  Alert
} from '@mui/material';
import VerifiedIcon from '@mui/icons-material/Verified';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import HomeIcon from '@mui/icons-material/Home';
import BusinessIcon from '@mui/icons-material/Business';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import DescriptionIcon from '@mui/icons-material/Description';
import VisibilityIcon from '@mui/icons-material/Visibility';
import GavelIcon from '@mui/icons-material/Gavel';
import ReceiptIcon from '@mui/icons-material/Receipt';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import FilePresentIcon from '@mui/icons-material/FilePresent';
import HomeWorkIcon from '@mui/icons-material/HomeWork';
import EnergySavingsLeafIcon from '@mui/icons-material/EnergySavingsLeaf';
import InfoIcon from '@mui/icons-material/Info';
import SummarizeIcon from '@mui/icons-material/Summarize';
import { 
  ValidationResult as ValidationResultType, 
  ValidationSeverity
} from '../types/models';
import InvoiceDetails from './InvoiceDetails';

interface ValidationResultProps {
  result: ValidationResultType;
}

/**
 * Component for displaying validation results
 */
const ValidationResult: React.FC<ValidationResultProps> = ({ result }) => {
  /**
   * Get the color for the status card based on validation result
   */
  const getHeaderColor = () => {
    if (result.isValid) {
      return 'success.main';
    }
    if (result.possibleTampering) {
      return 'warning.main';
    }
    return 'error.main';
  };
  
  /**
   * Generate a comprehensive summary of validation results with key reasons
   */
  const generateValidationSummary = (): { summary: string, details: string[] } => {
    const details: string[] = [];
    let summary = '';
    
    // Base validation summary
    if (result.isValid) {
      summary = 'This invoice meets validation requirements and is accepted';
      
      // Add home improvement status
      if (result.isHomeImprovement) {
        details.push('Recognized as valid home improvement purchase');
      } else {
        details.push('Not recognized as a home improvement purchase');
      }
      
      // Add Bouwdepot compliance details when applicable
      if (result.isBouwdepotCompliant) {
        details.push('Meets Bouwdepot requirements (quality improvement and permanent attachment)');
        
        if (result.bouwdepotValidation?.generalValidationNotes) {
          details.push(result.bouwdepotValidation.generalValidationNotes);
        }
      }
      
      // Add Verduurzamingsdepot compliance details
      if (result.isVerduurzamingsdepotCompliant) {
        if (result.bouwdepotValidation?.matchedVerduurzamingsdepotCategories.length) {
          details.push(`Qualifies for Verduurzamingsdepot in categories: ${result.bouwdepotValidation.matchedVerduurzamingsdepotCategories.join(', ')}`);
        } else {
          details.push('Qualifies for Verduurzamingsdepot');
        }
      }
      
      // Add audit justification if available
      if (result.auditJustification) {
        details.push(result.auditJustification);
      }
      
      // Add key positive criteria assessments
      const positiveAssessments = result.criteriaAssessments?.filter(c => c.score >= 70) || [];
      if (positiveAssessments.length > 0) {
        positiveAssessments.slice(0, 2).forEach(assessment => {
          details.push(`Strong ${assessment.criterionName}: ${assessment.reasoning}`);
        });
      }
      
    } else {
      // Rejection summary
      summary = 'This invoice has been rejected';
      
      // Add all error issues as rejection reasons
      const errorIssues = result.issues.filter(i => i.severity === 2); // Error severity
      if (errorIssues.length > 0) {
        details.push(...errorIssues.map(i => i.message));
      }
      
      // Add Bouwdepot compliance details when applicable
      if (result.bouwdepotValidation && !result.isBouwdepotCompliant) {
        const failedRules: string[] = [];
        
        if (!result.bouwdepotValidation.qualityImprovementRule) {
          failedRules.push('Does not improve home quality');
        }
        
        if (!result.bouwdepotValidation.permanentAttachmentRule) {
          failedRules.push('Items are not permanently attached to the property');
        }
        
        if (failedRules.length > 0) {
          details.push(`Failed Bouwdepot requirements: ${failedRules.join(', ')}`);
        }
        
        if (result.bouwdepotValidation.generalValidationNotes) {
          details.push(result.bouwdepotValidation.generalValidationNotes);
        }
      }
      
      // Add fraud indicators if present
      if (result.possibleTampering) {
        details.push('Possible document tampering detected');
        
        const fraudIndicators = result.fraudIndicatorAssessments?.filter(i => i.concernLevel >= 70) || [];
        if (fraudIndicators.length > 0) {
          fraudIndicators.forEach(indicator => {
            details.push(`${indicator.indicatorName}: ${indicator.reasoning}`);
          });
        }
      }
      
      // Add document verification issues
      if (result.isVerifiedInvoice === false) {
        details.push(`Document identified as "${result.detectedDocumentType}" instead of a valid invoice`);
      }
      
      // Add audit justification if available
      if (result.auditJustification) {
        details.push(result.auditJustification);
      }
    }
    
    // Add warning issues for both accepted and rejected invoices
    const warningIssues = result.issues.filter(i => i.severity === 1); // Warning severity
    if (warningIssues.length > 0 && details.length < 5) {
      details.push(...warningIssues.map(i => `Warning: ${i.message}`).slice(0, 2));
    }
    
    return { summary, details };
  };
  
  const validationSummary = generateValidationSummary();

  /**
   * Get the color for issue severity
   */
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

  return (
    <Card 
      elevation={3} 
sx={{ 
        mt: 0,
        borderTop: 6, 
        borderColor: getHeaderColor() 
      }}
    >
      <CardHeader 
        title="Validation Result" 
        sx={{ bgcolor: getHeaderColor(), color: 'white' }}
      />
      <CardContent>
        {/* New Summary Section */}
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <SummarizeIcon sx={{ mr: 1 }} />
            <Typography variant="h6">Summary</Typography>
          </Box>
          
          <Alert 
            severity={result.isValid ? "success" : result.possibleTampering ? "warning" : "error"}
            sx={{ mb: 2 }}
          >
            <Typography variant="subtitle1" sx={{ fontWeight: 'medium' }}>
              {validationSummary.summary}
            </Typography>
          </Alert>
          
          {validationSummary.details.length > 0 && (
            <Paper variant="outlined" sx={{ p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Key Factors:
              </Typography>
              <List dense disablePadding>
                {validationSummary.details.map((detail, idx) => (
                  <ListItem key={idx} sx={{ py: 0.5 }}>
                    <InfoIcon fontSize="small" sx={{ mr: 1, opacity: 0.7 }} />
                    <ListItemText primary={detail} />
                  </ListItem>
                ))}
              </List>
            </Paper>
          )}
        </Box>
        
        <Divider sx={{ my: 3 }} />
        
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
          {result.isValid ? (
            <Chip 
              icon={<VerifiedIcon />} 
              label="Valid" 
              color="success" 
              variant="filled" 
            />
          ) : (
            <Chip 
              icon={<ErrorIcon />} 
              label="Invalid" 
              color="error" 
              variant="filled" 
            />
          )}
          
          {result.possibleTampering && (
            <Chip 
              icon={<WarningIcon />} 
              label="Possible Tampering" 
              color="warning" 
              variant="filled" 
            />
          )}
          
          {result.isHomeImprovement ? (
            <Chip 
              icon={<HomeIcon />} 
              label="Home Improvement" 
              color="info" 
              variant="filled" 
            />
          ) : (
            <Chip 
              label="Not Home Improvement" 
              color="default" 
              variant="filled" 
            />
          )}
          
          {result.isVerifiedInvoice !== undefined && (
            result.isVerifiedInvoice ? (
              <Chip 
                icon={<ReceiptIcon />} 
                label="Verified Invoice" 
                color="success" 
                variant="filled" 
              />
            ) : (
              <Chip 
                icon={<FilePresentIcon />} 
                label={`Detected: ${result.detectedDocumentType || 'Unknown Document'}`}
                color="error" 
                variant="filled" 
              />
            )
          )}

          {/* Bouwdepot Compliance Status */}
          {result.isBouwdepotCompliant !== undefined && (
            result.isBouwdepotCompliant ? (
              <Chip 
                icon={<HomeWorkIcon />} 
                label="Bouwdepot Compliant" 
                color="success" 
                variant="filled" 
              />
            ) : (
              <Chip 
                icon={<HomeWorkIcon />} 
                label="Not Bouwdepot Compliant" 
                color="error" 
                variant="filled" 
              />
            )
          )}
          
          {/* Verduurzamingsdepot Compliance Status */}
          {result.isVerduurzamingsdepotCompliant !== undefined && (
            result.isVerduurzamingsdepotCompliant ? (
              <Chip 
                icon={<EnergySavingsLeafIcon />} 
                label="Verduurzamingsdepot Compliant" 
                color="success" 
                variant="filled" 
              />
            ) : (
              <Chip 
                icon={<EnergySavingsLeafIcon />} 
                label="Not Verduurzamingsdepot Compliant" 
                color="error" 
                variant="filled" 
              />
            )
          )}
        </Box>
        
        {result.issues.length > 0 && (
          <>
            <Typography variant="h6" gutterBottom>
              Issues:
            </Typography>
            <List>
              {result.issues.map((issue, index) => (
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
            <Divider sx={{ my: 2 }} />
          </>
        )}
        
        {/* Document Verification Section */}
        {(result.isVerifiedInvoice !== undefined || 
          result.detectedDocumentType || 
          (result.presentInvoiceElements && result.presentInvoiceElements.length > 0) ||
          (result.missingInvoiceElements && result.missingInvoiceElements.length > 0)) && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <FilePresentIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Document Verification</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle1" gutterBottom>
                  Document Type: {result.isVerifiedInvoice ? 'Invoice' : (result.detectedDocumentType || 'Unknown Document Type')}
                </Typography>
                
                {result.documentVerificationConfidence !== undefined && (
                  <Box sx={{ mb: 2 }}>
                    <Typography variant="subtitle2" gutterBottom>
                      Confidence: {(result.documentVerificationConfidence * 100).toFixed(0)}%
                    </Typography>
                    <LinearProgress 
                      variant="determinate" 
                      value={result.documentVerificationConfidence * 100} 
                      color={result.documentVerificationConfidence >= 0.7 ? "success" : 
                            result.documentVerificationConfidence >= 0.5 ? "warning" : "error"} 
                      sx={{ height: 10, borderRadius: 5 }}
                    />
                  </Box>
                )}
                
                {/* Present Invoice Elements */}
                {result.presentInvoiceElements && result.presentInvoiceElements.length > 0 && (
                  <Box sx={{ mt: 3 }}>
                    <Typography variant="subtitle1" gutterBottom>Present Invoice Elements:</Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {result.presentInvoiceElements.map((element, index) => (
                        <Chip 
                          key={index}
                          icon={<CheckCircleIcon />}
                          label={element}
                          color="success"
                          variant="outlined"
                          size="small"
                          sx={{ mb: 1 }}
                        />
                      ))}
                    </Box>
                  </Box>
                )}
                
                {/* Missing Invoice Elements */}
                {result.missingInvoiceElements && result.missingInvoiceElements.length > 0 && (
                  <Box sx={{ mt: 3 }}>
                    <Typography variant="subtitle1" gutterBottom>Missing Invoice Elements:</Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {result.missingInvoiceElements.map((element, index) => (
                        <Chip 
                          key={index}
                          icon={<CancelIcon />}
                          label={element}
                          color="error"
                          variant="outlined"
                          size="small"
                          sx={{ mb: 1 }}
                        />
                      ))}
                    </Box>
                  </Box>
                )}
              </Box>
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Purchase Analysis Section */}
        {result.purchaseAnalysis && result.purchaseAnalysis.summary && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <HomeWorkIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Purchase Analysis</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
                    <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                      Summary
                    </Typography>
                    <Typography variant="body1">
                      {result.purchaseAnalysis.summary}
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Paper variant="outlined" sx={{ p: 2, mb: 2, height: '100%' }}>
                    <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                      Primary Purpose
                    </Typography>
                    <Typography variant="body1">
                      {result.purchaseAnalysis.primaryPurpose}
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Paper variant="outlined" sx={{ p: 2, mb: 2, height: '100%' }}>
                    <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                      Categories
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {result.purchaseAnalysis.categories.map((category, idx) => (
                        <Chip 
                          key={idx}
                          label={category}
                          color="primary"
                          variant="outlined"
                          size="small"
                          sx={{ mb: 1 }}
                        />
                      ))}
                    </Box>
                  </Paper>
                </Grid>
                
                {result.purchaseAnalysis.lineItemDetails && result.purchaseAnalysis.lineItemDetails.length > 0 && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                      Line Item Analysis
                    </Typography>
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Description</TableCell>
                            <TableCell>Interpreted As</TableCell>
                            <TableCell>Category</TableCell>
                            <TableCell align="center">Home Improvement</TableCell>
                            <TableCell>Notes</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {result.purchaseAnalysis.lineItemDetails.map((item, idx) => (
                            <TableRow key={idx}>
                              <TableCell>{item.description}</TableCell>
                              <TableCell>{item.interpretedAs}</TableCell>
                              <TableCell>{item.category}</TableCell>
                              <TableCell align="center">
                                {item.isHomeImprovement ? 
                                  <CheckCircleIcon color="success" /> : 
                                  <CancelIcon color="error" />}
                              </TableCell>
                              <TableCell>{item.notes}</TableCell>
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
        )}
        
        {/* Approval/Rejection Details */}
        {((result.approvalFactors && result.approvalFactors.length > 0) || 
          (result.rejectionFactors && result.rejectionFactors.length > 0) || 
          (result.metRules && result.metRules.length > 0) || 
          (result.violatedRules && result.violatedRules.length > 0) || 
          (result.recommendedActions && result.recommendedActions.length > 0)) && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <InfoIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Decision Details</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Grid container spacing={2}>
                {/* Approval Factors */}
                {result.approvalFactors && result.approvalFactors.length > 0 && (
                  <Grid item xs={12} md={6}>
                    <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
                      <Typography variant="subtitle1" color="success.main" gutterBottom fontWeight="medium">
                        Reasons for Approval
                      </Typography>
                      <List dense disablePadding>
                        {result.approvalFactors.map((factor, idx) => (
                          <ListItem key={idx} sx={{ py: 0.5 }}>
                            <CheckCircleIcon fontSize="small" color="success" sx={{ mr: 1 }} />
                            <ListItemText primary={factor} />
                          </ListItem>
                        ))}
                      </List>
                    </Paper>
                  </Grid>
                )}
                
                {/* Rejection Factors */}
                {result.rejectionFactors && result.rejectionFactors.length > 0 && (
                  <Grid item xs={12} md={6}>
                    <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
                      <Typography variant="subtitle1" color="error.main" gutterBottom fontWeight="medium">
                        Reasons for Rejection
                      </Typography>
                      <List dense disablePadding>
                        {result.rejectionFactors.map((factor, idx) => (
                          <ListItem key={idx} sx={{ py: 0.5 }}>
                            <CancelIcon fontSize="small" color="error" sx={{ mr: 1 }} />
                            <ListItemText primary={factor} />
                          </ListItem>
                        ))}
                      </List>
                    </Paper>
                  </Grid>
                )}
                
                {/* Met Rules */}
                {result.metRules && result.metRules.length > 0 && (
                  <Grid item xs={12} md={6}>
                    <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
                      <Typography variant="subtitle1" color="success.main" gutterBottom fontWeight="medium">
                        Rules Satisfied
                      </Typography>
                      <List dense disablePadding>
                        {result.metRules.map((rule, idx) => (
                          <ListItem key={idx} sx={{ py: 0.5 }}>
                            <CheckCircleIcon fontSize="small" color="success" sx={{ mr: 1 }} />
                            <ListItemText primary={rule} />
                          </ListItem>
                        ))}
                      </List>
                    </Paper>
                  </Grid>
                )}
                
                {/* Violated Rules */}
                {result.violatedRules && result.violatedRules.length > 0 && (
                  <Grid item xs={12} md={6}>
                    <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
                      <Typography variant="subtitle1" color="error.main" gutterBottom fontWeight="medium">
                        Rules Violated
                      </Typography>
                      <List dense disablePadding>
                        {result.violatedRules.map((rule, idx) => (
                          <ListItem key={idx} sx={{ py: 0.5 }}>
                            <CancelIcon fontSize="small" color="error" sx={{ mr: 1 }} />
                            <ListItemText primary={rule} />
                          </ListItem>
                        ))}
                      </List>
                    </Paper>
                  </Grid>
                )}
                
                {/* Recommended Actions */}
                {result.recommendedActions && result.recommendedActions.length > 0 && (
                  <Grid item xs={12}>
                    <Paper variant="outlined" sx={{ p: 2 }}>
                      <Typography variant="subtitle1" color="info.main" gutterBottom fontWeight="medium">
                        Recommended Actions
                      </Typography>
                      <List dense disablePadding>
                        {result.recommendedActions.map((action, idx) => (
                          <ListItem key={idx} sx={{ py: 0.5 }}>
                            <InfoIcon fontSize="small" color="info" sx={{ mr: 1 }} />
                            <ListItemText primary={action} />
                          </ListItem>
                        ))}
                      </List>
                    </Paper>
                  </Grid>
                )}
              </Grid>
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Enhanced Audit Report */}
        {result.auditReport ? (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <GavelIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Audit Documentation</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <AuditReportPanel auditReport={result.auditReport} />
            </AccordionDetails>
          </Accordion>
        ) : result.auditJustification && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <GavelIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Audit Documentation</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              {result.weightedScore !== undefined && (
                <Box sx={{ mb: 2 }}>
                  <Typography variant="subtitle1" gutterBottom>Overall Score: {result.weightedScore.toFixed(0)}/100</Typography>
                  <LinearProgress 
                    variant="determinate" 
                    value={result.weightedScore} 
                    color={result.weightedScore >= 70 ? "success" : result.weightedScore >= 50 ? "warning" : "error"} 
                    sx={{ height: 10, borderRadius: 5 }}
                  />
                </Box>
              )}
              
              <Typography variant="subtitle1" gutterBottom>Justification:</Typography>
              <Paper elevation={0} sx={{ p: 2, bgcolor: 'grey.100', mb: 2 }}>
                <Typography variant="body2">{result.auditJustification}</Typography>
              </Paper>
              
              {result.regulatoryNotes && (
                <>
                  <Typography variant="subtitle1" gutterBottom>Regulatory Notes:</Typography>
                  <Paper elevation={0} sx={{ p: 2, bgcolor: 'grey.100', mb: 2 }}>
                    <Typography variant="body2">{result.regulatoryNotes}</Typography>
                  </Paper>
                </>
              )}
              
              {result.auditInfo && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="caption" component="div" color="text.secondary">
                    Assessment performed on {new Date(result.auditInfo.timestampUtc).toLocaleString()} 
                    using {result.auditInfo.modelVersion}.
                  </Typography>
                  <Typography variant="caption" component="div" color="text.secondary">
                    Framework: {result.auditInfo.frameworkVersion}
                  </Typography>
                </Box>
              )}
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Fraud Detection Panel */}
        {result.fraudDetection && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <WarningIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Fraud Analysis</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <FraudDetectionPanel fraudDetection={result.fraudDetection} />
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Vendor Insights Panel */}
        {result.vendorInsights && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <BusinessIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Vendor Profile</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <VendorInsightsPanel vendorInsights={result.vendorInsights} />
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Criteria Assessments */}
        {result.criteriaAssessments && result.criteriaAssessments.length > 0 && (
          <Accordion sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <DescriptionIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Criteria Assessments</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Criterion</TableCell>
                      <TableCell align="center">Weight</TableCell>
                      <TableCell align="center">Score</TableCell>
                      <TableCell>Assessment</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {result.criteriaAssessments.map((assessment, index) => (
                      <TableRow key={index}>
                        <TableCell component="th" scope="row">{assessment.criterionName}</TableCell>
                        <TableCell align="center">{(assessment.weight * 100).toFixed(0)}%</TableCell>
                        <TableCell align="center">
                          <Chip 
                            label={`${assessment.score}/100`}
                            size="small"
                            color={assessment.score >= 70 ? "success" : assessment.score >= 50 ? "warning" : "error"}
                          />
                        </TableCell>
                        <TableCell>{assessment.reasoning}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Visual Assessments */}
        {result.visualAssessments && result.visualAssessments.length > 0 && (
          <Accordion sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <VisibilityIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Visual Analysis</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Element</TableCell>
                      <TableCell align="center">Score</TableCell>
                      <TableCell>Finding</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {result.visualAssessments.map((assessment, index) => (
                      <TableRow key={index}>
                        <TableCell component="th" scope="row">{assessment.elementName}</TableCell>
                        <TableCell align="center">
                          <Chip 
                            label={`${assessment.score}/100`}
                            size="small"
                            color={assessment.score >= 70 ? "success" : assessment.score >= 50 ? "warning" : "error"}
                          />
                        </TableCell>
                        <TableCell>{assessment.reasoning}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </AccordionDetails>
          </Accordion>
        )}
        
        {/* Bouwdepot Validation Section */}
        {result.bouwdepotValidation && (
          <Accordion defaultExpanded sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <HomeWorkIcon sx={{ mr: 1 }} />
                <Typography variant="h6">Bouwdepot Validation</Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Grid container spacing={2}>
                {/* Core Rules Status */}
                <Grid item xs={12}>
                  <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
                    <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                      Core Rules Status
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                      <Chip 
                        icon={result.bouwdepotValidation.qualityImprovementRule ? 
                          <CheckCircleIcon /> : <CancelIcon />}
                        label="Quality Improvement Rule"
                        color={result.bouwdepotValidation.qualityImprovementRule ? 
                          "success" : "error"}
                        variant="outlined"
                        size="small"
                        sx={{ mb: 1 }}
                      />
                      <Chip 
                        icon={result.bouwdepotValidation.permanentAttachmentRule ? 
                          <CheckCircleIcon /> : <CancelIcon />}
                        label="Permanent Attachment Rule"
                        color={result.bouwdepotValidation.permanentAttachmentRule ? 
                          "success" : "error"}
                        variant="outlined"
                        size="small"
                        sx={{ mb: 1 }}
                      />
                    </Box>
                    {result.bouwdepotValidation.generalValidationNotes && (
                      <Typography variant="body2" color="text.secondary">
                        {result.bouwdepotValidation.generalValidationNotes}
                      </Typography>
                    )}
                  </Paper>
                </Grid>

                {/* Verduurzamingsdepot Categories */}
                {result.bouwdepotValidation.isVerduurzamingsdepotItem && (
                  <Grid item xs={12}>
                    <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
                      <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                        Sustainability Categories (Verduurzamingsdepot)
                      </Typography>
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                        {result.bouwdepotValidation.matchedVerduurzamingsdepotCategories.map((category, idx) => (
                          <Chip 
                            key={idx}
                            icon={<EnergySavingsLeafIcon />}
                            label={category}
                            color="success"
                            variant="outlined"
                            size="small"
                            sx={{ mb: 1 }}
                          />
                        ))}
                      </Box>
                    </Paper>
                  </Grid>
                )}

                {/* Line Item Validations */}
                {result.bouwdepotValidation.lineItemValidations && 
                 result.bouwdepotValidation.lineItemValidations.length > 0 && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle1" gutterBottom fontWeight="medium">
                      Line Item Validation
                    </Typography>
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Item Description</TableCell>
                            <TableCell align="center">Improves Quality</TableCell>
                            <TableCell align="center">Permanently Attached</TableCell>
                            <TableCell align="center">Sustainability</TableCell>
                            <TableCell>Notes</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {result.bouwdepotValidation.lineItemValidations.map((item, idx) => (
                            <TableRow key={idx}>
                              <TableCell>{item.description}</TableCell>
                              <TableCell align="center">
                                {item.improveHomeQuality ? 
                                  <CheckCircleIcon color="success" /> : 
                                  <CancelIcon color="error" />}
                              </TableCell>
                              <TableCell align="center">
                                {item.isPermanentlyAttached ? 
                                  <CheckCircleIcon color="success" /> : 
                                  <CancelIcon color="error" />}
                              </TableCell>
                              <TableCell align="center">
                                {item.isVerduurzamingsdepotItem ? (
                                  <Chip 
                                    label={item.verduurzamingsdepotCategory || "Yes"}
                                    color="success"
                                    size="small"
                                  />
                                ) : (
                                  <Typography variant="body2" color="text.secondary">No</Typography>
                                )}
                              </TableCell>
                              <TableCell>
                                {item.validationNotes || 
                                  (item.violatedRules && item.violatedRules.length > 0 ? 
                                    `Violated: ${item.violatedRules.join(', ')}` : '')}
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
        )}

        {/* Invoice Details */}
        {result.extractedInvoice && (
          <Accordion defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography variant="h6">Invoice Details</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <InvoiceDetails invoice={result.extractedInvoice} />
            </AccordionDetails>
          </Accordion>
        )}
      </CardContent>
    </Card>
  );
};

export default ValidationResult;
