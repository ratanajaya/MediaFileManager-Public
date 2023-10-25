// See https://aka.ms/new-console-template for more information
using ZFileLister;

string root = AppDomain.CurrentDomain.BaseDirectory;

Console.WriteLine($"Starting operation on {root}\r\n");

var excludedPaths = new List<string> {
    "$RECYCLE.BIN",
    "System Volume Information",
    "Production",
    "Project",
    "V Library\\TV Show"
};

List<string> GetFilesRecursive(string folderPath) {
    var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
        .Where(f => !f.EndsWith("Thumbs.db"))
        .OrderByAlphaNumeric(f => Path.GetFileName(f))
        .ToList();

    files.ForEach(f => Console.WriteLine(f));

    var dirInfo = new DirectoryInfo(folderPath);
    var subDirs = dirInfo.GetDirectories()
        .Where(d => !excludedPaths.Any(ex => Path.GetRelativePath(root, d.FullName).StartsWith(ex)) && !d.Name.Equals(".dthumb"))
        .OrderByAlphaNumeric(f => f.FullName)
        .ToList();

    foreach(var dir in subDirs) {
        files.AddRange(GetFilesRecursive(dir.FullName));
    }

    return files;
}

var result = GetFilesRecursive(root);

var resultFilePath = Path.Combine(root, "fileList.txt");

Console.WriteLine($"\r\nWriting result to {resultFilePath}");

File.WriteAllLines(resultFilePath, result);

Console.WriteLine($"Finished!");

Console.ReadLine();