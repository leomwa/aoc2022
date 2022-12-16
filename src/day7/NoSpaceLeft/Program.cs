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

    private static readonly Dictionary<string, DirectoryInformation> FileSystemMap = new();
    private static readonly DirectoryInformation RootDirectory = new(Root, string.Empty);
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
            AddFileToCurrentDirectory(new(fileName, fileSize));
        }
    }

    private static void ChangeDirectory(string directory)
    {
        string search = new FsIndex(directory, _cwd).NameInIndex;
        if (!FileSystemMap.TryGetValue(search, out var cwd))
        {
            switch (directory)
            {
                case Root:
                    FileSystemMap.Add(Root, RootDirectory);
                    cwd = RootDirectory;
                    break;

                case GoToParent:
                    if (_cwd.ParentName == Root || string.IsNullOrWhiteSpace(_cwd.ParentName))
                    {
                        cwd = RootDirectory;
                        break;
                    }

                    cwd = FileSystemMap[_cwd.ParentName];
                    break;

                default:
                    cwd = FileSystemMap[new FsIndex(directory, _cwd).NameInIndex];
                    break;
            }
        }

        _cwd = cwd;
    }

    private static void ListDirectoryContents()
    {
        // uhm, maybe I could have used this command better
        Console.WriteLine($"Adding files and directories to directory {_cwd.Name}");
    }

    private void AddDirectoryToFileSystem(string directoryName)
    {
        FsIndex index = new(directoryName, _cwd);
        DirectoryInformation fsItem = new DirectoryInformation(index.NameInIndex, index.ParentName);
        FileSystemMap.TryAdd(index.NameInIndex, fsItem);
        _cwd.AddChildDirectory(fsItem);

        Console.WriteLine($"Added dirname:{index.NameInIndex} with parent:{index.ParentName}");
    }

    private void AddFileToCurrentDirectory(FileInformation fileInfo)
    {
        _cwd.AddToFiles(fileInfo);
        Console.WriteLine($"Added {fileInfo.FileName} to {_cwd.Name}");
    }

    public static int ToTalSizesOfDirectoriesWithSize(int maxPerDirectory)
    {
        return
            FileSystemMap.Values.Where(dir => dir.TotalSize() <= maxPerDirectory)
                .Sum(dir => dir.TotalSize());
    }

    public static int GetSmallestDirectoryToFreeSpaceRequired(int spaceRequired)
    {
        // used space subtracted from max space on disk is unused space
        int unusedSpace = MaxDiskSpace - RootDirectory.TotalSize();
        // check if unused space is enough compared to required
        int spaceToFree = spaceRequired - unusedSpace;

        // find directory where space is >= spaceToFree, pick the smallest

        return FileSystemMap.Values
            .Where(dir => dir.TotalSize() >= spaceToFree)
            .Select(dir => dir.TotalSize())
            .Min();
    }

    private readonly record struct FsIndex(string Name, DirectoryInformation Parent)
    {
        // index in format: {ParentName}/{Name}, but if under root, then plain name is OK
        public string NameInIndex => Parent.Name == Root ? Name : $"{Parent.Name}/{Name}";

        public string ParentName => string.IsNullOrWhiteSpace(Parent.ParentName) ? Root : Parent.Name;
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