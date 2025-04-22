
using System;

namespace AssistantNest.Exceptions;

public class AnUserMissingIdClaimException : Exception
{
    public AnUserMissingIdClaimException() : base("User is missing id claim")
    {
    }

    public AnUserMissingIdClaimException(string message) : base(message)
    {
    }

    public AnUserMissingIdClaimException(string message, Exception innerException) : base(message, innerException)
    {
    }
}