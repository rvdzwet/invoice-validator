import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';
import { ValidationResult } from '../types/models';
import { ComprehensiveWithdrawalProofResponse } from '../types/comprehensiveModels';
import { ComprehensiveValidationView } from './ComprehensiveValidationView';

interface ValidationResultAdapterProps {
  validationResult: ValidationResult | null;
  isLoading?: boolean;
}

/**
 * Adapter component that handles legacy ValidationResult types and provides a compatibility
 * layer with the ComprehensiveValidationView component
 */
const ValidationResultAdapter: React.FC<ValidationResultAdapterProps> = ({ 
  validationResult, 
  isLoading = false 
}) => {
  if (isLoading && !validationResult) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="400px">
        <CircularProgress />
        <Typography variant="h6" sx={{ ml: 2 }}>
          Processing document...
        </Typography>
      </Box>
    );
  }
  
  if (!validationResult) {
    return null;
  }

  // Extract invoice data from validationResult
  const invoice = validationResult.extractedInvoice;

  // Convert legacy ValidationResult to a simplified ComprehensiveWithdrawalProofResponse
  // This is a basic adapter - you may need to expand this mapping based on your needs
  const adaptedResponse: ComprehensiveWithdrawalProofResponse = {
    documentAnalysis: {
      documentType: validationResult.detectedDocumentType || null,
      confidence: validationResult.documentVerificationConfidence || null,
      language: null,
      documentNumber: invoice?.invoiceNumber || null,
      issueDate: invoice?.invoiceDate || null,
      dueDate: null,
      totalAmount: invoice?.totalAmount || null,
      currency: invoice?.totalAmount ? (invoice.vatAmount ? (invoice.totalAmount > 0 ? 'EUR' : null) : null) : null,
      vendor: {
        name: invoice?.vendorName || 'Unknown Vendor',
        address: invoice?.vendorAddress || null,
        kvkNumber: invoice?.vendorKvkNumber || null,
        btwNumber: invoice?.vendorBtwNumber || null,
        contact: null
      },
      customer: {
        name: 'Customer', // Invoice doesn't appear to have customer name
        address: null
      },
      lineItems: invoice?.lineItems?.map(item => ({
        description: item.description,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        taxRate: item.vatRate,
        totalPrice: item.totalPrice,
        lineItemTaxAmount: item.vatRate ? (item.totalPrice * (item.vatRate / 100)) : null,
        lineItemTotalWithTax: item.vatRate ? (item.totalPrice * (1 + item.vatRate / 100)) : null
      })) || null,
      subtotal: invoice?.totalAmount ? (invoice.totalAmount - invoice.vatAmount) : null,
      taxAmount: invoice?.vatAmount || null,
      paymentTerms: null,
      notes: null,
      multipleDocumentsDetected: false,
      detectedDocumentCount: 1
    },
    constructionActivities: {
      isConstructionRelatedOverall: validationResult.isHomeImprovement || false,
      totalEligibleAmountCalculated: validationResult.purchaseAnalysis?.homeImprovementPercentage 
        ? (invoice?.totalAmount || 0) * (validationResult.purchaseAnalysis.homeImprovementPercentage / 100) 
        : 0,
      percentageEligibleCalculated: validationResult.purchaseAnalysis?.homeImprovementPercentage || 0,
      detailedActivityAnalysis: validationResult.purchaseAnalysis?.lineItemDetails 
        ? validationResult.purchaseAnalysis.lineItemDetails.map(item => ({
            originalDescription: item.description,
            categorization: item.category || 'Uncategorized',
            isEligible: item.isHomeImprovement || false,
            eligibleAmountForItem: 0, // Not available in the current model
            ineligibleAmountForItem: 0, // Not available in the current model
            confidence: item.confidence || 100,
            reasoningForEligibility: item.notes || 'No reason provided'
          })) 
        : invoice?.lineItems 
          ? invoice.lineItems.map(item => ({
              originalDescription: item.description,
              categorization: 'Unknown',
              isEligible: false,
              eligibleAmountForItem: 0,
              ineligibleAmountForItem: item.totalPrice,
              confidence: 0,
              reasoningForEligibility: 'No analysis available'
            }))
          : []
    },
    fraudAnalysis: {
      fraudRiskLevel: validationResult.fraudDetection?.riskLevel === undefined 
        ? 'NotApplicable' 
        : validationResult.fraudDetection.riskLevel === 0 ? 'Low'
        : validationResult.fraudDetection.riskLevel === 1 ? 'Medium'
        : validationResult.fraudDetection.riskLevel === 2 ? 'High'
        : 'NotApplicable',
      fraudRiskScore: validationResult.fraudDetection?.fraudRiskScore || 0,
      indicatorsFound: validationResult.fraudDetection?.detectedIndicators 
        ? validationResult.fraudDetection.detectedIndicators.map(indicator => ({
            category: typeof indicator.category === 'number' 
              ? ['DocumentManipulation', 'ContentInconsistency', 'AnomalousPricing', 'VendorIssue', 
                 'HistoricalPattern', 'DigitalArtifact', 'ContextualMismatch', 'BehavioralFlag'][indicator.category] 
              : String(indicator.category),
            description: indicator.description || 'No description',
            confidence: indicator.confidence !== undefined ? indicator.confidence * 100 : 100,
            implication: indicator.evidence || 'No implication specified'
          })) 
        : [],
      summary: validationResult.fraudDetection?.recommendedAction || 'No fraud analysis available'
    },
    eligibilityDetermination: {
      overallStatus: validationResult.meetsApprovalThreshold ? 'Eligible' : 'Ineligible',
      decisionConfidenceScore: validationResult.confidenceScore || 100,
      totalEligibleAmountDetermined: validationResult.purchaseAnalysis?.homeImprovementPercentage 
        ? (invoice?.totalAmount || 0) * (validationResult.purchaseAnalysis.homeImprovementPercentage / 100) 
        : 0,
      totalIneligibleAmountDetermined: validationResult.purchaseAnalysis?.homeImprovementPercentage 
        ? (invoice?.totalAmount || 0) * (1 - validationResult.purchaseAnalysis.homeImprovementPercentage / 100) 
        : (invoice?.totalAmount || 0),
      totalDocumentAmountReviewed: invoice?.totalAmount || 0,
      rationaleCategory: validationResult.purchaseAnalysis?.primaryPurpose || 'Unknown',
      rationaleSummary: validationResult.detailedReasoning || validationResult.purchaseAnalysis?.summary || 'No validation summary available',
      requiredActions: validationResult.fraudDetection?.suggestedVerificationSteps || [],
      notesForAuditor: validationResult.auditReport?.executiveSummary || null
    },
    auditSummary: {
      overallValidationSummary: validationResult.auditReport?.executiveSummary || 'No validation summary available',
      keyFindingsSummary: validationResult.purchaseAnalysis?.summary || 'No key findings available',
      regulatoryComplianceNotes: validationResult.auditReport?.applicableRegulations 
        ? validationResult.auditReport.applicableRegulations.map(reg => reg.description) 
        : [],
      auditSupportingEvidenceReferences: []
    }
  };

  return <ComprehensiveValidationView validation={adaptedResponse} />;
};

export default ValidationResultAdapter;
