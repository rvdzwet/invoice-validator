import React from 'react';
import { Box, Typography, Paper, Divider, Chip, Grid, Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material';
import { ComprehensiveWithdrawalProofResponse } from '../types/comprehensiveModels';

interface ComprehensiveValidationViewProps {
  validation: ComprehensiveWithdrawalProofResponse;
}

/**
 * Component for displaying comprehensive withdrawal proof validation results
 */
export const ComprehensiveValidationView: React.FC<ComprehensiveValidationViewProps> = ({ validation }) => {
  const { documentAnalysis, constructionActivities, fraudAnalysis, eligibilityDetermination, auditSummary } = validation;
  
  // Helper function to get status color
  const getStatusColor = (status: string): 'success' | 'warning' | 'error' | 'default' => {
    switch (status) {
      case 'Eligible': return 'success';
      case 'Partially Eligible': return 'warning';
      case 'Ineligible': return 'error';
      default: return 'default';
    }
  };

  // Helper function to get fraud level color
  const getFraudLevelColor = (level: string): 'success' | 'warning' | 'error' | 'default' => {
    switch (level) {
      case 'Low': return 'success';
      case 'Medium': return 'warning';
      case 'High': return 'error';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ mt: 3, mb: 5 }}>
      <Typography variant="h4" gutterBottom>
        Withdrawal Proof Validation Result
      </Typography>
      
      {/* Overall Status Section */}
      <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h5">
            Overall Status
          </Typography>
          <Chip 
            label={`${eligibilityDetermination.overallStatus} (${eligibilityDetermination.decisionConfidenceScore.toFixed(0)}%)`} 
            color={getStatusColor(eligibilityDetermination.overallStatus)} 
            sx={{ fontSize: '1rem', fontWeight: 'bold' }} 
          />
        </Box>
        
        <Typography variant="body1" paragraph>
          {eligibilityDetermination.rationaleSummary}
        </Typography>
        
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 2 }}>
          {eligibilityDetermination.requiredActions.map((action, index) => (
            <Chip key={index} label={action} color="primary" variant="outlined" />
          ))}
        </Box>
      </Paper>
      
      {/* Document Analysis Section */}
      <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>Document Analysis</Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle1">Document Type: {documentAnalysis.documentType || 'N/A'}</Typography>
            <Typography variant="subtitle1">Document Number: {documentAnalysis.documentNumber || 'N/A'}</Typography>
            <Typography variant="subtitle1">Issue Date: {documentAnalysis.issueDate || 'N/A'}</Typography>
            <Typography variant="subtitle1">Due Date: {documentAnalysis.dueDate || 'N/A'}</Typography>
            <Typography variant="subtitle1">Language: {documentAnalysis.language || 'N/A'}</Typography>
            <Typography variant="subtitle1">Confidence: {documentAnalysis.confidence !== null ? `${documentAnalysis.confidence}%` : 'N/A'}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle1">Total Amount: {documentAnalysis.totalAmount !== null ? 
              `${documentAnalysis.totalAmount} ${documentAnalysis.currency || ''}` : 'N/A'}</Typography>
            <Typography variant="subtitle1">Subtotal: {documentAnalysis.subtotal !== null ? 
              `${documentAnalysis.subtotal} ${documentAnalysis.currency || ''}` : 'N/A'}</Typography>
            <Typography variant="subtitle1">Tax Amount: {documentAnalysis.taxAmount !== null ? 
              `${documentAnalysis.taxAmount} ${documentAnalysis.currency || ''}` : 'N/A'}</Typography>
            <Typography variant="subtitle1">Payment Terms: {documentAnalysis.paymentTerms || 'N/A'}</Typography>
            <Typography variant="subtitle1">Multiple Documents: {documentAnalysis.multipleDocumentsDetected ? 'Yes' : 'No'}</Typography>
            <Typography variant="subtitle1">Document Count: {documentAnalysis.detectedDocumentCount}</Typography>
          </Grid>
        </Grid>
        
        <Box sx={{ mt: 3 }}>
          <Typography variant="h6">Vendor Information</Typography>
          <Typography>Name: {documentAnalysis.vendor?.name ?? 'N/A'}</Typography>
          <Typography>Address: {documentAnalysis.vendor?.address ?? 'N/A'}</Typography>
          <Typography>KvK Number: {documentAnalysis.vendor?.kvkNumber ?? 'N/A'}</Typography>
          <Typography>BTW Number: {documentAnalysis.vendor?.btwNumber ?? 'N/A'}</Typography>
          <Typography>Contact: {documentAnalysis.vendor?.contact ?? 'N/A'}</Typography>
        </Box>
        
        <Box sx={{ mt: 3 }}>
          <Typography variant="h6">Customer Information</Typography>
          <Typography>Name: {documentAnalysis.customer?.name ?? 'N/A'}</Typography>
          <Typography>Address: {documentAnalysis.customer?.address ?? 'N/A'}</Typography>
        </Box>
        
        {documentAnalysis.notes && (
          <Box sx={{ mt: 3 }}>
            <Typography variant="h6">Notes</Typography>
            <Typography>{documentAnalysis.notes}</Typography>
          </Box>
        )}
        
        {documentAnalysis.lineItems && documentAnalysis.lineItems.length > 0 && (
          <Box sx={{ mt: 3 }}>
            <Typography variant="h6">Line Items</Typography>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Description</TableCell>
                  <TableCell align="right">Quantity</TableCell>
                  <TableCell align="right">Unit Price</TableCell>
                  <TableCell align="right">Tax Rate</TableCell>
                  <TableCell align="right">Tax Amount</TableCell>
                  <TableCell align="right">Total Price</TableCell>
                  <TableCell align="right">Total With Tax</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {documentAnalysis.lineItems.map((item, index) => (
                  <TableRow key={index}>
                    <TableCell>{item.description}</TableCell>
                    <TableCell align="right">{item.quantity}</TableCell>
                    <TableCell align="right">{item.unitPrice}</TableCell>
                    <TableCell align="right">{item.taxRate !== null ? `${item.taxRate}%` : 'N/A'}</TableCell>
                    <TableCell align="right">{item.lineItemTaxAmount !== null ? item.lineItemTaxAmount : 'N/A'}</TableCell>
                    <TableCell align="right">{item.totalPrice}</TableCell>
                    <TableCell align="right">{item.lineItemTotalWithTax !== null ? item.lineItemTotalWithTax : 'N/A'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>
        )}
      </Paper>
      
      {/* Construction Activities Section */}
      <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>Construction Activities</Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Grid container spacing={2}>
          <Grid item xs={12} md={4}>
            <Typography>Construction Related Overall: {constructionActivities?.isConstructionRelatedOverall ? 'Yes' : 'No'}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography>Eligible Amount: {(constructionActivities?.totalEligibleAmountCalculated ?? 0).toFixed(2)}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography>Eligible Percentage: {(constructionActivities?.percentageEligibleCalculated ?? 0).toFixed(2)}%</Typography>
          </Grid>
        </Grid>
        
        <Typography variant="h6" sx={{ mt: 3 }}>Activity Analysis</Typography>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Description</TableCell>
              <TableCell>Category</TableCell>
              <TableCell align="center">Eligible</TableCell>
              <TableCell align="right">Eligible Amount</TableCell>
              <TableCell align="right">Ineligible Amount</TableCell>
              <TableCell align="right">Confidence</TableCell>
              <TableCell>Reason</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(constructionActivities?.detailedActivityAnalysis ?? []).map((activity, index) => (
              <TableRow key={index}>
                <TableCell>{activity.originalDescription}</TableCell>
                <TableCell>{activity.categorization}</TableCell>
                <TableCell align="center">
                  <Chip 
                    label={activity.isEligible ? 'Yes' : 'No'} 
                    color={activity.isEligible ? 'success' : 'error'} 
                    size="small" 
                  />
                </TableCell>
                <TableCell align="right">{activity.eligibleAmountForItem.toFixed(2)}</TableCell>
                <TableCell align="right">{activity.ineligibleAmountForItem.toFixed(2)}</TableCell>
                <TableCell align="right">{activity.confidence.toFixed(0)}%</TableCell>
                <TableCell>{activity.reasoningForEligibility}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
      
      {/* Fraud Analysis Section */}
      {/* Render this section only if fraudAnalysis is not null */}
      {fraudAnalysis && (
        <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h5" gutterBottom>Fraud Analysis</Typography>
          <Divider sx={{ mb: 2 }} />
          
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
            <Box>
              <Typography variant="subtitle1" display="inline">
                Fraud Risk Level: 
              </Typography>
              <Chip 
                // Safely access fraudRiskLevel, default to 'N/A'
                label={fraudAnalysis?.fraudRiskLevel ?? 'N/A'} 
                // Safely get color, default to 'default'
                color={getFraudLevelColor(fraudAnalysis?.fraudRiskLevel ?? 'N/A')} 
                sx={{ ml: 1 }} 
              />
            </Box>
            {/* Safely access fraudRiskScore, default to 0 */}
            <Typography variant="subtitle1">Fraud Risk Score: {fraudAnalysis?.fraudRiskScore ?? 0}</Typography>
          </Box>
          
          {/* Safely access summary, default to empty string */}
          <Typography variant="body1" paragraph>
            {fraudAnalysis?.summary ?? ''}
          </Typography>
          
          {/* Check if indicatorsFound exists and has items */}
          {(fraudAnalysis?.indicatorsFound?.length ?? 0) > 0 && (
            <>
              <Typography variant="h6" sx={{ mt: 3 }}>Fraud Indicators</Typography>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Category</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell align="right">Confidence</TableCell>
                    <TableCell>Implication</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {/* Safely map over indicatorsFound, default to empty array */}
                  {(fraudAnalysis?.indicatorsFound ?? []).map((indicator, index) => (
                    <TableRow key={index}>
                      <TableCell>{indicator.category}</TableCell>
                      <TableCell>{indicator.description}</TableCell>
                    <TableCell align="right">{indicator.confidence.toFixed(0)}%</TableCell>
                    <TableCell>{indicator.implication}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </>
        )}
      </Paper>
      )} {/* Add missing closing parenthesis for the fraudAnalysis conditional rendering */}
      
      {/* Eligibility Determination Section */}
      <Paper elevation={3} sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" gutterBottom>Eligibility Determination</Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle1">Overall Status: <Chip 
              label={eligibilityDetermination.overallStatus} 
              color={getStatusColor(eligibilityDetermination.overallStatus)} 
              size="small"
            /></Typography>
            <Typography variant="subtitle1">Confidence Score: {eligibilityDetermination.decisionConfidenceScore.toFixed(0)}%</Typography>
            <Typography variant="subtitle1">Rationale Category: {eligibilityDetermination.rationaleCategory}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="subtitle1">Total Eligible Amount: {eligibilityDetermination.totalEligibleAmountDetermined.toFixed(2)}</Typography>
            <Typography variant="subtitle1">Total Ineligible Amount: {eligibilityDetermination.totalIneligibleAmountDetermined.toFixed(2)}</Typography>
            <Typography variant="subtitle1">Total Document Amount: {eligibilityDetermination.totalDocumentAmountReviewed.toFixed(2)}</Typography>
          </Grid>
        </Grid>
        
        {eligibilityDetermination.notesForAuditor && (
          <Box sx={{ mt: 3 }}>
            <Typography variant="h6">Notes for Auditor</Typography>
            <Typography>{eligibilityDetermination.notesForAuditor}</Typography>
          </Box>
        )}
      </Paper>
      
      {/* Audit Summary Section */}
      <Paper elevation={3} sx={{ p: 3 }}>
        <Typography variant="h5" gutterBottom>Audit Summary</Typography>
        <Divider sx={{ mb: 2 }} />
        
        <Typography variant="h6">Overall Validation</Typography>
        <Typography variant="body1" paragraph>{auditSummary.overallValidationSummary}</Typography>
        
        <Typography variant="h6">Key Findings</Typography>
        <Typography variant="body1" paragraph>{auditSummary.keyFindingsSummary}</Typography>
        
        <Typography variant="h6">Regulatory Compliance</Typography>
        <ul>
          {auditSummary.regulatoryComplianceNotes.map((note, index) => (
            <li key={index}><Typography>{note}</Typography></li>
          ))}
        </ul>
        
        <Typography variant="h6" sx={{ mt: 3 }}>Supporting Evidence</Typography>
        <ul>
          {auditSummary.auditSupportingEvidenceReferences.map((evidence, index) => (
            <li key={index}><Typography>{evidence}</Typography></li>
          ))}
        </ul>
      </Paper>
    </Box>
  );
};
