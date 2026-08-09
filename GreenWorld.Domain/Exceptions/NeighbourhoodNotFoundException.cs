using GreenWorld.SharedKernel.Exceptions;

namespace GreenWorld.Domain.Exceptions;

public sealed class NeighbourhoodNotFoundException : DomainException
{
    public NeighbourhoodNotFoundException(Guid id)
        : base($"Neighbourhood '{id}' was not found.") { }
}
