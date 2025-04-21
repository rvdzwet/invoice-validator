namespace BouwdepotInvoiceValidator.Domain.Models
{
    /// <summary>
    /// Processing information
    /// </summary>
    public class ProcessingInfo
    {
        /// <summary>
        /// Duration of the processing in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// AI models used during processing
        /// </summary>
        public List<string> ModelsUsed { get; set; } = new List<string>();

        /// <summary>
        /// Processing steps performed
        /// </summary>
        public List<string> Steps { get; set; } = new List<string>();
        
        /// <summary>
        /// Detailed processing steps
        /// </summary>
        public List<ProcessingStep> DetailedSteps { get; set; } = new List<ProcessingStep>();
        
        /// <summary>
        /// Detailed AI model usage
        /// </summary>
        public List<AIModelUsageInfo> DetailedModelsUsed { get; set; } = new List<AIModelUsageInfo>();
        
        /// <summary>
        /// Conversation history
        /// </summary>
        public List<ConversationMessage> ConversationHistory { get; set; } = new List<ConversationMessage>();
    }
    
    /// <summary>
    /// Detailed information about a processing step
    /// </summary>
    public class ProcessingStep
    {
        /// <summary>
        /// Name of the step
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the step
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Status of the step
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Timestamp when the step was performed
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
    
    /// <summary>
    /// Detailed information about AI model usage
    /// </summary>
    public class AIModelUsageInfo
    {
        /// <summary>
        /// Name of the model
        /// </summary>
        public string ModelName { get; set; }
        
        /// <summary>
        /// Version of the model
        /// </summary>
        public string ModelVersion { get; set; }
        
        /// <summary>
        /// Operation performed with the model
        /// </summary>
        public string Operation { get; set; }
        
        /// <summary>
        /// Token count used
        /// </summary>
        public int TokenCount { get; set; }
        
        /// <summary>
        /// Timestamp when the model was used
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
    
    /// <summary>
    /// Message in a conversation with the LLM
    /// </summary>
    public class ConversationMessage
    {
        /// <summary>
        /// Role of the message sender (user or model)
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Content of the message
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Step that generated this message
        /// </summary>
        public string StepName { get; set; }
        
        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}
