using System.Security.Claims;

namespace HVO.Security
{
    public static class ClaimsPrincipalExtensions
    {

        public static string GetFullName(this ClaimsPrincipal principal)
        {
            var givenName = principal.FindFirst("given_Name");
            var familyName = principal.FindFirst("family_Name");

            return $"{givenName?.Value} {familyName?.Value}".Trim();
        }
    }
}
