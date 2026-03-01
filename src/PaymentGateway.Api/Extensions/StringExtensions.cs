namespace PaymentGateway.Api.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || char.IsLower(s[0])) return s ?? string.Empty;
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }
}
