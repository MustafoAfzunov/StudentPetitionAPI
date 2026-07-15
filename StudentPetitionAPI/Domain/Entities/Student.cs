namespace StudentPetitionAPI.Domain.Entities;

public class Student : IEntity
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string StudentNumber { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<Petition> Petitions { get; set; } = new List<Petition>();
}
