using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BouwdepotInvoiceValidator.Domain.Models.AdvancedDocumentAnalysis
{
    public class AdvancedDocumentAnalysisOutput
    {
        [JsonPropertyName("language")]
        public LanguageAnalysis Language { get; set; }

        [JsonPropertyName("sentiment")]
        public SentimentAnalysis Sentiment { get; set; }

        [JsonPropertyName("topics")]
        public List<TopicAnalysis> Topics { get; set; }

        [JsonPropertyName("entities")]
        public List<EntityRecognition> Entities { get; set; }

        [JsonPropertyName("documentType")]
        public DocumentTypeAnalysis DocumentType { get; set; }
    }

    public class DocumentTypeAnalysis
    {
        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }

    public class LanguageAnalysis
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }

    public class SentimentAnalysis
    {
        [JsonPropertyName("overall")]
        public string Overall { get; set; }

        [JsonPropertyName("positive")]
        public int Positive { get; set; }

        [JsonPropertyName("negative")]
        public int Negative { get; set; }

        [JsonPropertyName("neutral")]
        public int Neutral { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }

    public class TopicAnalysis
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }

    public class EntityRecognition
    {
        [JsonPropertyName("entity")]
        public string Entity { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }
}
