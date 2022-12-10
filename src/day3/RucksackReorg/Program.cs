const string inputFileName = "rucksacks.txt";
var fileReader = File.OpenText(inputFileName);

int elvesPerGroup = 3;
int membershipCount = 0;
List<ThreeElvesGroup> groups = new();
List<TwoCompartmentRucksack> rucksacksForGroup = new();

while (true)
{
    var contents = await fileReader.ReadLineAsync();
    if (contents is null)
    {
        break;
    }

    TwoCompartmentRucksack rucksack = new(contents);
    rucksacksForGroup.Add(rucksack);
    membershipCount++;

    if (membershipCount % elvesPerGroup == 0)
    {
        groups.Add(new ThreeElvesGroup(rucksacksForGroup));
        rucksacksForGroup = new(); // reset for every 3 elves
    }
}

groups.ForEach(g => g.ComputeRucksackStatistics());

int sumOfItemPriorities = groups.Sum(g => g.GetPrioritySumOfItemsCommonToCompartments());
Console.WriteLine($"Sum of item priorities is: {sumOfItemPriorities}");

int badgePrioritySums = groups.Sum(g => g.GetSumOfBadgeItemPriorities());
Console.WriteLine($"Sum of three-Elf group item priorities is: {badgePrioritySums}");

internal readonly record struct RucksackItem(char ItemType, int Priority);

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
    private class Compartment
    {
        protected internal readonly List<RucksackItem> Items = new(0);

        public Compartment(string contents)
        {
            foreach (var item in contents)
            {
                Items.Add(new(item, ItemPriorityMapper.GetPriority(item)));
            }
        }
    }

    private List<Compartment> Compartments { get; }

    public TwoCompartmentRucksack(string contents)
    {
        int numberOfCompartments = 2;
        Compartments = new(numberOfCompartments);

        InsertIntoCompartments(contents, numberOfCompartments);
    }

    private void InsertIntoCompartments(string contents, int numberOfCompartments)
    {
        int itemsPerCompartment = contents.Length / numberOfCompartments;
        for (int i = 0; i < numberOfCompartments; i++)
        {
            Compartments.Add(new Compartment(contents.Substring(i * itemsPerCompartment, itemsPerCompartment)));
        }
    }

    private int GetPrioritySumOfItemsCommonToCompartments()
    {
        return Compartments[0].Items.Intersect(Compartments[1].Items).Sum(i => i.Priority);
    }

    public void Accept(IRucksackVisitor visitor)
    {
        visitor.VisitRucksackItems(Compartments.SelectMany(c => c.Items));
        visitor.VisitPrioritySumOfItemsCommonToCompartments(GetPrioritySumOfItemsCommonToCompartments());
    }
}

internal class ThreeElvesGroup : IRucksackVisitor
{
    private readonly List<TwoCompartmentRucksack> _rucksacks;
    private readonly List<IEnumerable<RucksackItem>> _itemsInGroup = new();
    private int _prioritySumOfCommonItemsInCompartments;

    public ThreeElvesGroup(IReadOnlyCollection<TwoCompartmentRucksack> rucksacks)
    {
        if (rucksacks.Count != 3)
        {
            throw new ArgumentException("We need 3 rucksacks for this group of elves");
        }

        _rucksacks = rucksacks.ToList();
    }

    public void VisitRucksackItems(IEnumerable<RucksackItem> items)
    {
        _itemsInGroup.Add(items);
    }

    public void VisitPrioritySumOfItemsCommonToCompartments(int sum)
    {
        _prioritySumOfCommonItemsInCompartments += sum;
    }

    public int GetSumOfBadgeItemPriorities()
    {
        return _itemsInGroup[0].Intersect(_itemsInGroup[1]).Intersect(_itemsInGroup[2]).Sum(i => i.Priority);
    }

    public void ComputeRucksackStatistics()
    {
        foreach (var rucksack in _rucksacks)
        {
            rucksack.Accept(this); // visit each rucksack to get stats
        }
    }

    public int GetPrioritySumOfItemsCommonToCompartments() => _prioritySumOfCommonItemsInCompartments;
}

internal interface IRucksackVisitor
{
    void VisitRucksackItems(IEnumerable<RucksackItem> contents);
    void VisitPrioritySumOfItemsCommonToCompartments(int sum);
}

internal interface IVisitableRucksack
{
    void Accept(IRucksackVisitor visitor);
}