
using System;

namespace AssistantNest.Exceptions;

public class AnUserUpdateFailedException : Exception
{
    public AnUserUpdateFailedException(string message) : base(message)
    {
    }

    public AnUserUpdateFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}