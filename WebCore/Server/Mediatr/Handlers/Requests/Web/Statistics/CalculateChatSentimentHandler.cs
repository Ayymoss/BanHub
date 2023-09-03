using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Handlers.Events.Services;
using MediatR;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Statistics;

public class CalculateChatSentimentHandler : IRequestHandler<CalculateChatSentimentCommand, float>
{
    private readonly ITransformer _model;
    private readonly MLContext _mlContext;

    public CalculateChatSentimentHandler(ISentimentModelCache sentimentModelCache)
    {
        _model = sentimentModelCache.GetModel();
        _mlContext = new MLContext(seed: 1);
    }

    public Task<float> Handle(CalculateChatSentimentCommand request, CancellationToken cancellationToken)
    {
        var transformedData = _model.Transform(_mlContext.Data.LoadFromEnumerable(request.Messages));
        var predictions = _mlContext.Data.CreateEnumerable<MessagePrediction>(transformedData, reuseRowObject: false);
        var messagePredictions = predictions as MessagePrediction[] ?? predictions.ToArray();
        return Task.FromResult(messagePredictions.Average(p => p.Probability));
    }

    private class MessagePrediction
    {
        [ColumnName("PredictedLabel")] public bool PredictedLabel;
        public float Probability;
    }
}
