namespace DirectoryService.Domain.Shared;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; }

    void Delete();

    void Restore();
}