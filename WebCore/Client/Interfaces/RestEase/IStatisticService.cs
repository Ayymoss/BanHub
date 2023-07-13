using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IStatisticService
{
    [Get("/Statistic")]
    Task<HttpResponseMessage> GetStatisticAsync();
}
