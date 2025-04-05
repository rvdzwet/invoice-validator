using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BouwdepotInvoiceValidator.Infrastructure.Gemini.Services
{
    /// <summary>
    /// Interface for clients that interact with the Gemini API
    /// </summary>
    public interface IGeminiClient
    {
        /// <summary>
        /// Sends a text-only prompt to the Gemini API
        /// </summary>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="modelName">The name of the model to use (e.g., "gemini-pro")</param>
        /// <returns>The response from the API</returns>
        Task<string> SendTextPromptAsync(string prompt, string modelName = "gemini-pro");

        /// <summary>
        /// Sends a multimodal prompt (text + images) to the Gemini API
        /// </summary>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="imageStreams">The image streams to include in the prompt</param>
        /// <param name="mimeTypes">The MIME types of the images</param>
        /// <param name="modelName">The name of the model to use (e.g., "gemini-pro-vision")</param>
        /// <returns>The response from the API</returns>
        Task<string> SendMultimodalPromptAsync(
            string prompt, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes, 
            string modelName = "gemini-pro-vision");

        /// <summary>
        /// Sends a structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object</typeparam>
        /// <typeparam name="TResponse">The type of the response object</typeparam>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="requestObject">The structured request object</param>
        /// <param name="modelName">The name of the model to use (e.g., "gemini-pro")</param>
        /// <returns>The structured response from the API</returns>
        Task<TResponse> SendStructuredPromptAsync<TRequest, TResponse>(
            string prompt, 
            TRequest requestObject, 
            string modelName = "gemini-pro")
            where TRequest : class
            where TResponse : class, new();

        /// <summary>
        /// Sends a multimodal structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object</typeparam>
        /// <typeparam name="TResponse">The type of the response object</typeparam>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="requestObject">The structured request object</param>
        /// <param name="imageStreams">The image streams to include in the prompt</param>
        /// <param name="mimeTypes">The MIME types of the images</param>
        /// <param name="modelName">The name of the model to use (e.g., "gemini-pro-vision")</param>
        /// <returns>The structured response from the API</returns>
        Task<TResponse> SendMultimodalStructuredPromptAsync<TRequest, TResponse>(
            string prompt, 
            TRequest requestObject, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes, 
            string modelName = "gemini-pro-vision")
            where TRequest : class
            where TResponse : class, new();
    }
}
