namespace PSADTree.Style;

internal readonly struct RenderingSet
{
    internal static readonly RenderingSet Fancy = new("└── ", '└', '│', '├', "↔");

    internal static readonly RenderingSet FancyRounded = new("╰── ", '╰', '│', '├', "↔");

    internal static readonly RenderingSet Classic = new("+-- ", '+', '|', '|', "<>");

    internal static readonly RenderingSet ClassicRounded = new("`-- ", '`', '|', '|', "<>");

    internal string Corner { get; }

    internal char UpRight { get; }

    internal char Vertical { get; }

    internal char VerticalRight { get; }

    internal string Arrows { get; }

    private RenderingSet(string corner, char upRight, char vertical, char verticalRight, string arrows)
    {
        Corner = corner;
        UpRight = upRight;
        Vertical = vertical;
        VerticalRight = verticalRight;
        Arrows = arrows;
    }
}
