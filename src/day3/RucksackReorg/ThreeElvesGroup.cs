namespace RucksackReorg;

internal interface IRucksackVisitor
{
    void VisitRucksackItems(IEnumerable<RucksackItem> contents);
    void VisitPrioritySumOfItemsCommonToCompartments(int sum);
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