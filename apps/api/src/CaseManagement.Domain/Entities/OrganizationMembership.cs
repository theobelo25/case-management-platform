namespace CaseManagement.Domain.Entities;

public enum OrganizationRole
{
    Owner = 1,
    Admin = 2,
    Member = 3
}

public sealed class OrganizationMembership
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public OrganizationRole Role { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private OrganizationMembership() { }

    public OrganizationMembership(
        Guid organizationId, 
        Guid userId, 
        OrganizationRole role)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeRole(OrganizationRole role)
    {
        Role = role;

        Touched();
    }

    private void Touched()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}