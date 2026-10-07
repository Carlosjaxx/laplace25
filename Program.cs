using System.Text.RegularExpressions;
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Programa principal//
do
{
    Console.Clear();
    T("TRANSFORMADA DE LAPLACE");
    T("Ejemplos para poner 2, t, 4t^2, e^t, e^(3t), te^(2t), 3t^2e^(-t). Para sumas usa espacios: 3 + t^2 + e^(2t)");

    // Separamos la entrada por espacios y quitamos los signos "+" sueltos//
    string[] terminos = (Console.ReadLine() ?? "")
        .Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(p => p != "+").ToArray();

    if (terminos.Length == 0) { T("Debes escribir una función."); continue; }

    // Si hay varios términos usamos linealidad: L{f+g} = L{f} + L{g}//
    if (terminos.Length > 1)
    {
        T("Aplicamos linealidad: la transformada de una suma es la suma de transformadas.");
        E($"∫[0,∞] {string.Join(" + ", terminos.Select(t => $"e^(-st)({t})"))} dt");
    }

    // Resolvemos cada término; TakeWhile se detiene en el primero que no se reconozca y no muestra el resultado final//
    var res = terminos.Select(Resolver).TakeWhile(r => r != "").ToList();

    // Solo se muestra el resultado si todos los términos se resolvieron correctamente//
    if (res.Count == terminos.Length)
    {
        Console.WriteLine();
        E("RESULTADO FINAL");
        E($"L(f) = {string.Join(" + ", res.Select(r => res.Count > 1 ? $"({r})" : r))}");
    }
    T("\n¿Resolver otra función? (s/n)");
} while ((Console.ReadLine() ?? "").Trim().ToLower() == "s");

// Estilo de salida al ejecutar el programa//
static void Color(ConsoleColor c, string s) { Console.ForegroundColor = c; Console.WriteLine(s); Console.ResetColor(); }
static void T(string s) => Color(ConsoleColor.Yellow, s);
static void E(string s) => Color(ConsoleColor.Green, s); 
static void Real() => T("Calculando el resultado");

// Convierte el texto de un coeficiente a número: "" -> 1, "-" -> -1, "5" -> 5//
static int Coef(string s) => s is "" or "+" ? 1 : s == "-" ? -1 : int.Parse(s);

// Escribe t^k de forma limpia: t^0 -> 1, t^1 -> t//
static string Tp(int k) => k == 0 ? "1" : k == 1 ? "t" : $"t^{k}";

// Derivada de t^k por regla de potencias (baja el exponente y le resta 1): k·t^(k-1) dt//
static string Du(int k) => k == 1 ? "dt" : $"{k}{Tp(k - 1)} dt";

// Integración por partes para t^n con e^(-qt)//
// q = "s" para t^n, y q = "(s - a)" para t^n·e^(at)
// Usa I_k(b) = ∫[0,b] t^k e^(-qt) dt  y la fórmula  ∫u dv = uv - ∫v du formulas para resolver//
static string PorPartes(int c, int n, string q, string cond)
{
    T("Reescribir la integral impropia con un límite:");
    E($"{c} lim b->∞ ∫[0,b] {Tp(n)}e^(-{q}t) dt   ({cond})");
    T("Integración por partes: ∫u dv = uv - ∫v du");

    // Cada vuelta baja la potencia de t en 1 hasta llegar a I_0 y asi sucesivamente//
    for (int k = n; k >= 1; k--)
    {
        T($"\nPotencia {k}: definimos u, du, dv y v");
        E($"u = {Tp(k)}");                     // u: la parte polinomial
        E($"du = {Du(k)}");                    // du: su derivada
        E($"dv = e^(-{q}t) dt");               // dv: la exponencial
        E($"v = -e^(-{q}t)/{q}");              // v: la integral de dv
        E($"I_{k}(b) = [-{Tp(k)}e^(-{q}t)/{q}][0,b] + ({k}/{q})I_{k - 1}(b)");
        T("Evaluacion...");
        // El término evaluado en b tiende a 0 porque la exponencial decae más rápido que la potencia//
        E($"-b^{k}e^(-{q}b)/{q} -> 0 cuando b->∞ ({cond})");
    }

    // Integral base, sin t: I_0 = 1/q este es otro//
    T("\nIntegral base:");
    E($"I_0(b) = [-e^(-{q}t)/{q}][0,b] = (1 - e^(-{q}b))/{q} -> 1/{q}");

    // Armamos el factorial paso a paso: I_k = k!/q^(k+1) y le damos valor//
    decimal fact = 1;
    for (int k = 1; k <= n; k++)
    {
        fact *= k; Real();
        E($"lim b->∞ I_{k}(b) = {fact}/{q}^{k + 1}");
    }

    // Multiplicamos por el coeficiente que habíamos sacado de la integral//
    string den = n == 0 ? q : $"{q}^{n + 1}";
    Real();
    E($"{c} * {fact}/{den} = {c * fact}/{den}");
    return $"{c * fact}/{den}";
}

