using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSADTree;

public sealed class LdapCompleter : IArgumentCompleter
{
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters)
    {
        foreach (string key in LdapMap.Keys)
        {
            if (key.StartsWith(wordToComplete, System.StringComparison.OrdinalIgnoreCase))
            {
                yield return new CompletionResult(
                    key,
                    key,
                    CompletionResultType.ParameterValue,
                    $"LDAP DisplayName: {LdapMap.Get(key)}");
            }
        }
    }
}
