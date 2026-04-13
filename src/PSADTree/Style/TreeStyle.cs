using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PSADTree.Extensions;

namespace PSADTree.Style;

public sealed class TreeStyle
{
    public static TreeStyle Instance { get => s_instance ??= new(); }
    public OutputRendering OutputRendering { get; set; } = OutputRendering.Host;
    public RenderingStyle RenderingStyle
    {
        get;
        set
        {
            RenderingSet = value switch
            {
                RenderingStyle.Fancy => RenderingSet.Fancy,
                RenderingStyle.FancyRounded => RenderingSet.FancyRounded,
                RenderingStyle.Classic => RenderingSet.Classic,
                RenderingStyle.ClassicRounded => RenderingSet.ClassicRounded,
                _ => throw new ArgumentOutOfRangeException(nameof(RenderingStyle))
            };

            field = value;
        }
    } = RenderingStyle.Fancy;
    public string Reset { get; } = "\x1B[0m";
    public LeafStyle Leaf { get; } = new();
    public Palette Palette { get; } = new();

    internal RenderingSet RenderingSet { get; private set; } = RenderingSet.Fancy;

    private static TreeStyle? s_instance;
    private static readonly Regex s_validate = new(
        @"^\x1B\[(?:[0-9]+;?){1,}m$",
        RegexOptions.Compiled);

    internal TreeStyle()
    { }

    public static string CombineSequence(string left, string right)
    {
        ThrowIfInvalidSequence(left);
        ThrowIfInvalidSequence(right);
        return $"{left.TrimEnd('m')};{right.Substring(2)}";
    }

    public static string ToItalic(string vt)
    {
        ThrowIfInvalidSequence(vt);
        return $"{vt.TrimEnd('m')};3m";
    }

    public static string ToBold(string vt)
    {
        ThrowIfInvalidSequence(vt);
        return $"{vt.TrimEnd('m')};1m";
    }

    public static string EscapeSequence(string vt) =>
        $"{vt}{vt.Replace("\x1B", "`e")}\x1B[0m";

    public static void ResetSettings() =>
        s_instance = new();

    internal static string FormatType(object instance)
    {
        PropertyInfo[] properties = instance.GetType().GetProperties();
        StringBuilder builder = new(properties.Length);
        int i = 1;

        foreach (PropertyInfo property in properties)
        {
            string value = EscapeSequence(
                vt: (string)property.GetValue(instance)!,
                padding: 10);

            builder.Append(value);

            if (i++ % 4 == 0)
            {
                builder.AppendLine("\x1B[0m");
                continue;
            }

            builder.Append("\x1B[0m");
        }

        return builder.ToString();
    }

    internal static string ThrowIfInvalidSequence(string vt)
    {
        if (!s_validate.IsMatch(vt))
        {
            vt.ThrowInvalidSequence();
        }

        return vt;
    }

    private static string EscapeSequence(string vt, int padding) =>
        $"{vt}{vt.Replace("\x1B", "`e").PadRight(padding)}\x1B[0m";
}
