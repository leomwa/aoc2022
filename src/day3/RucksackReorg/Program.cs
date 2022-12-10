using RucksackReorg;

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