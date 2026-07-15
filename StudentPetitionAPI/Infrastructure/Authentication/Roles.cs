namespace StudentPetitionAPI.Infrastructure.Authentication;

public static class Roles
{
    public const string Student = "Student";
    public const string Reviewer = "Reviewer";
}

public static class Policies
{
    public const string StudentOnly = "StudentOnly";
    public const string ReviewerOnly = "ReviewerOnly";
    public const string CanCreateStudents = "CanCreateStudents";
    public const string CanCreatePetitions = "CanCreatePetitions";
    public const string CanManageOwnPetitions = "CanManageOwnPetitions";
    public const string CanViewPetitions = "CanViewPetitions";
    public const string CanReviewPetitions = "CanReviewPetitions";
    public const string CanViewStudents = "CanViewStudents";
}
