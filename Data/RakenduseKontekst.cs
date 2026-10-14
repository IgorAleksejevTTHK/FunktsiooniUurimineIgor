using FunktsiooniUurimineWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FunktsiooniUurimineWeb.Data;

public class RakenduseKontekst : DbContext
{
    public RakenduseKontekst(DbContextOptions<RakenduseKontekst> options) : base(options)
    {
    }

    public DbSet<FunktsiooniUurimine> FunktsiooniUurimised => Set<FunktsiooniUurimine>();
}
