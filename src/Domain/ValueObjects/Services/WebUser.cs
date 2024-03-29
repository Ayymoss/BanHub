﻿namespace BanHub.Domain.ValueObjects.Services;

public class WebUser
{
    public string UserName { get; set; }
    public string WebRole { get; set; }
    public string CommunityRole { get; set; }
    public string Identity { get; set; }
    public string SignedInGuid { get; set; }
}
