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
        // move 7 from 3 to 9
        var instructionsMatchCollection = Regex.Matches(contents, instructionsPattern, options);

        foreach (Match m in instructionsMatchCollection)
        {
            int move = Convert.ToInt32(m.Groups["cratesToMove"].Value);
            int from = Convert.ToInt32(m.Groups["from"].Value);
            int to = Convert.ToInt32(m.Groups["to"].Value);

            rearrangementInstructions.Add(new(move, from, to));
        }
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

Dictionary<int, Stack<string>> startingCrateStacks = stacksAndCratesInput
    .OrderBy(pair => pair.Key)
    .ToDictionary(pair => pair.Key, pair =>
    {
        pair.Value.Reverse();
        return new Stack<string>(pair.Value);
    });

RigOperator rigOperator = new(startingCrateStacks, rearrangementInstructions);
rigOperator.PerformRearrangementProcedure();
string topCrates = rigOperator.GetTopCratesFromAllStacks();

Console.WriteLine($"Top crates from all stacks are: {topCrates}");


public record struct Rearrangement(int CratesToMove, int FromStack, int ToStack);

internal class RigOperator
{
    private readonly Dictionary<int, Stack<string>> _startingCrateStacks;
    private readonly List<Rearrangement> _rearrangements;

    public RigOperator(Dictionary<int, Stack<string>> startingCrateStacks, List<Rearrangement> rearrangements)
    {
        _startingCrateStacks = startingCrateStacks;
        _rearrangements = rearrangements;
    }

    public void PerformRearrangementProcedure()
    {
        foreach (Rearrangement instruction in _rearrangements)
        {
            var source = _startingCrateStacks[instruction.FromStack];
            var destination = _startingCrateStacks[instruction.ToStack];
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
        var topCrates = _startingCrateStacks.Select(s => s.Value.Peek().Trim('[', ']'));
        return string.Join(string.Empty, topCrates);
    }
}