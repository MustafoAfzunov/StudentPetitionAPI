using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public PetitionStatus CurrentStatus { get; }

    public PetitionStatus? AttemptedStatus { get; }

    public string Action { get; }

    public InvalidStatusTransitionException(
        string action,
        PetitionStatus currentStatus,
        PetitionStatus? attemptedStatus = null)
        : base(BuildMessage(action, currentStatus, attemptedStatus))
    {
        Action = action;
        CurrentStatus = currentStatus;
        AttemptedStatus = attemptedStatus;
    }

    private static string BuildMessage(
        string action,
        PetitionStatus currentStatus,
        PetitionStatus? attemptedStatus)
    {
        return attemptedStatus.HasValue
            ? $"Cannot {action}: transition from '{currentStatus}' to '{attemptedStatus}' is not allowed."
            : $"Cannot {action}: petition is in '{currentStatus}' status.";
    }
}
