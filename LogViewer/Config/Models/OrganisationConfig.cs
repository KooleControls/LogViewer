using KC.InternalApi.Client;
using KC.InternalApi.Model;
using System.ComponentModel;

namespace LogViewer.Config.Models
{
    public class OrganisationConfig
    {
        public string? Name { get; set; }
        public int? OrganisationId { get; set; }


        public string? BasePath { get; set; }
        public string? AuthPath { get; set; }
        public string? ClientId { get; set; } = "SmartHomeAnalyzer";
        public string? ClientSecret { get; set; }

        public AuthenticationMethods? AuthenticationMethod { get; set; }
        public override string ToString() => Name ?? "No name";

    }


    public enum AuthenticationMethods
    {
        GetOAuth2_OpenIdConnectClient,
        GetOAuth2_ApplicationFlowClient,
    }
}

