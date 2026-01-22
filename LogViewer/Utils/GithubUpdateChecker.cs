using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Utils
{
    public class GithubUpdateChecker
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;

        public GithubUpdateChecker(string owner, string repo)
        {
            _owner = owner;
            _repo = repo;
            _client = new GitHubClient(new ProductHeaderValue(repo));
        }

        public async Task<AppVersion> GetLatestStableVersionAsync()
        {
            // Ordered newest-first by GitHub API/Octokit
            var releases = await _client.Repository.Release.GetAll(_owner, _repo);

            var stable = releases.FirstOrDefault(r =>
                r != null &&
                !r.Prerelease &&
                !string.IsNullOrWhiteSpace(r.TagName) &&
                AppVersion.TryParse(r.TagName, out _));

            if (stable == null)
                throw new InvalidOperationException("No stable (non-prerelease) release found.");

            if (!AppVersion.TryParse(stable.TagName, out var latest))
                throw new InvalidOperationException($"Stable tag '{stable.TagName}' is not a valid version.");

            return latest;
        }
    }
}
