using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

#pragma warning disable CS8767
internal sealed class PSADTreeComparer : IComparer<Principal>
{
    public int Compare(Principal lhs, Principal rhs) =>
        lhs.StructuralObjectClass == "group" && rhs.StructuralObjectClass == "group"
            ? rhs.SamAccountName.CompareTo(lhs.SamAccountName)  // Groups in descending order
            : lhs.SamAccountName.CompareTo(rhs.SamAccountName); // Other in ascending order
}
