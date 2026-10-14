using System.Globalization;
using System.Text;

namespace FunktsiooniUurimineWeb.Services;

public abstract class Avaldis
{
    public abstract double Arvuta(double x);
    public abstract Avaldis Tuleta();
    public virtual bool SisaldabX => true;
    public abstract string Tekst();
}

public sealed class Arv : Avaldis
{
    public double Vaartus { get; }
    public Arv(double vaartus) => Vaartus = vaartus;
    public override double Arvuta(double x) => Vaartus;
    public override Avaldis Tuleta() => new Arv(0);
    public override bool SisaldabX => false;
    public override string Tekst() => Vaartus.ToString("0.####", CultureInfo.InvariantCulture);
}

public sealed class Muutuja : Avaldis
{
    public override double Arvuta(double x) => x;
    public override Avaldis Tuleta() => new Arv(1);
    public override string Tekst() => "x";
}

public sealed class Unaarsed : Avaldis
{
    public string Operatsioon { get; }
    public Avaldis Arg { get; }
    public Unaarsed(string operatsioon, Avaldis arg) { Operatsioon = operatsioon; Arg = arg; }
    public override double Arvuta(double x)
    {
        var v = Arg.Arvuta(x);
        return Operatsioon switch
        {
            "+" => v,
            "-" => -v,
            _ => double.NaN
        };
    }
    public override Avaldis Tuleta() => new Unaarsed(Operatsioon, Arg.Tuleta());
    public override bool SisaldabX => Arg.SisaldabX;
    public override string Tekst() => Operatsioon == "-" ? $"(-{Arg.Tekst()})" : Arg.Tekst();
}

public sealed class Kahend : Avaldis
{
    public string Operatsioon { get; }
    public Avaldis Vasak { get; }
    public Avaldis Parem { get; }
    public Kahend(string operatsioon, Avaldis vasak, Avaldis parem) { Operatsioon = operatsioon; Vasak = vasak; Parem = parem; }
    public override double Arvuta(double x)
    {
        var a = Vasak.Arvuta(x);
        var b = Parem.Arvuta(x);
        return Operatsioon switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => Math.Abs(b) < 1e-14 ? double.NaN : a / b,
            "^" => SafePow(a, b),
            _ => double.NaN
        };
    }
    static double SafePow(double a, double b)
    {
        if (a < 0 && Math.Abs(b - Math.Round(b)) > 1e-10) return double.NaN;
        var v = Math.Pow(a, b);
        return double.IsFinite(v) ? v : double.NaN;
    }
    public override Avaldis Tuleta()
    {
        return Operatsioon switch
        {
            "+" => new Kahend("+", Vasak.Tuleta(), Parem.Tuleta()),
            "-" => new Kahend("-", Vasak.Tuleta(), Parem.Tuleta()),
            "*" => new Kahend("+", new Kahend("*", Vasak.Tuleta(), Parem), new Kahend("*", Vasak, Parem.Tuleta())),
            "/" => new Kahend("/", new Kahend("-", new Kahend("*", Vasak.Tuleta(), Parem), new Kahend("*", Vasak, Parem.Tuleta())), new Kahend("^", Parem, new Arv(2))),
            "^" when Parem is Arv a => new Kahend("*", new Kahend("*", new Arv(a.Vaartus), new Kahend("^", Vasak, new Arv(a.Vaartus - 1))), Vasak.Tuleta()),
            "^" => new Kahend("*", new Kahend("^", Vasak, Parem), new Kahend("+", new Kahend("*", Parem.Tuleta(), new Funktsioon("ln", Vasak)), new Kahend("/", new Kahend("*", Parem, Vasak.Tuleta()), Vasak))),
            _ => new Arv(0)
        };
    }
    public override bool SisaldabX => Vasak.SisaldabX || Parem.SisaldabX;
    public override string Tekst() => $"({Vasak.Tekst()} {Operatsioon} {Parem.Tekst()})";
}

public sealed class Funktsioon : Avaldis
{
    public string Nimi { get; }
    public Avaldis Arg { get; }
    public Funktsioon(string nimi, Avaldis arg) { Nimi = nimi; Arg = arg; }
    public override double Arvuta(double x)
    {
        var v = Arg.Arvuta(x);
        return Nimi switch
        {
            "sin" => Math.Sin(v),
            "cos" => Math.Cos(v),
            "tan" => Math.Tan(v),
            "sqrt" => v < 0 ? double.NaN : Math.Sqrt(v),
            "ln" => v <= 0 ? double.NaN : Math.Log(v),
            "log" => v <= 0 ? double.NaN : Math.Log10(v),
            "exp" => Math.Exp(v),
            "abs" => Math.Abs(v),
            _ => double.NaN
        };
    }
    public override Avaldis Tuleta()
    {
        var ap = Arg.Tuleta();
        return Nimi switch
        {
            "sin" => new Kahend("*", new Funktsioon("cos", Arg), ap),
            "cos" => new Kahend("*", new Unaarsed("-", new Funktsioon("sin", Arg)), ap),
            "tan" => new Kahend("*", new Kahend("/", new Arv(1), new Kahend("^", new Funktsioon("cos", Arg), new Arv(2))), ap),
            "sqrt" => new Kahend("/", ap, new Kahend("*", new Arv(2), new Funktsioon("sqrt", Arg))),
            "ln" => new Kahend("/", ap, Arg),
            "log" => new Kahend("/", ap, new Kahend("*", Arg, new Arv(Math.Log(10)))),
            "exp" => new Kahend("*", new Funktsioon("exp", Arg), ap),
            "abs" => new Kahend("*", new Funktsioon("sign", Arg), ap),
            _ => new Arv(0)
        };
    }
    public override bool SisaldabX => Arg.SisaldabX;
    public override string Tekst() => $"{Nimi}({Arg.Tekst()})";
}

