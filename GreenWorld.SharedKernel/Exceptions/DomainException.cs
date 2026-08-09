namespace GreenWorld.SharedKernel.Exceptions;

/// <summary>Base type for all domain-level exceptions.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
