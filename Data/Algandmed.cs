using FunktsiooniUurimineWeb.Models;
using FunktsiooniUurimineWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace FunktsiooniUurimineWeb.Data;

public static class Algandmed
{
    public static async Task LooAsync(RakenduseKontekst kontekst)
    {
        if (await kontekst.FunktsiooniUurimised.AnyAsync())
            return;

        var analyys = new FunktsiooniAnalyys();
        var valemid = new[] { "x^3 - 3*x", "x^2 - 4*x + 3" };

        foreach (var valem in valemid)
        {
            var tulemus = analyys.Uuri(valem);
            kontekst.FunktsiooniUurimised.Add(new FunktsiooniUurimine
            {
                Valem = valem,
                Maaramispiirkond = tulemus.Maaramispiirkond,
                Tuletis = tulemus.Tuletis,
                KriitilisedPunktid = tulemus.KriitilisedPunktid,
                Ekstreemumid = tulemus.Ekstreemumid,
                LuodudAeg = DateTime.UtcNow
            });
        }

        await kontekst.SaveChangesAsync();
    }
}
