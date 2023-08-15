using System.DirectoryServices;

namespace PSADTree;

public abstract class TreeObject<T>
    where T : DirectoryEntry
{
    internal string Source { get; }

    internal int Depth { get; set; }

    public string Hierarchy { get; internal set; }

    protected TreeObject(T entry, int depth, string source)
    {
        Source = source;
        Hierarchy = DirectoryEntry.
        Instance = identity;
    }
}
