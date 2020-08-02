using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;
using edudy_consumer.Models;

namespace edudy_consumer
{
    public class Worker : CronJobService 
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _clientFactory;


        public bool GetBranchesError { get; private set; }

        public Worker(ILogger<Worker> logger, IHttpClientFactory clientFactory) : base("*/1 * * * *", TimeZoneInfo.Local)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }



        public override async Task<Task> DoWork(CancellationToken cancellationToken)
        {
            // change to production path
            var directories = Directory.GetDirectories("/Users/nattaaekwattanuyan/Desktop/watcher");

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} CronJob 3 is working.");
            foreach(string directory in directories)
            {
                //todo: change zip name base on folder
                var zipPath = $"{directory}/Archive.zip";
                var extractPath = $"{directory}/";
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    archive.ExtractToDirectory(extractPath + "Archive");
                }

                // do pr stuff

                var prs = await getPullRequest();

                

                // delete directory after done PR stuff
                //todo need to delete zip file as well.
                Directory.Delete(extractPath + "Archive", true);

            }

            return Task.CompletedTask;
        }

        #region private methods

        private async Task<List<GithubPullRequest>> getPullRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/aspnet/AspNetCore.Docs/pulls");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            var branches = new List<GithubPullRequest>();

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                branches = await JsonSerializer.DeserializeAsync
                    <List<GithubPullRequest>>(responseStream);
                _logger.LogInformation($"{branches}");
            }
            else
            {
                GetBranchesError = true;
                branches = new List<GithubPullRequest>();
                _logger.LogInformation($"{branches}");
            }

            return branches;
        }

        #endregion
    }
}
