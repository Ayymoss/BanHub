﻿using GlobalInfraction.WebCore.Server.Context;

namespace GlobalInfraction.WebCore.Server.Services;

public class ApiKeyCache
{
    public List<Guid>? ApiKeys { get; set; }
}