public static class FunktsiooniParser
{
    private enum TokenType { Number, Identifier, Plus, Minus, Multiply, Divide, Power, LParen, RParen, End }
    private readonly record struct Token(TokenType Type, string Text, double Value = 0);

    public static Avaldis Parse(string sisend)
    {
        if (string.IsNullOrWhiteSpace(sisend)) throw new FormatException("Valem on tühi.");
        var parser = new Parser(LoeTokenid(sisend));
        var tulemus = parser.ParseExpression();
        if (parser.Current.Type != TokenType.End) throw new FormatException($"Ootamatu märk: {parser.Current.Text}");
        return tulemus;
    }

    private static List<Token> LoeTokenid(string sisend)
    {
        var list = new List<Token>();
        for (int i = 0; i < sisend.Length;)
        {
            var c = sisend[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (char.IsDigit(c) || c == '.')
            {
                int start = i++;
                while (i < sisend.Length && (char.IsDigit(sisend[i]) || sisend[i] == '.')) i++;
                var text = sisend[start..i];
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    throw new FormatException($"Vigane arv: {text}");
                list.Add(new Token(TokenType.Number, text, value));
                continue;
            }
            if (char.IsLetter(c))
            {
                int start = i++;
                while (i < sisend.Length && char.IsLetter(sisend[i])) i++;
                list.Add(new Token(TokenType.Identifier, sisend[start..i].ToLowerInvariant()));
                continue;
            }
            list.Add(c switch
            {
                '+' => new Token(TokenType.Plus, "+"),
                '-' => new Token(TokenType.Minus, "-"),
                '*' => new Token(TokenType.Multiply, "*"),
                '/' => new Token(TokenType.Divide, "/"),
                '^' => new Token(TokenType.Power, "^"),
                '(' => new Token(TokenType.LParen, "("),
                ')' => new Token(TokenType.RParen, ")"),
                _ => throw new FormatException($"Lubamatu märk: {c}")
            });
            i++;
        }
        list.Add(new Token(TokenType.End, ""));
        return list;
    }

    private sealed class Parser
    {
        private readonly List<Token> _tokens; private int _i;
        public Parser(List<Token> tokens) => _tokens = tokens;
        public Token Current => _tokens[_i];
        void Move() => _i++;
        bool Take(TokenType t) { if (Current.Type != t) return false; Move(); return true; }
        public Avaldis ParseExpression() => ParseAddSub();
        Avaldis ParseAddSub()
        {
            var node = ParseMulDiv();
            while (Current.Type is TokenType.Plus or TokenType.Minus)
            {
                var op = Current.Text; Move(); node = new Kahend(op, node, ParseMulDiv());
            }
            return node;
        }
        Avaldis ParseMulDiv()
        {
            var node = ParsePower();
            while (Current.Type is TokenType.Multiply or TokenType.Divide)
            {
                var op = Current.Text; Move(); node = new Kahend(op, node, ParsePower());
            }
            return node;
        }
        Avaldis ParsePower()
        {
            var node = ParseUnary();
            if (Take(TokenType.Power)) node = new Kahend("^", node, ParsePower());
            return node;
        }
        Avaldis ParseUnary()
        {
            if (Take(TokenType.Plus)) return new Unaarsed("+", ParseUnary());
            if (Take(TokenType.Minus)) return new Unaarsed("-", ParseUnary());
            return ParsePrimary();
        }
        Avaldis ParsePrimary()
        {
            if (Current.Type == TokenType.Number) { var v = Current.Value; Move(); return new Arv(v); }
            if (Current.Type == TokenType.Identifier)
            {
                var nimi = Current.Text; Move();
                if (nimi == "x") return new Muutuja();
                if (nimi == "pi") return new Arv(Math.PI);
                if (nimi == "e") return new Arv(Math.E);
                if (!Take(TokenType.LParen)) throw new FormatException($"Funktsioon {nimi} vajab sulge.");
                var arg = ParseExpression();
                if (!Take(TokenType.RParen)) throw new FormatException("Puuduv sulgev sulg.");
                if (nimi is not ("sin" or "cos" or "tan" or "sqrt" or "ln" or "log" or "exp" or "abs"))
                    throw new FormatException($"Tundmatu funktsioon: {nimi}");
                return new Funktsioon(nimi, arg);
            }
            if (Take(TokenType.LParen))
            {
                var node = ParseExpression();
                if (!Take(TokenType.RParen)) throw new FormatException("Puuduv sulgev sulg.");
                return node;
            }
            throw new FormatException("Oodatud oli arv, x või sulg.");
        }
    }
}
