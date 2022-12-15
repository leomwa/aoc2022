using System.Text.RegularExpressions;

const string inputFileName = "no-space-left-input.txt";
using StreamReader fileReader = File.OpenText(inputFileName);

FileSystemSpaceCalculator spaceCalculator = new();

while (true)
{
    string? input = await fileReader.ReadLineAsync();
    if (input is null)
    {
        break;
    }

    spaceCalculator.Parse(input);
}

int totalSize100K = FileSystemSpaceCalculator.ToTalSizesOfDirectoriesWithSize(100_000);
Console.WriteLine($"Part1: Sum of total sizes of directories with at least 100000 in size is: {totalSize100K}");

int availableToDelete = FileSystemSpaceCalculator.GetSmallestDirectoryToFreeSpaceRequired(30_000_000);
Console.WriteLine($"Part2: Smallest directory size available to delete is: {availableToDelete}");


public class FileSystemSpaceCalculator
{
    private const int MaxDiskSpace = 70_000_000;

    private const string Root = "/";
    private const string GoToParent = "..";
    private const string CommandPattern = @"\$ (cd|ls) ?(?<directory>.*)";
    private const string DirectoryNamePattern = @"(dir (?<dirName>.+))";
    private const string FileInfoPattern = @"(?<fileSize>\d.*) (?<fileName>.*)";

    private static readonly Dictionary<string, DirectoryInformation> FileSystem = new();
    private static DirectoryInformation _cwd;

    public void Parse(string input)
    {
        // \$ (cd|ls) ?(?<directory>.*)
        var commandMatch = Regex.Match(input, CommandPattern);
        if (commandMatch.Success)
        {
            string command = commandMatch.Groups[1].Value;
            string directory = commandMatch.Groups["directory"].Value;

            switch (command)
            {
                case "cd":
                    ChangeDirectory(directory);
                    break;
                case "ls":
                    ListDirectoryContents();
                    break;
            }
        }

        // (dir (?<dirName>.+))
        var dirNameMatch = Regex.Match(input, DirectoryNamePattern);
        if (dirNameMatch.Success)
        {
            string directoryName = dirNameMatch.Groups["dirName"].Value;
            AddDirectoryToFileSystem(directoryName);
        }

        // (?<fileSize>\d.*) (?<fileName>.*)
        var fileInfoMatch = Regex.Match(input, FileInfoPattern);
        if (fileInfoMatch.Success)
        {
            int fileSize = Convert.ToInt32(fileInfoMatch.Groups["fileSize"].Value);
            string fileName = fileInfoMatch.Groups["fileName"].Value;
            AddFileToCurrentDirectory(new FileInformation(fileName, fileSize));
        }
    }

    private static void ChangeDirectory(string directory)
    {
        string search = _cwd.Name == Root ? directory : $"{_cwd.Name}/{directory}";
        if (!FileSystem.TryGetValue(search, out var currentWorkingDirectory))
        {
            switch (directory)
            {
                case Root:
                    var root = new DirectoryInformation(Root, string.Empty);
                    FileSystem.Add(Root, root);
                    _cwd = root;
                    return;

                case GoToParent:
                    if (_cwd.ParentName == Root || string.IsNullOrWhiteSpace(_cwd.ParentName))
                    {
                        _cwd = FileSystem[Root];
                        return;
                    }

                    _cwd = FileSystem[_cwd.ParentName];
                    return;

                default:
                    string dirIndexName = $"{_cwd.Name}/{directory}";
                    if (_cwd.Name == Root)
                    {
                        dirIndexName = directory;
                    }

                    _cwd = FileSystem[dirIndexName];
                    return;
            }
        }

        _cwd = currentWorkingDirectory;
    }

    private static void ListDirectoryContents()
    {
        // uhm, maybe I could have used this command better
        Console.WriteLine($"Adding files and directories to directory {_cwd.Name}");
    }

    private void AddDirectoryToFileSystem(string directoryName)
    {
        string nameInFileSystem = $"{_cwd.Name}/{directoryName}";
        if (_cwd.Name == Root)
        {
            nameInFileSystem = directoryName;
        }

        string parentName = _cwd.Name;
        if (string.IsNullOrWhiteSpace(_cwd.ParentName))
        {
            parentName = Root;
        }

        var dir = new DirectoryInformation(nameInFileSystem, parentName);
        FileSystem.TryAdd(nameInFileSystem, dir);
        _cwd.AddChildDirectory(dir);

        Console.WriteLine($"Added dirname:{nameInFileSystem} with parent:{parentName}");
    }

    private void AddFileToCurrentDirectory(FileInformation fileInfo)
    {
        _cwd.AddToFiles(fileInfo);
        Console.WriteLine($"Added {fileInfo.FileName} to {_cwd.Name}");
    }

    public static int ToTalSizesOfDirectoriesWithSize(int maxPerDirectory)
    {
        return
            FileSystem.Values.Where(dir => dir.TotalSize() <= maxPerDirectory)
                .Sum(dir => dir.TotalSize());
    }

    public static int GetSmallestDirectoryToFreeSpaceRequired(int spaceRequired)
    {
        // used space subtracted from max space on disk is unused space
        int unusedSpace = MaxDiskSpace - FileSystem[Root].TotalSize();
        // check if unused space is enough compared to required
        int spaceToFree = spaceRequired - unusedSpace;

        // find directory where space is >= spaceToFree, pick the smallest

        return FileSystem.Values
            .Where(dir => dir.TotalSize() >= spaceToFree)
            .Select(dir => dir.TotalSize())
            .Min();
    }
}

public readonly record struct DirectoryInformation(string Name, string ParentName)
{
    private List<DirectoryInformation> Children { get; } = new();
    private List<FileInformation> Files { get; } = new();

    public void AddToFiles(FileInformation file)
    {
        Files.Add(file);
    }

    public void AddChildDirectory(DirectoryInformation dir)
    {
        Children.Add(dir);
    }

    public int TotalSize()
    {
        int sumOfChildren = Children.Sum(c => c.TotalSize());
        return Files.Sum(f => f.FileSize) + sumOfChildren;
    }
}

public record struct FileInformation(string FileName, int FileSize);