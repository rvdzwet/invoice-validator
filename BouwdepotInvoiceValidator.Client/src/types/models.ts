/**
 * Represents an invoice with all extracted data
 */
export interface Invoice {
  invoiceNumber: string;
  invoiceDate: string | null; // Will be converted from DateTime
  totalAmount: number;
  vatAmount: number;
  vendorName: string;
  vendorAddress: string;
  vendorKvkNumber: string;
  vendorBtwNumber: string;
  rawText: string;
  lineItems: InvoiceLineItem[];
  
  // Original file metadata
  fileName: string;
  fileSizeBytes: number;
  fileCreationDate: string | null; // Will be converted from DateTime
  fileModificationDate: string | null; // Will be converted from DateTime
  
  // Multi-modal validation properties
  pageImages?: InvoicePageImage[];
  visualAnalysis?: VisualAnalysisResult;
  documentStructure?: DocumentStructure;
}

/**
 * Represents an image of a page from the invoice PDF
 */
export interface InvoicePageImage {
  pageNumber: number;
  base64EncodedImage: string;
  textBoundingBoxes: Record<string, Rectangle>;
}

/**
 * Results of visual analysis of the invoice
 */
export interface VisualAnalysisResult {
  hasLogo: boolean;
  hasSignature: boolean;
  hasStamp: boolean;
  hasTableStructure: boolean;
  identifiedElements: Record<string, Rectangle>;
  detectedAnomalies: VisualAnomalyDetection[];
}

/**
 * Represents a visual anomaly detected in the invoice
 */
export interface VisualAnomalyDetection {
  anomalyType: string;
  description: string;
  location: Rectangle;
  confidence: number;
  evidenceDescription: string;
}

/**
 * Represents the structure of the invoice document
 */
export interface DocumentStructure {
  hasHeader: boolean;
  hasFooter: boolean;
  hasTableOfLineItems: boolean;
  hasLogo: boolean;
  hasSignature: boolean;
  elementPositions: Record<string, Rectangle>;
}

/**
 * Represents a rectangle with position and size
 */
export interface Rectangle {
  x: number;
  y: number;
  width: number;
  height: number;
}

/**
 * Represents a single line item in an invoice
 */
export interface InvoiceLineItem {
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  vatRate: number;
}

/**
 * Represents the result of an invoice validation process
 */
export interface ValidationResult {
  validationId?: string;
  isValid: boolean;
  issues: ValidationIssue[];
  extractedInvoice: Invoice | null;
  possibleTampering: boolean;
  isHomeImprovement: boolean;
  
  // Updated API response property
  rawAIResponse: string;
  rawGeminiResponse?: string; // Kept for backward compatibility
  
  // Confidence and approval status
  confidenceScore: number;
  meetsApprovalThreshold: boolean;
  claimantId?: string;
  validatedAt?: string; // ISO date string
  
  // Content summary properties
  summary?: ContentSummary;
  confidenceFactors?: ConfidenceFactor[];
  
  // Document verification properties
  isVerifiedInvoice?: boolean;
  detectedDocumentType?: string;
  documentVerificationConfidence?: number;
  presentInvoiceElements?: string[];
  missingInvoiceElements?: string[];
  
  // Bouwdepot validation properties
  isBouwdepotCompliant?: boolean;
  isVerduurzamingsdepotCompliant?: boolean;
  bouwdepotValidation?: BouwdepotValidationDetails;
  
  // Purchase analysis properties
  purchaseAnalysis?: PurchaseAnalysis;
  
  // Fraud detection properties
  fraudDetection?: FraudDetection;
  
  // Vendor insights
  vendorInsights?: VendorInsights;
  
  // Audit transparency properties
  auditReport?: AuditReport;
  signature?: DigitalSignature;
  
  // Backward compatibility properties (deprecated)
  auditInfo?: AuditMetadata;
  criteriaAssessments?: CriterionAssessment[];
  fraudIndicatorAssessments?: IndicatorAssessment[];
  visualAssessments?: VisualAssessment[];
  auditJustification?: string;
  weightedScore?: number;
  regulatoryNotes?: string;
  fraudRiskCategory?: string;
  approvalFactors?: string[];
  rejectionFactors?: string[];
  metRules?: string[];
  violatedRules?: string[];
  recommendedActions?: string[];
}

/**
 * Contains details about Bouwdepot rules validation
 */
export interface BouwdepotValidationDetails {
  // Core validation properties
  qualityImprovementRule: boolean;
  permanentAttachmentRule: boolean;
  generalValidationNotes?: string;
  
  // Line item validations
  lineItemValidations: BouwdepotLineItemValidation[];
  
  // Verduurzamingsdepot-specific properties
  isVerduurzamingsdepotItem: boolean;
  meetsVerduurzamingsdepotCriteria: boolean;
  matchedVerduurzamingsdepotCategories: string[];
}

/**
 * Validation details for a specific line item
 */
