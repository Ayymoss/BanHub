﻿using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class GetPenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<Shared.Models.PenaltiesView.Penalty>>
{
    public bool Privileged { get; set; }
}
