const string inputFileName = "rucksacks.txt";
var fileReader = File.OpenText(inputFileName);


int sumOfItemPriorities=0;
while (true)
{
    var lineItem = await fileReader.ReadLineAsync();
    if (lineItem is null)
    {
        break;
    }
    
    TwoCompartmentRucksack rucksack = new(lineItem);
    sumOfItemPriorities += rucksack.GetSumOfPrioritiesCommonToCompartments();
}

Console.WriteLine($"Sum of item priorities is: {sumOfItemPriorities}");

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

public class TwoCompartmentRucksack
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
}