export interface BouwdepotLineItemValidation {
  description: string;
  isPermanentlyAttached: boolean;
  improveHomeQuality: boolean;
  violatedRules: string[];
  validationNotes?: string;
  isVerduurzamingsdepotItem?: boolean;
  verduurzamingsdepotCategory?: string;
}

/**
 * Detailed content summary of what was purchased and why
 */
export interface ContentSummary {
  purchasedItems: string;
  intendedPurpose: string;
  propertyImpact: string;
  projectCategory: string;
  estimatedProjectScope: string;
  likelyPartOfLargerProject?: boolean;
  projectStage?: string;
}

/**
 * A factor that contributed to the confidence score
 */
export interface ConfidenceFactor {
  factorName: string;
  impact: number;
  explanation: string;
}

/**
 * Analysis of what was purchased in the invoice
 */
export interface PurchaseAnalysis {
  summary: string;
  primaryPurpose: string;
  categories: string[];
  homeImprovementPercentage: number;
  lineItemDetails: LineItemAnalysisDetail[];
}

/**
 * Detailed analysis of a specific line item
 */
export interface LineItemAnalysisDetail {
  description: string;
  interpretedAs: string;
  category: string;
  isHomeImprovement: boolean;
  confidence: number;
  notes: string;
}

/**
 * Comprehensive fraud detection result
 */
export interface FraudDetection {
  // Overall fraud risk score (0-100)
  fraudRiskScore: number;
  
  // Risk level assessment
  riskLevel: FraudRiskLevel;
  
  // List of detected fraud indicators
  detectedIndicators: FraudIndicator[];
  
  // Recommended action based on fraud risk
  recommendedAction: string;
  
  // Whether manual review is required
  requiresManualReview?: boolean;
  
  // Suggested verification steps
  suggestedVerificationSteps?: string[];
}

/**
 * Fraud risk level enumeration
 */
export enum FraudRiskLevel {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

/**
 * Detailed fraud indicator with evidence and severity
 */
export interface FraudIndicator {
  indicatorId?: string;
  indicatorName: string;
  description: string;
  severity: number; // 0-1 scale
  category: FraudIndicatorCategory;
  evidence: string;
  affectedElements?: string[];
  confidence?: number;
}

/**
 * Categories of fraud indicators
 */
export enum FraudIndicatorCategory {
  DocumentManipulation = 0,
  ContentInconsistency = 1,
  AnomalousPricing = 2,
  VendorIssue = 3,
  HistoricalPattern = 4,
  DigitalArtifact = 5,
  ContextualMismatch = 6,
  BehavioralFlag = 7
}

/**
 * Digital signature for verification
 */
export interface DigitalSignature {
  signatureValue: string;
  signedAt: string; // ISO date string
  algorithm: string;
  signedFields: string;
  signerId: string;
  verificationStatus: SignatureVerificationStatus;
}

/**
 * Status of digital signature verification
 */
export enum SignatureVerificationStatus {
  NotVerified = 0,
  Valid = 1,
  Invalid = 2,
  CryptoError = 3
}

/**
 * Insights about the vendor from profiling
 */
export interface VendorInsights {
  vendorName: string;
  businessCategories: string[];
  invoiceCount: number;
  firstSeen: string; // ISO date string
  lastSeen: string; // ISO date string
  reliabilityScore: number;
  priceStabilityScore: number;
  documentQualityScore: number;
  unusualServicesDetected: boolean;
  unreasonablePricesDetected: boolean;
  totalAnomalyCount: number;
  vendorSpecialties: string[];
}

/**
 * Comprehensive audit report for transparency
 */
export interface AuditReport {
  auditIdentifier: string;
  timestampUtc: string; // ISO date string
  modelVersion: string;
  validatorVersion: string;
  documentHash: string;
  documentSource: string;
  verificationTests: string[];
  executiveSummary: string;
  ruleAssessments: RuleAssessment[];
  applicableRegulations: RegulationReference[];
  keyMetrics: Record<string, number>;
  appliedThresholds: ValidationThresholdConfiguration;
  approvalFactors: string[];
  concernFactors: string[];
  processingEvents: ProcessingEvent[];
}

/**
 * Assessment of a specific rule
 */
export interface RuleAssessment {
  ruleId: string;
  ruleName: string;
  description: string;
  isSatisfied: boolean;
  evidence: string;
  reasoning: string;
  weight: number;
  score: number;
}

/**
 * Reference to an applicable regulation
 */
export interface RegulationReference {
  referenceCode: string;
  description: string;
  applicabilityExplanation: string;
  complianceStatus: string;
}

/**
 * Record of a processing event in the validation pipeline
 */
export interface ProcessingEvent {
  timestamp: string; // ISO date string
  eventType: string;
  component: string;
  description: string;
  parameters: Record<string, string>;
}

/**
 * Configuration for validation thresholds
 */
export interface ValidationThresholdConfiguration {
  autoApprovalThreshold: number;
  highRiskThreshold: number;
  enableAutoApproval: boolean;
  lastModifiedDate: string; // ISO date string
}

/**
 * Metadata for audit trails
 */
export interface AuditMetadata {
  timestampUtc: string; // Will be converted from DateTime
  frameworkVersion: string;
  modelVersion: string;
}

/**
 * Assessment of a specific validation criterion
 */
export interface CriterionAssessment {
  criterionName: string;
  weight: number;
  evidence: string;
  score: number;
  confidence: number;
  reasoning: string;
}

/**
 * Assessment of a specific fraud indicator
 */
export interface IndicatorAssessment {
  indicatorName: string;
  weight: number;
  evidence: string;
  concernLevel: number;
  regulatoryReference: string;
  confidence: number;
  reasoning: string;
}

/**
 * Assessment of a visual element in the invoice
 */
export interface VisualAssessment {
  elementName: string;
  weight: number;
  evidence: string;
  score: number;
  confidence: number;
  reasoning: string;
  location?: Rectangle;
}

/**
 * Represents an issue found during validation
 */
export interface ValidationIssue {
  severity: ValidationSeverity;
  message: string;
}

/**
 * Enum representing the severity of a validation issue
 */
export enum ValidationSeverity {
  Info = 0,
  Warning = 1,
  Error = 2
}

/**
 * Represents a consolidated audit report with simplified structure
 */
export interface ConsolidatedAuditReport {
  // Basic Invoice Information
  invoiceNumber: string;
  invoiceDate: string; // Will be converted from DateTime
  totalAmount: number;
  
