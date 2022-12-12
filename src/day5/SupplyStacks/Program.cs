using System.Text.RegularExpressions;

const string inputFileName = "supply-stacks-input.txt";
using StreamReader fileReader = File.OpenText(inputFileName);

string startingStacksPattern = @"(?:(\[[A-Z]])(?:\s*|$))+.*";
string instructionsPattern = @"move (?<cratesToMove>\d+) from (?<from>\d+) to (?<to>\d+)";
RegexOptions options = RegexOptions.Multiline;

Dictionary<int, List<string>> stacksAndCratesInput = new();
List<Rearrangement> rearrangementInstructions = new();

while (true)
{
    string? contents = await fileReader.ReadLineAsync();
    if (contents is null)
    {
        break;
    }

    if (contents.StartsWith("move"))
    {
        var instructionsMatchCollection = Regex.Matches(contents, instructionsPattern, options);

        foreach (Match m in instructionsMatchCollection)
        {
            int move = Convert.ToInt32(m.Groups["cratesToMove"].Value);
            int from = Convert.ToInt32(m.Groups["from"].Value);
            int to = Convert.ToInt32(m.Groups["to"].Value);

            rearrangementInstructions.Add(new(move, from, to));
        }
        
        continue;
    }

    var startingStacksCollection = Regex.Matches(contents, startingStacksPattern, options);
    foreach (Match m in startingStacksCollection)
    {
        //Console.WriteLine("'{0}' found", m.Value);
        foreach (Capture capture in m.Groups[1].Captures)
        {
            int stackName = capture.Index / 4 + 1; // 0-index, so add 1 for stack number
            string crate = capture.Value;

            //Console.WriteLine("'{0}' found at index {1} belongs to stack: {1}", crate, stackNumber.ToString());
            if (stacksAndCratesInput.TryGetValue(stackName, out var stack))
            {
                stack.Add(crate);
            }
            else
            {
                stacksAndCratesInput.Add(stackName, new() { crate });
            }
        }
    }
}

Dictionary<int, Stack<string>> DefaultCrateStacksFactory() =>
    stacksAndCratesInput
        .OrderBy(pair => pair.Key)
        .ToDictionary(pair => pair.Key, pair =>
        {
            var strings = pair.Value.ToArray();
            return new Stack<string>(strings.Reverse());
        });

ICargoCrane rigOperatorOnCrateMover9000 = new CrateMover9000(DefaultCrateStacksFactory, rearrangementInstructions);
rigOperatorOnCrateMover9000.PerformRearrangementProcedure();
string topCratesCrateMover9000 = rigOperatorOnCrateMover9000.GetTopCratesFromAllStacks();

Console.WriteLine($"CrateMover9000: Top crates from all stacks are: {topCratesCrateMover9000}");

ICargoCrane rigOperatorOnCrateMover9001 = new CrateMover9001(DefaultCrateStacksFactory, rearrangementInstructions);
rigOperatorOnCrateMover9001.PerformRearrangementProcedure();
string topCratesCrateMover9001 = rigOperatorOnCrateMover9001.GetTopCratesFromAllStacks();
Console.WriteLine($"CrateMover9001: Top crates from all stacks are: {topCratesCrateMover9001}");

public record struct Rearrangement(int CratesToMove, int FromStack, int ToStack);

internal interface ICargoCrane
{
    void PerformRearrangementProcedure();
    string GetTopCratesFromAllStacks();
}

internal class CrateMover9000 : ICargoCrane
{
    private readonly Dictionary<int, Stack<string>> _initialCrateStacks;
    private readonly List<Rearrangement> _rearrangements;

    public CrateMover9000(Func<Dictionary<int, Stack<string>>> stacksFactory, IEnumerable<Rearrangement> rearrangements)
    {
        _initialCrateStacks = stacksFactory();
        _rearrangements = new(rearrangements);
    }

    public void PerformRearrangementProcedure()
    {
        foreach (Rearrangement instruction in _rearrangements)
        {
            var source = _initialCrateStacks[instruction.FromStack];
            var destination = _initialCrateStacks[instruction.ToStack];
            int moves = instruction.CratesToMove;

            while (moves > 0)
            {
                var createOnTheMove = source.Pop();
                destination.Push(createOnTheMove);
                moves--;
            }
        }
    }

    public string GetTopCratesFromAllStacks()
    {
        var topCrates = _initialCrateStacks.Select(s => s.Value.Peek().Trim('[', ']'));
        return string.Join(string.Empty, topCrates);
    }
}

internal class CrateMover9001 : ICargoCrane
{
    private readonly Dictionary<int, Stack<string>> _initialCrateStacks;
    private readonly List<Rearrangement> _rearrangements;

    public CrateMover9001(Func<Dictionary<int, Stack<string>>> stacksFactory, IEnumerable<Rearrangement> rearrangements)
    {
        _initialCrateStacks = stacksFactory();
        _rearrangements = new(rearrangements);
    }

    public void PerformRearrangementProcedure()
    {
        foreach (Rearrangement instruction in _rearrangements)
        {
            var source = _initialCrateStacks[instruction.FromStack];
            var destination = _initialCrateStacks[instruction.ToStack];
            int moves = instruction.CratesToMove;
            Stack<string> cratesOnTheMove = new(moves);

            while (moves > 0)
            {
                var crateOnTheMove = source.Pop();
                cratesOnTheMove.Push(crateOnTheMove);
                moves--;
            }

            while (cratesOnTheMove.Count > 0)
            {
                var crateOnTheMove = cratesOnTheMove.Pop();
                destination.Push(crateOnTheMove);
            }
        }
    }

    public string GetTopCratesFromAllStacks()
    {
        var topCrates = _initialCrateStacks.Select(s => s.Value.Peek().Trim('[', ']'));
        return string.Join(string.Empty, topCrates);
    }
}