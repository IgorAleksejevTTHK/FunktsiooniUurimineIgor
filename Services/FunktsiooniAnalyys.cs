using System.Globalization;
using System.Text;

namespace FunktsiooniUurimineWeb.Services;

public sealed record AnalüüsiTulemus(
    string Maaramispiirkond,
    string Nullkohad,
    string Tuletis,
    string KriitilisedPunktid,
    string Ekstreemumid);

public sealed class FunktsiooniAnalyys
{
    public AnalüüsiTulemus Uuri(string valem)
    {
        var avaldis = FunktsiooniParser.Parse(Normaliseeri(valem));
        var tuletis = avaldis.Tuleta();
        var nullkohad = LeiaNullkohad(avaldis);
        var kriitilised = LeiaNullkohad(tuletis);

        var nullText = nullkohad.Count == 0
            ? "Nullkohti ei leitud vahemikus [-100; 100]."
            : string.Join(", ", nullkohad.Select((x, i) => $"x{i + 1} = {Formaat(x)}"));

        var kpText = kriitilised.Count == 0
            ? "Kriitilisi punkte ei leitud vahemikus [-100; 100]."
            : string.Join(", ", kriitilised.Select((x, i) => $"x{i + 1} = {Formaat(x)}"));

        var ekstreemumid = LeiaEkstreemumid(avaldis, tuletis, kriitilised);
        return new AnalüüsiTulemus(
            LeiaMaaramispiirkond(valem, avaldis),
            nullText,
            $"f'(x) = {PuhastaTuletis(tuletis.Tekst())}",
            kpText,
            ekstreemumid);
    }

    private static string Normaliseeri(string valem)
    {
        return valem.Trim().Replace(',', '.').Replace("∞", "inf");
    }

    private static string LeiaMaaramispiirkond(string valem, Avaldis avaldis)
    {
        var piirangud = new List<double>();
        LeiaNullkohadPiirajaTestiks(avaldis, piirangud);
        piirangud = piirangud.DistinctBy(x => Math.Round(x, 7)).OrderBy(x => x).ToList();
        if (piirangud.Count == 0)
            return "X = (-∞; ∞) ehk kõik reaalarvud.";
        return "X = (-∞; ∞), välja arvatud määramata punktid: " + string.Join(", ", piirangud.Select(x => "x ≠ " + Formaat(x)));
    }

    private static void LeiaNullkohadPiirajaTestiks(Avaldis avaldis, List<double> piirangud)
    {
        switch (avaldis)
        {
            case Funktsioon f when f.Nimi is "ln" or "log":
                piirangud.AddRange(LeiaNullkohad(f.Arg).Where(x => double.IsFinite(x)));
                break;
            case Kahend b when b.Operatsioon == "/":
                piirangud.AddRange(LeiaNullkohad(b.Parem));
                LeiaNullkohadPiirajaTestiks(b.Vasak, piirangud);
                LeiaNullkohadPiirajaTestiks(b.Parem, piirangud);
                break;
            case Kahend b:
                LeiaNullkohadPiirajaTestiks(b.Vasak, piirangud);
                LeiaNullkohadPiirajaTestiks(b.Parem, piirangud);
                break;
            case Unaarsed u:
                LeiaNullkohadPiirajaTestiks(u.Arg, piirangud);
                break;
        }
    }

    private static List<double> LeiaNullkohad(Avaldis f, double algus = -100, double lopp = 100, double samm = 0.25)
    {
        var tulem = new List<double>();
        double x0 = algus;
        double y0 = Turvaline(f, x0);
        for (double x1 = algus + samm; x1 <= lopp + 1e-9; x1 += samm)
        {
            var y1 = Turvaline(f, x1);
            if (double.IsFinite(y0) && Math.Abs(y0) < 1e-8) LisaUnikaalne(tulem, x0);
            if (double.IsFinite(y0) && double.IsFinite(y1) && y0 * y1 < 0)
            {
                var juur = Bisektsioon(f, x0, x1);
                if (double.IsFinite(juur)) LisaUnikaalne(tulem, juur);
            }
            x0 = x1; y0 = y1;
        }
        if (double.IsFinite(y0) && Math.Abs(y0) < 1e-8) LisaUnikaalne(tulem, x0);
        return tulem.OrderBy(x => x).ToList();
    }

    private static string LeiaEkstreemumid(Avaldis f, Avaldis fp, List<double> kriitilised)
    {
        if (kriitilised.Count == 0) return "Ekstreemume ei leitud.";
        var tulem = new List<string>();
        foreach (var x in kriitilised)
        {
            const double h = 0.001;
            var vasak = Turvaline(fp, x - h);
            var parem = Turvaline(fp, x + h);
            var y = Turvaline(f, x);
            if (!double.IsFinite(y)) continue;

            if (vasak > 0 && parem < 0)
                tulem.Add($"max: x = {Formaat(x)}, y = {Formaat(y)}");
            else if (vasak < 0 && parem > 0)
                tulem.Add($"min: x = {Formaat(x)}, y = {Formaat(y)}");
            else
                tulem.Add($"statsionaarne punkt: x = {Formaat(x)}, y = {Formaat(y)}");
        }
        return tulem.Count == 0 ? "Ekstreemume ei leitud." : string.Join("; ", tulem);
    }

    private static double Bisektsioon(Avaldis f, double a, double b)
    {
        var fa = Turvaline(f, a); var fb = Turvaline(f, b);
        if (!double.IsFinite(fa) || !double.IsFinite(fb)) return double.NaN;
        for (int i = 0; i < 70; i++)
        {
            var m = (a + b) / 2;
            var fm = Turvaline(f, m);
            if (!double.IsFinite(fm)) return double.NaN;
            if (Math.Abs(fm) < 1e-10) return m;
            if (fa * fm <= 0) { b = m; fb = fm; }
            else { a = m; fa = fm; }
        }
        return (a + b) / 2;
    }

    private static double Turvaline(Avaldis avaldis, double x)
    {
        try
        {
            var y = avaldis.Arvuta(x);
            return double.IsFinite(y) && Math.Abs(y) < 1e100 ? y : double.NaN;
        }
        catch { return double.NaN; }
    }

    private static void LisaUnikaalne(List<double> list, double x)
    {
        if (list.All(v => Math.Abs(v - x) > 1e-4)) list.Add(x);
    }

    private static string PuhastaTuletis(string tekst)
    {
        return tekst.Replace("(0 + 0)", "0").Replace("(1 * ", "(").Replace(" * 1)", ")");
    }

    private static string Formaat(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}
