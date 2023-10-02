using Microsoft.ML;

namespace BanHub.Application.Interfaces.Services;

public interface ISentimentModelCache
{
    ITransformer GetModel();
}
