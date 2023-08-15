namespace PSADTree;

public sealed class TreeObject
{
    internal string Source { get; }

    internal int Depth { get; }

    public string SamAccountName { get; }

    public string ObjectClass { get; }

    public string Hierarchy { get; internal set; }

    internal TreeObject(
        string source,
        string samAccountName,
        string objectClass,
        int depth)
    {
        Source = source;
        SamAccountName = samAccountName;
        ObjectClass = objectClass;
        Hierarchy = samAccountName.Indent(depth);
    }

    internal TreeObject(
        string source,
        string samAccountName,
        string objectClass)
    {
        Source = source;
        SamAccountName = samAccountName;
        ObjectClass = objectClass;
        Hierarchy = samAccountName;
    }
}
