using System;

namespace IdentityServer.Models
{
    public class AccountOptions
    {
        public static bool AllowLocalLogin { get; } = true;

        public static bool AllowRememberLogin { get; } = true;

        public static TimeSpan RememberMeLoginDuration { get; } = TimeSpan.FromDays(30);

        public static bool ShowLogoutPrompt { get; } = true;

        public static bool AutomaticRedirectAfterSignOut { get; } = false;

        // specify the Windows authentication scheme being used
        public static readonly string WindowsAuthenticationSchemeName = Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme;

        // if user uses windows auth, should we load the groups from windows
        public static bool IncludeWindowsGroups { get; } = false;

        public static readonly string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}
