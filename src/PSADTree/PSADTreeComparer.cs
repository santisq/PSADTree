using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

#pragma warning disable CS8767
internal sealed class PSADTreeComparer : IComparer<Principal>
{
    internal static PSADTreeComparer Value { get; }

    static PSADTreeComparer() => Value = new();

    public int Compare(Principal lhs, Principal rhs) =>
        lhs.StructuralObjectClass == "group" && rhs.StructuralObjectClass == "group"
            ? string.Compare(rhs.SamAccountName, lhs.SamAccountName, StringComparison.Ordinal)  // Groups in descending order
            : string.Compare(lhs.SamAccountName, rhs.SamAccountName, StringComparison.Ordinal); // Other in ascending order
}
