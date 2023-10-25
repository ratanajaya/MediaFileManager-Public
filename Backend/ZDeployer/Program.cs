using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text.Json;
using ZDeployer;

Initialize();

var audn = new string[] { "a", "u", "d", "n"};

Console.Write($"{_newLine}Proceed? (A,U,D,N) ");
var res = Console.ReadLine();
if (string.IsNullOrEmpty(res)) return;

if(res.ToLower().Contains("a")) {
    WriteTask("Deploying WebApi");
    BuildApi();
    DeployApi();
}

if(res.ToLower().Contains("u")) {
    WriteTask("Deploying WebUI");
    BuildUI("./");
    DeployUI(_appConf.UiDeploymentPath);
    BuildUI("../www");
    DeployUI(Path.Combine(_appConf.CordovaPath, "www"));
}

if(res.ToLower().Contains("d")) {
    WriteTask("Deploying DashboardUI");
    BuildDashboardUI();
    DeployDashboardUI(Path.Combine(_appConf.UiDeploymentPath, "Dashboard"));
    DeployDashboardUI(Path.Combine(_appConf.CordovaPath, "www", "Dashboard"));
}

if(res.ToLower().Contains("n")) {
    WriteTask("Deploying Android App");
    BuildAndroidApp();
    DeployAndroidApp();
}


public static partial class Program
{
    static AppConf _appConf;
    static string _newLine = System.Environment.NewLine;

    private static void WriteTask(string str) {
        Console.WriteLine("-------------------------------");
        Console.WriteLine(str);
    }

    private static void  WriteSubTask(string str) {
        var paddedStr = "    " + str;
        Console.WriteLine(paddedStr);
    }


