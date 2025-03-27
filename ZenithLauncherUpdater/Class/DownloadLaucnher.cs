using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ZenithLauncherUpdater.Class
{
    public class DownloadLaucnher
    {
        private readonly string _serverUrl;
        private readonly string _destinationPath;
        private readonly TextBlock _updateText;
        private readonly HttpClient _httpClient;

        public class UpdateManifest
        {
            public string? Version { get; set; }
            public List<UpdateFile> Files { get; set; }
        }

        public class UpdateFile
        {
            public string? Path { get; set; }
            public long Size { get; set; }
            public string? Hash { get; set; }
        }

        public DownloadLaucnher(string serverUrl, string destinationPath, TextBlock updateText)
        {
            _serverUrl = serverUrl.EndsWith("/") ? serverUrl : serverUrl + "/";
            _destinationPath = destinationPath;
            _updateText = updateText;
            _httpClient = new HttpClient();
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                _updateText.Text = "Contacting download server...";

                UpdateManifest manifest = await FetchManifestAsync();

                _updateText.Text = $"Got it! There are {manifest.Files.Count} files to download.";
                return true;
            }
            catch (Exception ex)
            {
                _updateText.Text = $"We ran into an error while attempting to install Zenith: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> PerformUpdateAsync()
        {
            try
            {
                _updateText.Text = "Starting install process...";

                UpdateManifest manifest = await FetchManifestAsync();

                int totalFiles = manifest.Files.Count;
                int completedFiles = 0;

                if (!Directory.Exists(_destinationPath))
                {
                    Directory.CreateDirectory(_destinationPath);
                }

                foreach (var file in manifest.Files)
                {
                    completedFiles++;
                    _updateText.Text = $"Installing ({completedFiles}/{totalFiles}): {file.Path}";

                    await DownloadFileAsync(file);
                }

                _updateText.Text = "Install completed successfully!";
                return true;
            }
            catch (Exception ex)
            {
                _updateText.Text = $"Install failed: {ex.Message}";
                return false;
            }
        }

        private async Task<UpdateManifest> FetchManifestAsync()
        {
            string manifestUrl = $"{_serverUrl}api/download/manifest";
            string manifestJson = await _httpClient.GetStringAsync(manifestUrl);
            return JsonSerializer.Deserialize<UpdateManifest>(manifestJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private async Task DownloadFileAsync(UpdateFile file)
        {
            string fileUrl = $"{_serverUrl}api/download/files/{file.Path}";
            string destFilePath = Path.Combine(_destinationPath, file.Path);
            string destDirPath = Path.GetDirectoryName(destFilePath);

            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }

            byte[] fileData = await _httpClient.GetByteArrayAsync(fileUrl);

            string downloadedHash = CalculateMd5(fileData);
            if (!string.Equals(downloadedHash, file.Hash, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Manifest invalid.");
            }

            await File.WriteAllBytesAsync(destFilePath, fileData);
        }

        private string CalculateMd5(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}