namespace RucksackReorg;

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

    internal static int GetPriority(char item)
    {
        return ItemPriorityMap[item];
    }
}