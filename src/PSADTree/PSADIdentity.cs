using System;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace PSADTree;

public class PSADIdentity
{
    private readonly string _queryString;

    private static readonly Regex s_re = new(
        "^(?:CN|OU|DC)=",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(10));

    public PSADIdentity(Guid objectGuid) =>
        _queryString = $"LDAP://<guid={objectGuid}>";

    public PSADIdentity(SecurityIdentifier objectSid) =>
        _queryString = $"LDAP://<sid={objectSid}>";

    public PSADIdentity(string identity) =>
        _queryString = string.Concat("LDAP://", identity);

    public static PSADIdentity Parse(string identity)
    {
        if (Guid.TryParse(identity, out Guid guid))
        {
            return new(guid);
        }

        if (TryParseSid(identity, out SecurityIdentifier? sid))
        {
            return new(sid);
        }

        if (s_re.IsMatch(identity))
        {
            return new(identity);
        }

        return new((SecurityIdentifier) new NTAccount(identity)
            .Translate(typeof(SecurityIdentifier)));
    }

    private static bool TryParseSid(
        string input,
        [NotNullWhen(true)] out SecurityIdentifier? sid)
    {
        try
        {
            sid = new SecurityIdentifier(input);
            return true;
        }
        catch
        {
            sid = null;
            return false;
        }
    }

    public DirectoryEntry GetEntry() => new(_queryString);
}
