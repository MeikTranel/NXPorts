namespace NXPorts
{
    public sealed class Diagnostic
    {
        public string Code { get; }
        public string MessageResourceKey { get; }

        public Diagnostic(string code, string messageResourceKey)
        {
            Code = code;
            MessageResourceKey = messageResourceKey;
        }
    }

    public static class Diagnostics
    {
        public static readonly Diagnostic DuplicateAliases = new("NXP0001", "Diag_DuplicateAliasesMessage");
        public static readonly Diagnostic DuplicateAliasesWithDifferentCaps = new("NXP0002", "Diag_CaseInsensitivelyDuplicateAliasesMessage");
        public static readonly Diagnostic NoMethodAnnotationsFound = new("NXP0003", "Diag_NoMethodAnnotationsFoundMessage");
    }
}
