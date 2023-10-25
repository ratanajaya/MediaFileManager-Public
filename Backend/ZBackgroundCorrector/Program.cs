using CloudAPI;
using CloudAPI.AL.Models.Sc;
using CloudAPI.Models;
using Serilog.Events;
using Serilog;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;
using Microsoft.Extensions.Configuration;

class Program
{
    static string _newLine = System.Environment.NewLine;

    static string _correctedPathsJsonPath = "";
    static string _mainApiUrl = "";

    static string _getAcm() => $"{_mainApiUrl}/Main/GetAlbumCardModels";
    static string _hscanCorrectablePaths() => $"{_mainApiUrl}/Operation/HScanCorrectiblePaths";
    static string _getCorrectablePages() => $"{_mainApiUrl}/Operation/GetCorrectablePages";
    static string _correctPages() => $"{_mainApiUrl}/Operation/CorrectPages";

    static async Task Main(string[] args) {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("bcAppSettings.json");

        var configuration = configurationBuilder.Build();

        _mainApiUrl = configuration["MainApiUrl"];
        _correctedPathsJsonPath = configuration["CorrectedPathJsonFile"];

        Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
              .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "L_.Log"), rollingInterval: RollingInterval.Day)
              .WriteTo.Console()
              .CreateLogger();
        Log.Information("~START~");

        Console.WriteLine("a. H Correction");
        Console.WriteLine("b. SC Correction");
        Console.Write($"{_newLine}Proceed? (a,b) ");

        var res = Console.ReadLine();

        var cts = new CancellationTokenSource();

        if (res == "a") {
            Console.WriteLine("Query: ");
            var query = Console.ReadLine();
            if(string.IsNullOrEmpty(query))
                return;

            Console.WriteLine("Thread: ");
            var threadStr = Console.ReadLine();
            var thread = Math.Clamp(int.Parse(threadStr), 1, 5);

            try {
                var ongoingProcessTask = MainHCorrection(query, thread, cts.Token);
                var listenForKeyPressTask = Task.Run(() => { ListenForKeyPress(cts); });

                await Task.WhenAny(listenForKeyPressTask, ongoingProcessTask);

                // If 'c' is pressed, ensure OngoingProcess has a chance to complete
                if (cts.IsCancellationRequested) {
                    await ongoingProcessTask; // Wait for it to complete
                }
            }
            catch(Exception ex) {
                Log.Error($"MainHCorrection | {ex.Message}");
            }
        }
        if(res == "b") {
            Console.WriteLine("Section: ");
            var section = Console.ReadLine();
            if(string.IsNullOrEmpty(section))
                return;

            try {
                await MainSCCorrection(section);
            }
            catch(Exception ex) {
                Log.Error($"MainSCCorrection | {ex.Message}");
            }
        }

        Log.Information("~END~");
        Console.ReadLine();
    }

    #region Local Methods
    static void ListenForKeyPress(CancellationTokenSource cts) {
        while (true) {
            var key = Console.ReadKey(intercept: true).KeyChar;

            if (key == 'c' || key == 'C') {
                Log.Information("'c' key pressed. The app will terminate soon.");
                cts.Cancel();
                break;
            }
        }
    }

    static List<string> GetFinishedHPaths() {
        if (File.Exists(_correctedPathsJsonPath)) {
            var jsonStr = File.ReadAllText(_correctedPathsJsonPath);

            var result = JsonSerializer.Deserialize<List<string>>(jsonStr);
            return result!;
        }

        throw new Exception("_correctedPathsJsonPath does not exist");
    }

    static void SerializeFinishedHPaths(List<string> paths) {
        var jsonStr = JsonSerializer.Serialize(paths, new JsonSerializerOptions {
            WriteIndented = true
        });

        File.WriteAllText(_correctedPathsJsonPath, jsonStr);
    }

    #endregion

    #region Backend Caller Methods
    static async Task MainHCorrection(string query, int thread, CancellationToken token) {
        Log.Information($"Start MainHCorrection | {query}");
        try {
            var finishedHPaths = GetFinishedHPaths();

            var albumCardModels = await GetAlbumCardModelsAsync(query);
            var albumCardModelToProcessList = albumCardModels
                .Where(a => !finishedHPaths.Contains(a.Path))
                .ToList();

            Log.Information($"AlbumCardModels | Total: {albumCardModels.Count} | ToProcess {albumCardModelToProcessList.Count}");

            bool exitMainHCorrectionApp = false;

            // Divide the albumCardModels into batches of 30 Paths each
            int batchSize = 30;
            for(int i = 0; i < albumCardModelToProcessList.Count; i += batchSize) {
                if(exitMainHCorrectionApp) {
                    Log.Information($"Exiting outer loop...");
                    break;
                }

                List<string> batchPaths = albumCardModelToProcessList
                    .Skip(i)
                    .Take(batchSize)
                    .Select(model => model.Path)
                    .ToList();

                Log.Information($"Scanning for correctible albums in batch of {batchPaths.Count}");
                Console.WriteLine("");

                HScanCorrectiblePathParam param = new HScanCorrectiblePathParam {
                    Paths = batchPaths,
                    Thread = thread,
                    UpscaleTarget = 1280
                };

                var pathCorrectionModels = await PostHScanCorrectiblePaths(param);

                foreach(var pcm in pathCorrectionModels) {
                    if(token.IsCancellationRequested) {
                        Log.Information($"Exiting inner loop...");
                        exitMainHCorrectionApp = true;
                        break;
                    }

                    Log.Information($"Pgs: {pcm.CorrectablePageCount} | {Path.GetFileName(pcm.LibRelPath)}");

                    if(pcm.CorrectablePageCount == 0) {
                        finishedHPaths.Add(pcm.LibRelPath);
                        SerializeFinishedHPaths(finishedHPaths);

                        Log.Information($"--Skp");

                        continue;
                    }

                    var fileCorrectionModels = await GetCorrectablePagesAsync(pcm.LibRelPath);

                    Log.Information($"Ret: {fileCorrectionModels.Count} | Starting correction...");

                    var correctionSw = new Stopwatch();
                    correctionSw.Start();

                    var report = await PostCorrectPages(new CorrectPageParam {
                        LibRelPath = pcm.LibRelPath,
                        FileToCorrectList = fileCorrectionModels,
                        Thread = thread,
                        Type = 0,
                        UpscalerType = SharedLibrary.UpscalerType.Waifu2xCunet,
                        ToJpeg = true
                    });

                    correctionSw.Stop();

                    if(report.Any(a => !a.Success)) {
                        var firstErrorFile = report.First(a => !a.Success);
                        Log.Error($"Error: {pcm.LibRelPath} | {firstErrorFile.AlRelPath} | {firstErrorFile.Message} | {correctionSw.Elapsed.TotalSeconds} s");
                    }
                    else {
                        finishedHPaths.Add(pcm.LibRelPath);
                        SerializeFinishedHPaths(finishedHPaths);

                        double totalSeconds = correctionSw.Elapsed.TotalSeconds;

                        double avg = totalSeconds / fileCorrectionModels.Count;

                        Log.Information($"Fin: {fileCorrectionModels.Count} | {correctionSw.Elapsed.TotalSeconds.ToString("0.##")} s | {avg.ToString("0.##")} s");
                    }
                }
            }            
        }
        catch(Exception ex) {
            Log.Error($"MainHCorrection | {ex.Message}");
        }
    }

    static async Task MainSCCorrection(string section) {

    }

    static async Task<List<AlbumCardModel>> GetAlbumCardModelsAsync(string query) {
        using(HttpClient client = new HttpClient()) {
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.GetAsync($"{_getAcm()}?query={HttpUtility.UrlEncode(query)}");

            if(response.IsSuccessStatusCode) {
                List<AlbumCardModel> albumCardModels = await response.Content.ReadAsAsync<List<AlbumCardModel>>();
                return albumCardModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetAlbumCardModelsAsync | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<PathCorrectionModel>> PostHScanCorrectiblePaths(HScanCorrectiblePathParam param) {
        using(HttpClient client = new HttpClient()) {
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.PostAsJsonAsync(_hscanCorrectablePaths(), param);

            if(response.IsSuccessStatusCode) {
                List<PathCorrectionModel> pathCorrectionModels = await response.Content.ReadAsAsync<List<PathCorrectionModel>>();
                return pathCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"PostHScanCorrectiblePaths | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<FileCorrectionModel>> GetCorrectablePagesAsync(string libRelPath) {
        using(HttpClient client = new HttpClient()) {
            client.Timeout = Timeout.InfiniteTimeSpan;
            string fullUrl = $"{_getCorrectablePages()}?type=0&path={HttpUtility.UrlEncode(libRelPath)}&thread=2&upscaleTarget=1280&clampToTarget=true";
            HttpResponseMessage response = await client.GetAsync(fullUrl);

            if(response.IsSuccessStatusCode) {
                List<FileCorrectionModel> fileCorrectionModels = await response.Content.ReadAsAsync<List<FileCorrectionModel>>();
                return fileCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetCorrectablePagesAsync | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<FileCorrectionReportModel>> PostCorrectPages(CorrectPageParam param) {
        using(HttpClient client = new HttpClient()) {
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.PostAsJsonAsync(_correctPages(), param);

            if(response.IsSuccessStatusCode) {
                List<FileCorrectionReportModel> pathCorrectionModels = await response.Content.ReadAsAsync<List<FileCorrectionReportModel>>();
                return pathCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"PostCorrectPages | {response.StatusCode} | {errMsg}");
            }
        }
    }
    #endregion
}
