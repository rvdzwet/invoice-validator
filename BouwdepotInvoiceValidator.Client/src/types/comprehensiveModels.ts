/**
 * Comprehensive withdrawal proof validation response
 */
export interface ComprehensiveWithdrawalProofResponse {
  documentAnalysis: DocumentAnalysis;
  constructionActivities: ConstructionActivities;
  fraudAnalysis: FraudAnalysis;
  eligibilityDetermination: EligibilityDetermination;
  auditSummary: AuditSummary;
}

/**
 * Document analysis section
 */
export interface DocumentAnalysis {
  documentType: string | null;
  confidence: number | null;
  language: string | null;
  documentNumber: string | null;
  issueDate: string | null;
  dueDate: string | null;
  totalAmount: number | null;
  currency: string | null;
  vendor: VendorInfo;
  customer: CustomerInfo;
  lineItems: DocumentLineItem[] | null;
  subtotal: number | null;
  taxAmount: number | null;
  paymentTerms: string | null;
  notes: string | null;
  multipleDocumentsDetected: boolean;
  detectedDocumentCount: number;
}

/**
 * Vendor information
 */
export interface VendorInfo {
  name: string;
  address: string | null;
  kvkNumber: string | null;
  btwNumber: string | null;
  contact: string | null;
}

/**
 * Customer information
 */
export interface CustomerInfo {
  name: string;
  address: string | null;
}

/**
 * Document line item
 */
export interface DocumentLineItem {
  description: string;
  quantity: number;
  unitPrice: number;
  taxRate: number | null;
  totalPrice: number;
  lineItemTaxAmount: number | null;
  lineItemTotalWithTax: number | null;
}

/**
 * Construction activities section
 */
export interface ConstructionActivities {
  isConstructionRelatedOverall: boolean;
  totalEligibleAmountCalculated: number;
  percentageEligibleCalculated: number;
  detailedActivityAnalysis: ActivityAnalysis[];
}

/**
 * Activity analysis for a line item
 */
export interface ActivityAnalysis {
  originalDescription: string;
  categorization: string;
  isEligible: boolean;
  eligibleAmountForItem: number;
  ineligibleAmountForItem: number;
  confidence: number;
  reasoningForEligibility: string;
}

/**
 * Fraud analysis section
 */
export interface FraudAnalysis {
  fraudRiskLevel: 'Low' | 'Medium' | 'High' | 'NotApplicable';
  fraudRiskScore: number;
  indicatorsFound: FraudIndicatorInfo[];
  summary: string;
}

/**
 * Fraud indicator information
 */
export interface FraudIndicatorInfo {
  category: string;
  description: string;
  confidence: number;
  implication: string;
}

/**
 * Eligibility determination section
 */
export interface EligibilityDetermination {
  overallStatus: 'Eligible' | 'Partially Eligible' | 'Ineligible';
  decisionConfidenceScore: number;
  totalEligibleAmountDetermined: number;
  totalIneligibleAmountDetermined: number;
  totalDocumentAmountReviewed: number;
  rationaleCategory: string;
  rationaleSummary: string;
  requiredActions: string[];
  notesForAuditor: string | null;
}

/**
 * Audit summary section
 */
export interface AuditSummary {
  overallValidationSummary: string;
  keyFindingsSummary: string;
  regulatoryComplianceNotes: string[];
  auditSupportingEvidenceReferences: string[];
}
