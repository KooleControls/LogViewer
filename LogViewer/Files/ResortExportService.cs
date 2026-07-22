using KC.InternalApi.Model;
using KC.InternalApiClient;
using LogViewer.Config.Models;
using LogViewer.Providers.API;
using System.Xml.Serialization;

namespace LogViewer.Files
{
    /// <summary>
    /// UI-free logic for exporting resorts/organisations to the .kcresort / .kcbundle
    /// files the KC220 config tool imports. Shared by the Tools ▸ Export dialog and the
    /// "220 tool" launcher on the WebAPI tab.
    /// </summary>
    public static class ResortExportService
    {
        // Bouwt de export van één resort (instellingen + alle apparaten via objecten -> gateways).
        public static async Task<ResortExport> BuildResortExportAsync(
            InternalApiClient client, OrganisationConfig org, Resort resort,
            IProgress<double>? progress, CancellationToken token)
        {
            var export = new ResortExport
            {
                Organisation = org.Name,
                OrganisationHost = ExtractHost(org.BasePath),
                ResortName = resort.Name,
                ServerAddress = resort.Settings?.ConnectionServerSettings?.ServerAddress,
                ComPort = resort.Settings?.ConnectionServerSettings?.ComPort?.ToString(),
                TrgPort = resort.Settings?.ConnectionServerSettings?.TrgPort?.ToString(),
                InstallCode = resort.Settings?.InstallCode,
            };
            if (resort.Id == null)
                return export;

            var objectsProvider = new ApiObjectItemProviderBuilder(client)
                .ForResort(resort.Id.Value)
                .WithSortByName()
                .Build();

            var objects = new List<ObjectItem>();
            await foreach (var obj in objectsProvider.GetData(token, null))
                objects.Add(obj);

            for (int i = 0; i < objects.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                progress?.Report((double)i / Math.Max(1, objects.Count));

                var obj = objects[i];
                if (obj.Id == null)
                    continue;

                var gatewaysProvider = new ApiGatewayProviderBuilder(client)
                    .ForObjectItem(obj.Id.Value)
                    .WithSortByName()
                    .Build();

                var gateways = new List<Gateway>();
                await foreach (var gw in gatewaysProvider.GetData(token, null))
                    gateways.Add(gw);

                foreach (var gw in gateways)
                {
                    export.Devices.Add(new ResortExportDevice
                    {
                        Name = string.IsNullOrWhiteSpace(gw.Name) ? obj.Name : gw.Name,
                        Object = obj.Name,
                        Sid = (int)(gw.Sid ?? 0),
                        DeviceId = (int)(gw.GatewayId ?? 0),
                    });
                }
            }
            return export;
        }

        // Bouwt de export van ALLE resorts van een organisatie tot één bundle.
        public static async Task<ResortBundle> BuildBundleAsync(
            InternalApiClient client, OrganisationConfig org,
            IProgress<double>? progress, CancellationToken token)
        {
            var bundle = new ResortBundle();
            if (org.OrganisationId == null)
                return bundle;

            var resortProvider = new ApiResortProviderBuilder(client)
                .ForOrganization(org.OrganisationId.Value)
                .WithSortByName()
                .Build();

            var resorts = new List<Resort>();
            await foreach (var r in resortProvider.GetData(token, null))
                resorts.Add(r);

            for (int i = 0; i < resorts.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                progress?.Report((double)i / Math.Max(1, resorts.Count));
                var export = await BuildResortExportAsync(client, org, resorts[i], null, token);
                bundle.Resorts.Add(export);
            }
            return bundle;
        }

        public static void SaveResort(ResortExport export, string path)
        {
            var serializer = new XmlSerializer(typeof(ResortExport));
            using var stream = File.Create(path);
            serializer.Serialize(stream, export);
        }

        public static void SaveBundle(ResortBundle bundle, string path)
        {
            var serializer = new XmlSerializer(typeof(ResortBundle));
            using var stream = File.Create(path);
            serializer.Serialize(stream, bundle);
        }

        public static string ExtractHost(string? basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                return "";
            try { return new Uri(basePath).Host; } catch { return ""; }
        }

        public static string MakeSafeFileName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "resort";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
