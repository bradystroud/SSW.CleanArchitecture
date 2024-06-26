﻿using Microsoft.EntityFrameworkCore;
using SSW.CleanArchitecture.Application.Common.Exceptions;
using SSW.CleanArchitecture.Application.Common.Interfaces;
using SSW.CleanArchitecture.Domain.Heroes;

namespace SSW.CleanArchitecture.Application.Features.Heroes.Commands.UpdateHero;

public sealed record UpdateHeroCommand(
    HeroId HeroId,
    string Name,
    string Alias,
    IEnumerable<UpdateHeroPowerDto> Powers) : IRequest<Guid>;

// ReSharper disable once UnusedType.Global
public sealed class UpdateHeroCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateHeroCommand, Guid>
{
    public async Task<Guid> Handle(UpdateHeroCommand request, CancellationToken cancellationToken)
    {
        var hero = await dbContext.Heroes
            .Include(h => h.Powers)
            .FirstOrDefaultAsync(h => h.Id == request.HeroId, cancellationToken);

        if (hero is null)
        {
            throw new NotFoundException(nameof(Hero), request.HeroId);
        }
        
        hero.UpdateName(request.Name);
        hero.UpdateAlias(request.Alias);
        var powers = request.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        await dbContext.SaveChangesAsync(cancellationToken);

        return hero.Id.Value;
    }
}