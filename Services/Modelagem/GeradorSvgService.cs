public class GeradorSvgService
{
    public string GerarSvg(string? tipoVisual)
    {
        return GerarTradicional();
    }

    private string GerarTradicional()
    {
        return @"<svg ...>...</svg>";
    }
}