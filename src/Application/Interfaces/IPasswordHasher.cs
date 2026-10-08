namespace Application.Interfaces;

// Password hashing contract. Lives in Application so services
// never depend on Infrastructure directly.
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
