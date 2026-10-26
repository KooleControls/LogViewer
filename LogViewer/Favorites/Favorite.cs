using LogViewer.Config.Models;

namespace LogViewer.Favorites
{
    /// <summary>
    /// Where a favorite's logs come from. Only <see cref="Api"/> is implemented; the
    /// discriminator exists from the start so adding <see cref="Device"/> later doesn't
    /// invalidate anyone's favorites.yaml.
    /// </summary>
    public enum FavoriteSource
    {
        Api,
        Device,
    }

    /// <summary>
    /// A saved gateway, so you don't have to walk organisation -> resort -> object -> gateway
    /// every time you want to look at it.
    /// </summary>
    public class Favorite
    {
        public FavoriteSource Source { get; set; } = FavoriteSource.Api;

        /// <summary>Yours, not the API's. Defaults to the gateway name when added.</summary>
        public string? Label { get; set; }

        // --- Api source ---

        /// <summary>Organisation name, for display and as a fallback match.</summary>
        public string? Organisation { get; set; }

        /// <summary>
        /// Which organisation to authenticate against, identified by its server. Deliberately not
        /// organisationId: every tenant numbers its own organisation 1, so that field identifies
        /// nothing across servers - the same mistake as using a gateway's database oid as its id.
        /// </summary>
        public string? OrganisationBasePath { get; set; }

        /// <summary>Database key of the gateway record - what the log query filters on.</summary>
        public int GatewayOid { get; set; }

        /// <summary>The gateway's own device id (DGID), stamped onto fetched entries as the parent.</summary>
        public int? GatewayDeviceId { get; set; }

        /// <summary>Context only, for the list. Nothing breaks when these go stale.</summary>
        public string? Resort { get; set; }
        public string? ObjectName { get; set; }

        /// <summary>Same gateway, regardless of what it has been renamed to since.</summary>
        public bool IsSameGateway(Favorite other)
            => other.Source == Source
            && IsSameOrganisation(other)
            && other.GatewayOid == GatewayOid;

        /// <summary>
        /// Compares by server when both know it, by name otherwise - so favorites written before
        /// the basePath existed keep matching instead of silently duplicating.
        /// </summary>
        private bool IsSameOrganisation(Favorite other)
        {
            if (!string.IsNullOrWhiteSpace(OrganisationBasePath) &&
                !string.IsNullOrWhiteSpace(other.OrganisationBasePath))
                return string.Equals(OrganisationBasePath, other.OrganisationBasePath, StringComparison.OrdinalIgnoreCase);

            return string.Equals(Organisation, other.Organisation, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finds the organisation to authenticate against: by server first, by name as a fallback
        /// for favorites written before the server was stored. Never by organisationId - every
        /// tenant numbers its own organisation 1, so matching on it silently picks another server.
        /// </summary>
        public OrganisationConfig? FindOrganisation(IEnumerable<OrganisationConfig> organisations)
        {
            var candidates = organisations.ToList();

            if (!string.IsNullOrWhiteSpace(OrganisationBasePath))
            {
                var byServer = candidates.FirstOrDefault(o =>
                    string.Equals(o.BasePath, OrganisationBasePath, StringComparison.OrdinalIgnoreCase));

                if (byServer != null)
                    return byServer;
            }

            return candidates.FirstOrDefault(o =>
                string.Equals(o.Name, Organisation, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reads from small to big - gateway, object, resort, organisation - so the part you
        /// picked it for comes first and the shared context trails behind it.
        /// </summary>
        public override string ToString()
        {
            var name = string.IsNullOrWhiteSpace(Label) ? $"Gateway {GatewayOid}" : Label;
            var id = GatewayDeviceId is int dgid ? $" [{dgid}]" : string.Empty;

            return string.Join(" / ", new[] { $"{name}{id}", ObjectName, Resort, Organisation }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