// Aqui se resuelve cada termino//
static string Resolver(string p)
{
    Console.WriteLine();
    T($"--- Resolviendo: {p} ---");

    // Caso 1: constante (5, -3, .)//
    if (Regex.IsMatch(p, @"^[+-]?\d{1,9}$"))
    {
        int c = int.Parse(p);
        if (c == 0) { T("La transformada de cero es cero."); return "0"; }
        string k = c < 0 ? $"({c})" : $"{c}";   // paréntesis si es negativa//

        T("Reescribir la integral impropia con un límite:");
        E($"{k} lim b->∞ ∫[0,b] e^(-st) dt");
        T("Integramos:");
        E($"lim b->∞ : [-{k}/s(e^(-st))][0,b]");
        T("Evaluacion...");
        E($"lim b->∞ : [-{k}/s(e^(-s*b))] - [-{k}/s(e^(-s*0))]");
        Real(); T("Para s > 0, e^(-sb) -> 0");
        E($"lim b->∞ : [0] - [-{k}/s(e^(-s*0))]");
        Real();
        E($"- [-{k}/s(1)] = {k}/s(1)");
        Real();
        E($"{c}/s");
        return $"{c}/s";
    }

    // Caso 2: potencia de t (t, t^2, 3t^2, -2t .)//
    var m = Regex.Match(p, @"^(?<c>[+-]?\d{0,9})\*?t(?:\^(?<n>\d{1,2}))?$");
    if (m.Success)
    {
        int c = Coef(m.Groups["c"].Value);
        int n = m.Groups["n"].Success ? int.Parse(m.Groups["n"].Value) : 1;
        if (n > 20) { T("Usa una potencia entera entre 0 y 20."); return ""; }
        if (c == 0) { T("El término es cero. Su transformada es 0."); return "0"; }

        return PorPartes(c, n, "s", "s > 0");   // aquí q = s
    }

    // Caso 3: potencia de t por exponencial (te^(2t), 3t^2e^(-t) .)//
    var pe = Regex.Match(p, @"^(?<c>[+-]?\d{0,9})\*?t(?:\^(?<n>\d{1,2}))?\*?e\^(?:t|\((?<a>[+-]?\d{0,9})t\))$");
    if (pe.Success)
    {
        int c = Coef(pe.Groups["c"].Value);
        int n = pe.Groups["n"].Success ? int.Parse(pe.Groups["n"].Value) : 1;
        int a = pe.Groups["a"].Success ? Coef(pe.Groups["a"].Value) : 1;   // e^t -> a = 1
        if (n > 20) { T("Usa una potencia entera entre 0 y 20."); return ""; }
        if (c == 0) { T("El término es cero. Su transformada es 0."); return "0"; }

        // q = s - a  (se escribe + si a es negativo y s si a es cero)//
        string q = a > 0 ? $"(s - {a})" : a < 0 ? $"(s + {-(long)a})" : "s";

        T("Multiplicamos las exponenciales:");
        E($"e^(-st) * e^({a}t) = e^(-{q}t)");
        string r = PorPartes(c, n, q, $"{q} > 0");
        T($"Condición: s > {a}");
        return r;
    }

    // Caso 4: exponencial sola (e^t, e^(2t), 3e^(-t) .)//
    var e = Regex.Match(p, @"^(?<c>[+-]?\d{0,9})\*?e\^(?:t|\((?<a>[+-]?\d{0,9})t\))$");
    if (e.Success)
    {
        int c = Coef(e.Groups["c"].Value);
        int a = e.Groups["a"].Success ? Coef(e.Groups["a"].Value) : 1;
        if (c == 0) { T("El término es cero. Su transformada es 0."); return "0"; }
        string q = a > 0 ? $"(s - {a})" : a < 0 ? $"(s + {-(long)a})" : "s";

        // e^(-st) * e^(at) se combinan en una sola exponencial e^(-qt)//
        T("Multiplicamos las exponenciales:");
        E($"e^(-st) * e^({a}t) = e^((-s + ({a}))t) = e^(-{q}t)");
        T("Reescribir la integral impropia con q = " + q + ":");
        E($"{c} lim b->∞ ∫[0,b] e^(-qt) dt");
        T("Sustitución: u = -qt, du = -q dt");
        E("∫e^(-qt) dt = -e^(-qt)/q");
        T("Evaluacion...");
        E($"{c} lim b->∞ [-e^(-qb)/q + 1/q]");
        Real(); T("Si q > 0, e^(-qb) -> 0");
        E($"{c}/{q}");
        T($"Condición: s > {a}");
        return $"{c}/{q}";
    }

    // Si no llega ha reconocer el termino da un error//
    T($"No se reconoce el término: {p}");
    T("Ejemplos válidos: 2, t, 4t^2, e^t, 2e^(-t), te^(2t).");
    return "";
}