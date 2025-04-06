namespace BouwdepotValidationValidator.Infrastructure.Abstractions
{
    /// <summary>
    /// Interface for clients that interact with the Gemini API
    /// </summary>
    public interface ILLMProvider
    {
        /// <summary>
        /// Sends a text-only prompt to the Gemini API
        /// </summary>
        /// <param name="prompt">The text prompt to send</param>
        /// <returns>The response from the API</returns>
        Task<string> SendTextPromptAsync(string prompt);

        /// <summary>
        /// Sends a multimodal prompt (text + images) to the Gemini API
        /// </summary>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="imageStreams">The image streams to include in the prompt</param>
        /// <param name="mimeTypes">The MIME types of the images</param>
        /// <returns>The response from the API</returns>
        Task<string> SendMultimodalPromptAsync(
            string prompt, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes);

        /// <summary>
        /// Sends a structured prompt to the Gemini API and gets a structured response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request object</typeparam>
        /// <typeparam name="TResponse">The type of the response object</typeparam>
        /// <param name="prompt">The text prompt to send</param>
        /// <param name="requestObject">The structured request object</param>
        /// <returns>The structured response from the API</returns>
        Task<TResponse> SendStructuredPromptAsync<TRequest, TResponse>(
            string prompt, 
            TRequest requestObject)
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
        /// <returns>The structured response from the API</returns>
        Task<TResponse> SendMultimodalStructuredPromptAsync<TRequest, TResponse>(
            string prompt, 
            TRequest requestObject, 
            IEnumerable<Stream> imageStreams, 
            IEnumerable<string> mimeTypes)
            where TRequest : class
            where TResponse : class, new();
    }
}
