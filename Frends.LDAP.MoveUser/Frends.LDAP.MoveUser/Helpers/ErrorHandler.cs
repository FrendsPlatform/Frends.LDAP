using System;
using System.Runtime.ExceptionServices;
using Frends.LDAP.MoveUser.Definitions;

namespace Frends.LDAP.MoveUser.Helpers;

internal static class ErrorHandler
{
    /// <summary>
    /// Converts an exception into a failed Result object or rethrows based on task options.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="options"> Task options that control whether failures are returned as a Result object or thrown. </param>
    /// <returns> A failed Result object when the exception is handled instead of rethrown. </returns>
    internal static Result Handle(this Exception exception, Options options)
    {
        if (exception is OperationCanceledException)
            ExceptionDispatchInfo.Capture(exception).Throw();

        if (options.ThrowErrorOnFailure) ThrowBaseException(exception, options.ErrorMessageOnFailure);

        return ReturnResult(exception, options.ErrorMessageOnFailure);
    }

    private static void ThrowBaseException(Exception exception, string customMessage = null)
    {
        if (string.IsNullOrEmpty(customMessage))
            ExceptionDispatchInfo.Capture(exception).Throw();

        throw new Exception(customMessage, exception);
    }

    private static Result ReturnResult(Exception exception, string customMessage = null)
    {
        var errorMessage = string.IsNullOrEmpty(customMessage)
            ? exception.Message
            : $"{customMessage}: {exception.Message}";

        return new Result
        {
            Success = false,
            Error = new Error
            {
                Message = errorMessage,
                AdditionalInfo = exception,
            },
        };
    }
}
