namespace RucksackReorg;

internal interface IVisitableRucksack
{
    void Accept(IRucksackVisitor visitor);
}

internal readonly record struct RucksackItem(char ItemType, int Priority);

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