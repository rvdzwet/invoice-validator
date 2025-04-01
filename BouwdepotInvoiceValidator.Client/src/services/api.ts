import axios, { AxiosInstance } from 'axios';
import { ValidationResult } from '../types/models';

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
    this.baseUrl = 'https://localhost:7051';
    
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
   * @returns Promise with validation result
   */
  async validateInvoice(file: File): Promise<ValidationResult> {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await this.api.post<ValidationResult>(
        '/api/invoice/validate',
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );

      return response.data;
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
}

// Create and export a singleton instance
export const apiService = new ApiService();
