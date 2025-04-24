import axios, { AxiosInstance } from 'axios';
import { ValidationResult, ConsolidatedAuditReport } from '../types/models';

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
   * Validates an invoice PDF file
   * @param file The PDF file to validate
   * @returns Promise with both validation result and audit report
   */
  async validateInvoice(file: File): Promise<{
    validationResult: ValidationResult;
    auditReport: ConsolidatedAuditReport;
  }> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await this.api.post<CombinedValidationResponse>(
        '/api/invoice/validate',
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      return {
        validationResult: response.data.validationResult,
        auditReport: response.data.auditReport
      };
    } catch (error) {
      console.error('Error validating invoice:', error);
      throw error;
    }
  }

  /**
   * Health check to verify API connectivity
   * @returns Promise with health status
   */
  async healthCheck(): Promise<{ status: string; timestamp: string }> {
    try {
      const response = await this.api.get<{ status: string; timestamp: string }>(
        '/api/invoice/health'
      );
      return response.data;
    } catch (error) {
      console.error('API health check failed:', error);
      throw error;
    }
  }
  
  /**
   * Gets a consolidated audit report for a validation result
   * @param validationId The ID of the validation result to get an audit report for
   * @returns Promise with the consolidated audit report
   */
  async getAuditReport(validationId: string): Promise<ConsolidatedAuditReport> {
    try {
      const response = await this.api.get<ConsolidatedAuditReport>(
        `/api/invoice/audit-report/${validationId}`
      );
      return response.data;
    } catch (error) {
      console.error('Error getting audit report:', error);
      throw error;
    }
  }

  /**
   * Process additional pages of a PDF for more comprehensive analysis
   * @param validationId The ID of the validation result
   * @param startPage The page number to start processing from (optional, defaults to 2)
   * @returns Promise with the processing result
   */
  async processAdditionalPages(validationId: string, startPage: number = 2): Promise<ProcessAdditionalPagesResponse> {
    try {
      const response = await this.api.post<ProcessAdditionalPagesResponse>(
        `/api/invoice/process-additional-pages/${validationId}`,
        null,
        {
          params: { startPage }
        }
      );
      return response.data;
    } catch (error) {
      console.error('Error processing additional pages:', error);
      throw error;
    }
  }
  
  /**
   * Start a new conversation with Gemini
   * @param metadata Optional metadata for the conversation
   * @returns Promise with the new conversation details
   */
  async startConversation(metadata?: Record<string, string>): Promise<ConversationResponse> {
    try {
      const response = await this.api.post<ConversationResponse>(
        '/api/invoice/conversation/start',
        { metadata }
      );
      return response.data;
    } catch (error) {
      console.error('Error starting conversation:', error);
      throw error;
    }
  }
  
  /**
   * Get information about the current conversation
   * @returns Promise with information about the current conversation
   */
  async getCurrentConversation(): Promise<ConversationResponse> {
    try {
      const response = await this.api.get<ConversationResponse>(
        '/api/invoice/conversation/current'
      );
      return response.data;
    } catch (error) {
      console.error('Error getting current conversation:', error);
      throw error;
    }
  }
  
  /**
   * Switch to an existing conversation
   * @param conversationId ID of the conversation to switch to
   * @returns Promise with information about the switched conversation
   */
  async switchConversation(conversationId: string): Promise<ConversationResponse> {
    try {
      const response = await this.api.post<ConversationResponse>(
        `/api/invoice/conversation/switch/${conversationId}`
      );
      return response.data;
    } catch (error) {
      console.error('Error switching conversation:', error);
      throw error;
    }
  }
  
  /**
   * Clear the message history of the current conversation
   * @returns Promise with information about the cleared conversation
   */
  async clearConversation(): Promise<ConversationResponse> {
    try {
      const response = await this.api.post<ConversationResponse>(
        '/api/invoice/conversation/clear'
      );
      return response.data;
    } catch (error) {
      console.error('Error clearing conversation:', error);
      throw error;
    }
  }
  
  /**
   * Send a prompt to Gemini and get a response
   * @param prompt The text prompt to send
   * @param useHistory Whether to include conversation history (defaults to true)
   * @returns Promise with Gemini's response and conversation details
   */
  async sendPrompt(prompt: string, useHistory: boolean = true): Promise<PromptResponse> {
    try {
      const response = await this.api.post<PromptResponse>(
        '/api/invoice/conversation/prompt',
        { prompt, useHistory }
      );
      return response.data;
    } catch (error) {
      console.error('Error sending prompt:', error);
      throw error;
    }
  }
}

// Create and export a singleton instance
export const apiService = new ApiService();
