﻿using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Community;

public class GetCommunitiesPaginationCommand : Pagination, IRequest<IEnumerable<Models.CommunitiesView.Community>>
{
    public bool Privileged { get; set; }
}