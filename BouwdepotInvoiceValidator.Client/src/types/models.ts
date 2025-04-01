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
  isValid: boolean;
  issues: ValidationIssue[];
  extractedInvoice: Invoice | null;
  possibleTampering: boolean;
  isHomeImprovement: boolean;
  rawGeminiResponse: string;
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
