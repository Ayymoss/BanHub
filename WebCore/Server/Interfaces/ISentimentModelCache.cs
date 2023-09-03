using Microsoft.ML;

namespace BanHub.WebCore.Server.Interfaces;

public interface ISentimentModelCache
{
    ITransformer GetModel();
}