    private static void ChangeFilePermissionsToMatchParent(string filePath) {
        var dirPath = Path.GetDirectoryName(filePath);

        var directorySecurity = (new DirectoryInfo(dirPath)).GetAccessControl();
        var fi = new FileInfo(filePath);
        var fileSecurity = fi.GetAccessControl();
        fileSecurity.SetSecurityDescriptorSddlForm(directorySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.All));
        fi.SetAccessControl(fileSecurity);
    }

    public static void Initialize() {
        var appConfStr = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appConf.json"));

        _appConf = JsonSerializer.Deserialize<AppConf>(appConfStr);

        WriteTask($"Starting deployment with configurations bellow:");

        foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(_appConf)) {
            string name = descriptor.Name.PadRight(17);
            object value = descriptor.GetValue(_appConf);
            WriteSubTask($"{name} = {value}");
        }
    }

    public static void BuildApi() {
        var command = $"dotnet publish \"{Path.Combine(_appConf.WkrPath, "CloudAPI")}\" --configuration Release";
        WriteSubTask($"Executing command \"{command}\"");

        Process cmd = new Process();

        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;

        cmd.Start();

        cmd.StandardInput.WriteLine(command);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        var commandReport = cmd.StandardOutput.ReadToEnd();

        WriteSubTask("Finished building WebApi!");
    }

    public static void DeployApi() {
        string deployPath = _appConf.ApiDeploymentPath;
        WriteSubTask($"Deleting existing files in {deployPath}...");

        var excludedFiles = new List<string> {
            "ffmpeg.exe",
            "ffplay.exe",
            "ffprobe.exe",
            "web.config"
        };
        var existingFiles = Directory.GetFiles(deployPath, "*.*", SearchOption.AllDirectories);
        var toDeleteFiles = existingFiles.Where(a => !excludedFiles.Contains(Path.GetFileName(a))).ToList();

        foreach(var f in toDeleteFiles) {
            File.Delete(f);
        }

        WriteSubTask($"Existing files deleted. Copying new files to deployment location...");

        var buildLocation = Path.Combine(_appConf.WkrPath, @"CloudAPI\bin\Release\net6.0\publish");
        var buildFiles = Directory.GetFiles(buildLocation, "*.*", SearchOption.AllDirectories);
        var toMoveFiles = buildFiles.Where(a => !excludedFiles.Contains(Path.GetFileName(a))).ToList();

        foreach(var f in toMoveFiles) {
            var relativePath = Path.GetRelativePath(buildLocation, f);
            var dstPath = Path.Combine(deployPath, relativePath);
            var dstDir = Path.GetDirectoryName(dstPath);
            Directory.CreateDirectory(dstDir);
            File.Move(f, dstPath);

            ChangeFilePermissionsToMatchParent(dstPath);
        }

        WriteSubTask("Finished copying files!");
    }

    public static void BuildUI(string hmp) {
        WriteSubTask("Modifying Package.json...");

        var packageJsonPath = Path.Combine(_appConf.UiPath, "package.json");
        var packageStr = File.ReadAllText(packageJsonPath);

        var startTxt = "\"homepage\": \"";
        var endTxt = "\",";

        var startAfterIndex = packageStr.IndexOf(startTxt) + startTxt.Length;
        var endBeforeIndex = packageStr.IndexOf(endTxt, startAfterIndex);

        var newPackageStr = packageStr.Remove(startAfterIndex, (endBeforeIndex - startAfterIndex)).Insert(startAfterIndex, hmp);

        File.WriteAllText(packageJsonPath, newPackageStr);

        var command = $"npm run build --prefix \"{_appConf.UiPath}\"";
        WriteSubTask($"Executing command \"{command}\"");

        Process cmd = new Process();

        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;

        cmd.Start();

        cmd.StandardInput.WriteLine(command);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        var commandReport = (cmd.StandardOutput.ReadToEnd());

        WriteSubTask("Finished building WebUI!");
    }

    public static void DeployUI(string deployPath) {
        WriteSubTask($"Deleting existing files in {deployPath}...");

        var excludedFiles = new List<string> {
            "web.config"
        };
        var existingFiles = Directory.GetFiles(deployPath, "*.*", SearchOption.AllDirectories);
        var toDeleteFiles = existingFiles.Where(a => 
                !excludedFiles.Contains(Path.GetFileName(a)) 
                && !Path.GetRelativePath(deployPath, a).StartsWith("dashboard", StringComparison.OrdinalIgnoreCase)
            ).ToList();

        foreach(var f in toDeleteFiles) {
            File.Delete(f);
        }

        WriteSubTask($"Existing files deleted. Copying new files to deployment location...");

        var buildLocation = Path.Combine(_appConf.UiPath, "build");
        var buildFiles = Directory.GetFiles(buildLocation, "*.*", SearchOption.AllDirectories);
        var toMoveFiles = buildFiles.Where(a => !excludedFiles.Contains(Path.GetFileName(a))).ToList();

        foreach(var f in toMoveFiles) {
            var relativePath = Path.GetRelativePath(buildLocation, f);
            var dstPath = Path.Combine(deployPath, relativePath);
            var dstDir = Path.GetDirectoryName(dstPath);
            Directory.CreateDirectory(dstDir);
            File.Move(f, dstPath);

            ChangeFilePermissionsToMatchParent(dstPath);
        }

        WriteSubTask("Finished copying files!");
    }

    public static void BuildDashboardUI() {
        var command = $"npm run build --prefix \"{_appConf.DashboardUiPath}\"";
        WriteSubTask($"Executing command \"{command}\"");

        Process cmd = new Process();

        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;

        cmd.Start();

        cmd.StandardInput.WriteLine(command);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        var commandReport = (cmd.StandardOutput.ReadToEnd());

        WriteSubTask("Finished building DashboardUI!");
    }

    public static void DeployDashboardUI(string deployPath) {
        WriteSubTask($"Deleting existing files in {deployPath}...");
        Directory.CreateDirectory(deployPath);

        var existingFiles = Directory.GetFiles(deployPath, "*.*", SearchOption.AllDirectories);

        foreach(var f in existingFiles) {
            File.Delete(f);
        }

        WriteSubTask($"Existing files deleted. Copying new files to deployment location...");

        var buildLocation = Path.Combine(_appConf.DashboardUiPath, "build");
        Directory.CreateDirectory(buildLocation);
        var buildFiles = Directory.GetFiles(buildLocation, "*.*", SearchOption.AllDirectories);

        foreach(var f in buildFiles) {
            var relativePath = Path.GetRelativePath(buildLocation, f);
            var dstPath = Path.Combine(deployPath, relativePath);
            var dstDir = Path.GetDirectoryName(dstPath);
            Directory.CreateDirectory(dstDir);

            File.Copy(f, dstPath);

            ChangeFilePermissionsToMatchParent(dstPath);
        }

        WriteSubTask("Finished copying files!");
    }

    public static void BuildAndroidApp() {
        var command = $"cd /d \"{_appConf.CordovaPath}\"&cordova build android";
        WriteSubTask($"Executing command \"{command}\"");

        Process cmd = new Process();

        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;

        cmd.Start();

        cmd.StandardInput.WriteLine(command);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        var commandReport = (cmd.StandardOutput.ReadToEnd());

        WriteSubTask("Finished building Android app!");
    }

    public static void DeployAndroidApp() {
        var deployPath = _appConf.ApkDeploymentPath;

        if(string.IsNullOrEmpty(deployPath)) return;

        WriteSubTask($"Copying .apk file to {deployPath}...");

        var srcPath = Path.Combine(_appConf.CordovaPath, "platforms\\android\\app\\build\\outputs\\apk\\debug\\app-debug.apk");

        var dstPath = Path.Combine(deployPath, $"Wkr-{DateTime.Now.ToString("yyyyMMMdd-HHmmss")}.apk");

        File.Copy(srcPath, dstPath);

        WriteSubTask("Copying .apk file finished!");
    }
}