  // Vendor Information
  vendorName: string;
  vendorAddress: string;
  bankDetails: VendorBankDetails;
  
  // Overall Assessment (Summary)
  isApproved: boolean;
  overallScore: number;
  summaryText: string;
  
  // Validation Results (Simplified)
  documentValidation: DocumentValidation;
  bouwdepotEligibility: BouwdepotEligibility;
  lineItems: ValidatedLineItem[];
  approvedItems: ValidatedLineItem[];
  reviewItems: ValidatedLineItem[];
  
  // Detailed Analysis Sections
  detailedAnalysis: DetailedAnalysisSection[];
  
  // Audit Trail (Compliant with European law)
  auditInformation: AuditTrail;
  
  // Optional Technical Details (expandable)
  technicalDetails: TechnicalDetails;
}

/**
 * Vendor bank account details for payment processing
 */
export interface VendorBankDetails {
  bankName: string;
  accountNumber: string;
  iban: string;
  bic: string;
  paymentReference: string;
  isVerified: boolean;
}

/**
 * Document validation results in a simplified format
 */
export interface DocumentValidation {
  isValid: boolean;
  score: number;
  primaryReason: string;
  validationDetails: string[];
}

/**
 * Bouwdepot eligibility assessment in a simplified format
 */
export interface BouwdepotEligibility {
  isEligible: boolean;
  score: number;
  meetsQualityImprovement: boolean;
  meetsPermanentAttachment: boolean;
  constructionType: string;
  paymentPriority: string;
  specialConditions: string;
}

/**
 * Represents a validated line item with simplified structure
 */
export interface ValidatedLineItem {
  description: string;
  amount: number;
  isApproved: boolean;
  validationNote: string;
}

/**
 * Court-admissible audit trail that complies with European legal requirements
 */
export interface AuditTrail {
  // Core audit information
  auditIdentifier: string;
  validationTimestamp: string; // Will be converted from DateTime
  validationSystemVersion: string;
  
  // User and system information
  validatedBy: string;
  validatorIdentifier: string;
  validatorRole: string;
  systemIPAddress: string;
  
  // Document integrity
  originalDocumentHash: string;
  documentSourceIdentifier: string;
  
  // Decision and reasoning
  conclusionExplanation: string;
  keyFactors: string[];
  
  // Legal references and compliance
  appliedRules: RuleApplication[];
  legalBasis: LegalReference[];
  
  // Chain of custody
  processingSteps: ProcessingStep[];
  
  // Digital signature
  auditSignature: string;
  signatureCertificateThumbprint: string;
}

/**
 * Reference to a specific legal or regulatory basis for a decision
 */
export interface LegalReference {
  referenceCode: string;
  description: string;
  applicability: string;
}

/**
 * Represents a step in the processing chain with timing information
 */
export interface ProcessingStep {
  timestamp: string; // Will be converted from DateTime
  processName: string;
  processorIdentifier: string;
  inputState: string;
  outputState: string;
}

/**
 * Documents the application of a specific rule to an invoice
 */
export interface RuleApplication {
  ruleId: string;
  ruleName: string;
  ruleVersion: string;
  ruleDescription: string;
  isSatisfied: boolean;
  applicationResult: string;
  evidenceReference: string;
}

/**
 * Technical details for advanced users or debugging
 */
export interface TechnicalDetails {
  detailedMetrics: Record<string, string>;
  processingNotes: string[];
}

/**
 * Detailed analysis section with name and key-value pairs
 */
export interface DetailedAnalysisSection {
  sectionName: string;
  analysisItems: Record<string, string>;
}
