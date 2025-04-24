import axios, { AxiosInstance } from 'axios';
import { ValidationResult, ConsolidatedAuditReport } from '../types/models';
import { ComprehensiveWithdrawalProofResponse } from '../types/comprehensiveModels';

// Define interfaces for API requests

/**
 * Response with conversation details
 */
interface ConversationResponse {
  conversationId: string;
  status: string;
  messageCount: number;
  createdAt: string;
  lastUpdatedAt: string;
  metadata?: Record<string, string>;
}


/**
 * Response from Gemini with conversational context
 */
interface PromptResponse {
  response: string;
  conversationId: string;
  messageCount: number;
  lastUpdatedAt: string;
}

/**
 * Response from processing additional pages
 */
interface ProcessAdditionalPagesResponse {
  success: boolean;
  message: string;
  pagesProcessed: number;
}

/**
 * Combined response for validation and audit report
 */
interface CombinedValidationResponse {
  validationResult: ValidationResult;
  auditReport: ConsolidatedAuditReport;
}

/**
 * Response for comprehensive withdrawal validation
 */
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

/**
 * Service for API communication with the backend
 */
class ApiService {
  private api: AxiosInstance;
  private baseUrl: string;

  /**
   * Constructor
   */
  constructor() {
    // API base URL - use environment variable in production
    this.baseUrl = '';
    
    // Create axios instance with default config
    this.api = axios.create({
      baseURL: this.baseUrl,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  /**
   * Health check to verify API connectivity
   * @returns Promise with health status
   */
  async healthCheck(): Promise<{ status: string; timestamp: string }> {
    try {
      const response = await this.api.get<{ status: string; timestamp: string }>(
        '/api/health'
      );
      return response.data;
    } catch (error) {
      console.error('API health check failed:', error);
      throw error;
    }
  }
  
/**
   * Validates a withdrawal proof document (PDF, image)
   * @param file The document file to validate
   * @returns Promise with validation context including comprehensive validation result
   */
  async validateWithdrawalProof(file: File): Promise<ValidationContext> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      // Use the case-sensitive endpoint that matches the controller route
      const response = await this.api.post<ValidationContext>(
        '/api/WithdrawalProof/validate',
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      console.log('Withdrawal proof API response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error validating withdrawal proof:', error);
      throw error;
    }
  }
  
  /**
   * Validates an invoice document (PDF)
   * @param file The invoice file to validate
   * @returns Promise with validation result and audit report
   */
  async validateInvoice(file: File): Promise<CombinedValidationResponse> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await this.api.post<CombinedValidationResponse>(
        '/api/Invoice/validate',
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      console.log('Invoice validation API response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error validating invoice:', error);
      throw error;
    }
  }
  
}

// Create and export a singleton instance
export const apiService = new ApiService();
