const string inputFileName = "rucksacks.txt";
var fileReader = File.OpenText(inputFileName);


int sumOfItemPriorities = 0;
int elvesPerGroup = 3;
int groupCounter = 0;
List<ThreeElvesGroup> groups = new();
ThreeElvesGroup threeElvesGroup = new();

while (true)
{
    var lineItem = await fileReader.ReadLineAsync();
    if (lineItem is null)
    {
        break;
    }

    TwoCompartmentRucksack rucksack = new(lineItem);
    sumOfItemPriorities += rucksack.GetSumOfPrioritiesCommonToCompartments();
    threeElvesGroup.Add(rucksack);
    groupCounter++;

    if (groupCounter % elvesPerGroup == 0)
    {
        groups.Add(threeElvesGroup);
        threeElvesGroup = new(); // reset
    }
}

Console.WriteLine($"Sum of item priorities is: {sumOfItemPriorities}");
int badgePrioritySums = groups.Sum(g=>g.GetSumOfBadgeItemPriorities());
Console.WriteLine($"Sum of three-Elf group item priorities is: {badgePrioritySums}");
    

internal static class ItemPriorityMapper
{
    private static readonly Dictionary<char, int> ItemPriorityMap;

    static ItemPriorityMapper()
    {
        var letters = Enumerable.Range('a', 26).ToArray();

        // a-z lower letters have priority 1-26
        var lowerLettersPriority = letters
            .Select((charId, index) =>
                new KeyValuePair<char, int>((char)charId, index + 1)).ToArray();

        // A-Z upper letters have priority 27-52
        var upperLettersPriority = letters
            .Select((charId, index) =>
                new KeyValuePair<char, int>(char.ToUpperInvariant((char)charId), index + 27)).ToArray();


        ItemPriorityMap = new(lowerLettersPriority.Concat(upperLettersPriority));
    }

    public static int GetPriority(char item)
    {
        return ItemPriorityMap[item];
    }
}

internal class TwoCompartmentRucksack : IVisitableRucksack
{
    class Compartment
    {
        protected internal readonly List<(char, int)> Items = new(0);

        public Compartment(string contents)
        {
            foreach (var item in contents)
            {
                Items.Add((item, ItemPriorityMapper.GetPriority(item)));
            }
        }
    }

    private List<Compartment> Compartments { get; }

    public TwoCompartmentRucksack(string contents)
    {
        Compartments = new(2);

        int itemsPerCompartment = contents.Length / 2;
        InsertIntoCompartments(contents, 2, itemsPerCompartment);
    }

    private void InsertIntoCompartments(string contents, int numberOfCompartments, int itemsPerCompartment)
    {
        for (int i = 0; i < numberOfCompartments; i++)
        {
            Compartments.Add(new Compartment(contents.Substring(i * itemsPerCompartment, itemsPerCompartment)));
        }
    }

    public int GetSumOfPrioritiesCommonToCompartments()
    {
        return Compartments[0].Items.Intersect(Compartments[1].Items).Select(i => i.Item2).Sum();
    }

    public void Accept(IRucksackVisitor visitor)
    {
        visitor.VisitRucksack(Compartments.SelectMany(c => c.Items));
    }
}

internal class ThreeElvesGroup : IRucksackVisitor
{
    private readonly List<TwoCompartmentRucksack> _rucksacks = new(3);
    private readonly List<IEnumerable<(char, int)>> _itemsInGroup = new();

    public void Add(TwoCompartmentRucksack rucksack)
    {
        _rucksacks.Add(rucksack);
    }

    public void VisitRucksack(IEnumerable<(char, int)> items)
    {
        _itemsInGroup.Add(items);
    }

    public int GetSumOfBadgeItemPriorities()
    {
        foreach (var rucksack in _rucksacks)
        {
            rucksack.Accept(this); // collect all items in this group by visiting each rucksack
        }

        return _itemsInGroup[0].Intersect(_itemsInGroup[1]).Intersect(_itemsInGroup[2]).Sum(i => i.Item2);
    }
}

interface IRucksackVisitor
{
    void VisitRucksack(IEnumerable<(char, int)> contents);
}

interface IVisitableRucksack
{
    void Accept(IRucksackVisitor visitor);
}