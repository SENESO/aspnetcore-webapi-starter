using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => await _db.Users.AnyAsync(u => u.Username == username, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _db.Users.AnyAsync(u => u.Email == email, ct);
}
