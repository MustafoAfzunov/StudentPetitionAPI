using StudentPetitionAPI.Domain.Enums;
using StudentPetitionAPI.Application.Exceptions;

namespace StudentPetitionAPI.Application.Services;

/// <summary>
/// Centralizes allowed petition status transitions.
/// Draft → Submitted → UnderReview → Approved | Rejected
/// </summary>
public static class PetitionStatusTransitions
{
    public static bool IsAllowed(PetitionStatus from, PetitionStatus to) =>
        (from, to) switch
        {
            (PetitionStatus.Draft, PetitionStatus.Submitted) => true,
            (PetitionStatus.Submitted, PetitionStatus.UnderReview) => true,
            (PetitionStatus.UnderReview, PetitionStatus.Approved) => true,
            (PetitionStatus.UnderReview, PetitionStatus.Rejected) => true,
            _ => false
        };

    public static void Ensure(
        PetitionStatus from,
        PetitionStatus to,
        string action)
    {
        if (!IsAllowed(from, to))
        {
            throw new InvalidStatusTransitionException(action, from, to);
        }
    }

    public static void EnsureCurrent(
        PetitionStatus currentStatus,
        PetitionStatus requiredStatus,
        string action)
    {
        if (currentStatus != requiredStatus)
        {
            throw new InvalidStatusTransitionException(action, currentStatus);
        }
    }
}
