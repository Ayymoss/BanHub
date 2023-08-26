using BanHub.WebCore.Server.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace BanHub.WebCore.Server.Services;

public class SentimentService : ISentimentService
{
    private readonly Configuration _config;
    private readonly MLContext _mlContext;
    private readonly ITransformer? _model;

    public record Message(string Comment);

    public SentimentService(Configuration config)
    {
        _config = config;
        _mlContext = new MLContext(seed: 1);
        _model = _mlContext.Model.Load(Path.GetFullPath(_config.SentimentModelDirectory), out _);
    }

    public float CalculateChatsSentiment(IEnumerable<Message> chats)
    {
        if (_model is null) throw new NullReferenceException("Model is null");
        var transformedData = _model.Transform(_mlContext.Data.LoadFromEnumerable(chats));
        var predictions = _mlContext.Data.CreateEnumerable<MessagePrediction>(transformedData, reuseRowObject: false);
        var messagePredictions = predictions as MessagePrediction[] ?? predictions.ToArray();
        return messagePredictions.Average(p => p.Probability);
    }

    private class MessagePrediction
    {
        [ColumnName("PredictedLabel")] public bool PredictedLabel;
        public float Probability;
    }
}
