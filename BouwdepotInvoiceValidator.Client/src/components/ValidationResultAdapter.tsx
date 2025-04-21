import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';
import { ValidationResult } from '../types/models';
import { ComprehensiveWithdrawalProofResponse } from '../types/comprehensiveModels';
import { ComprehensiveValidationView } from './ComprehensiveValidationView';
import { ValidationContextView } from './ValidationContextView';

// Define ValidationContext interface to match the backend response
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

interface ValidationResultAdapterProps {
  validationResult: ValidationResult | ValidationContext | null;
  isLoading?: boolean;
}

/**
 * Adapter component that handles both ValidationResult and ValidationContext types
 * and provides a compatibility layer with the appropriate view components
 */
const ValidationResultAdapter: React.FC<ValidationResultAdapterProps> = ({ 
  validationResult, 
  isLoading = false 
}) => {
  // Add debug logging to help identify the type of result we're getting
  console.log('ValidationResultAdapter received result:', validationResult);
  // if (isLoading && !validationResult) {
  //   return (
  //     <Box display="flex" justifyContent="center" alignItems="center" height="400px">
  //       <CircularProgress />
  //       <Typography variant="h6" sx={{ ml: 2 }}>
  //         Processing document...
  //       </Typography>
  //     </Box>
  //   );
  // }
  
  if (!validationResult) {
    return null;
  }

  // Check if this is a ValidationContext (from withdrawal proof validation)
  const isValidationContext = (result: any): result is ValidationContext => {
    return result && 
      typeof result === 'object' &&
      'comprehensiveValidationResult' in result && 
      'overallOutcome' in result;
  };

  // If this is a ValidationContext, pass it directly to ValidationContextView
  if (isValidationContext(validationResult)) {
    console.log('Rendering ValidationContext result', validationResult);
    return <ValidationContextView validationContext={validationResult} />;
  }

  // Otherwise, handle it as a regular ValidationResult
  console.log('Rendering adapted ValidationResult', validationResult);

  // Extract invoice data from the ValidationResult type
  const invoice = (validationResult as ValidationResult).extractedInvoice;

  // Convert legacy ValidationResult to a simplified ComprehensiveWithdrawalProofResponse
  // This is a basic adapter - you may need to expand this mapping based on your needs
  const adaptedResponse: ComprehensiveWithdrawalProofResponse = {
    documentAnalysis: {
      documentType: validationResult.detectedDocumentType ? String(validationResult.detectedDocumentType) : null,
      confidence: validationResult.documentVerificationConfidence ? Number(validationResult.documentVerificationConfidence) : null,
      language: null,
      documentNumber: invoice?.invoiceNumber || null,
      issueDate: invoice?.invoiceDate || null,
      dueDate: null,
      totalAmount: invoice?.totalAmount || null,
      currency: invoice?.totalAmount ? (invoice.vatAmount ? (invoice.totalAmount > 0 ? 'EUR' : null) : null) : null,
      vendor: {
        name: invoice?.vendorName ? String(invoice.vendorName) : 'Unknown Vendor',
        address: invoice?.vendorAddress ? String(invoice.vendorAddress) : null,
        kvkNumber: invoice?.vendorKvkNumber ? String(invoice.vendorKvkNumber) : null,
        btwNumber: invoice?.vendorBtwNumber ? String(invoice.vendorBtwNumber) : null,
        contact: null
      },
      customer: {
        name: 'Customer', // Invoice doesn't appear to have customer name
        address: null
      },
      lineItems: invoice?.lineItems?.filter(item => item.description != null).map(item => ({
        description: String(item.description || "Unnamed Item"),
        quantity: Number(item.quantity || 0),
        unitPrice: Number(item.unitPrice || 0),
        taxRate: item.vatRate ? Number(item.vatRate) : null,
        totalPrice: Number(item.totalPrice || 0),
        lineItemTaxAmount: item.vatRate && item.totalPrice ? (Number(item.totalPrice) * (Number(item.vatRate) / 100)) : null,
        lineItemTotalWithTax: item.vatRate && item.totalPrice ? (Number(item.totalPrice) * (1 + Number(item.vatRate) / 100)) : null
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
        ? validationResult.fraudDetection.detectedIndicators.map(indicator => {
            // Ensure category is properly handled
            let categoryText: string;
            if (typeof indicator.category === 'number' && indicator.category >= 0 && indicator.category < 8) {
              categoryText = ['DocumentManipulation', 'ContentInconsistency', 'AnomalousPricing', 'VendorIssue', 
                'HistoricalPattern', 'DigitalArtifact', 'ContextualMismatch', 'BehavioralFlag'][indicator.category];
            } else if (indicator.category) {
              categoryText = String(indicator.category);
            } else {
              categoryText = 'Unknown';
            }
            
            return {
              category: categoryText,
              description: indicator.description ? String(indicator.description) : 'No description',
              confidence: indicator.confidence !== undefined ? Number(indicator.confidence) * 100 : 100,
              implication: indicator.evidence ? String(indicator.evidence) : 'No implication specified'
            };
          }) 
        : [],
      summary: validationResult.fraudDetection?.recommendedAction 
        ? String(validationResult.fraudDetection.recommendedAction) 
        : 'No fraud analysis available'
    },
    eligibilityDetermination: {
      overallStatus: validationResult.meetsApprovalThreshold === true ? 'Eligible' : 'Ineligible' as 'Eligible' | 'Partially Eligible' | 'Ineligible',
      decisionConfidenceScore: validationResult.confidenceScore || 100,
      totalEligibleAmountDetermined: validationResult.purchaseAnalysis?.homeImprovementPercentage 
        ? (invoice?.totalAmount || 0) * (validationResult.purchaseAnalysis.homeImprovementPercentage / 100) 
        : 0,
      totalIneligibleAmountDetermined: validationResult.purchaseAnalysis?.homeImprovementPercentage 
        ? (invoice?.totalAmount || 0) * (1 - validationResult.purchaseAnalysis.homeImprovementPercentage / 100) 
        : (invoice?.totalAmount || 0),
      totalDocumentAmountReviewed: invoice?.totalAmount || 0,
      rationaleCategory: validationResult.purchaseAnalysis?.primaryPurpose 
        ? String(validationResult.purchaseAnalysis.primaryPurpose) 
        : 'Unknown',
      rationaleSummary: 
        (validationResult.detailedReasoning ? String(validationResult.detailedReasoning) : null) || 
        (validationResult.purchaseAnalysis?.summary ? String(validationResult.purchaseAnalysis.summary) : null) || 
        'No validation summary available',
      requiredActions: validationResult.fraudDetection?.suggestedVerificationSteps 
        ? validationResult.fraudDetection.suggestedVerificationSteps.map(step => 
            typeof step === 'string' ? step : String(step)
          ) 
        : [],
      notesForAuditor: validationResult.auditReport?.executiveSummary 
        ? String(validationResult.auditReport.executiveSummary) 
        : null
    },
    auditSummary: {
      overallValidationSummary: validationResult.auditReport?.executiveSummary 
        ? String(validationResult.auditReport.executiveSummary) 
        : 'No validation summary available',
      keyFindingsSummary: validationResult.purchaseAnalysis?.summary 
        ? String(validationResult.purchaseAnalysis.summary) 
        : 'No key findings available',
      regulatoryComplianceNotes: validationResult.auditReport?.applicableRegulations 
        ? validationResult.auditReport.applicableRegulations.map(reg => 
            reg.description ? String(reg.description) : 'Regulation details not available'
          ) 
        : [],
      auditSupportingEvidenceReferences: []
    }
  };

  return <ComprehensiveValidationView validation={adaptedResponse} />;
};

export default ValidationResultAdapter;
