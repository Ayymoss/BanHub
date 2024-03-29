﻿using BanHub.Application.DTOs.WebView.CommunityProfileView;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Domain.ValueObjects.Services;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;
using Community = BanHub.Application.DTOs.WebView.CommunitiesView.Community;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class CommunityService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly ICommunityService _api = RestClient.For<ICommunityService>(ApiHost);

    public async Task<PaginationContext<Community>> GetCommunitiesPaginationAsync(GetCommunitiesPaginationCommand communitiesPagination)
    {
        try
        {
            var response = await _api.GetCommunitiesPaginationAsync(communitiesPagination);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Community>>();
            return result ?? new PaginationContext<Community>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new PaginationContext<Community>();
    }

    public async Task<Application.DTOs.WebView.CommunityProfileView.Community> GetCommunityAsync(string identity)
    {
        try
        {
            var response = await _api.GetCommunityAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<Application.DTOs.WebView.CommunityProfileView.Community>();
            return result ?? new Application.DTOs.WebView.CommunityProfileView.Community();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new Application.DTOs.WebView.CommunityProfileView.Community();
    }

    public async Task<PaginationContext<Server>> GetCommunityProfileServersPaginationAsync(
        GetCommunityProfileServersPaginationCommand pagination)
    {
        try
        {
            var response = await _api.GetCommunityProfileServersPaginationAsync(pagination);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Server>>();
            return result ?? new PaginationContext<Server>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new PaginationContext<Server>();
    }

    public async Task<bool> ToggleCommunityActivationAsync(string identity)
    {
        try
        {
            var response = await _api.ToggleCommunityActivationAsync(identity);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to toggle instance activation: {e.Message}");
        }

        return false;
    }

    public async Task<PaginationContext<Penalty>> GetCommunityProfilePenaltiesPaginationAsync(
        GetCommunityProfilePenaltiesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetCommunityProfilePenaltiesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Penalty>>();
            return result ?? new PaginationContext<Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get community penalties: {e.Message}");
        }

        return new PaginationContext<Penalty>();
    }
}
