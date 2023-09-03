﻿using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Utilities;
using Microsoft.ML;

namespace BanHub.WebCore.Server.Services;

public class SentimentModelCache : ISentimentModelCache
{
    private readonly ITransformer? _model;

    public SentimentModelCache(Configuration config)
    {
        var context = new MLContext(seed: 1);
        _model = context.Model.Load(Path.GetFullPath(config.SentimentModelDirectory), out _);
    }

    public ITransformer GetModel()
    {
        return _model ?? throw new NullReferenceException("Model is null");
    }
}
