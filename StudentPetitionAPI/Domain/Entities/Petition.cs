using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Domain.Entities;

public class Petition : IEntity
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public PetitionType PetitionType { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public PetitionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewComment { get; set; }
}
