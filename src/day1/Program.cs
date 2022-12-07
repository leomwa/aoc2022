const string inputFileName = "food-calories-per-elf-input.txt";
var fileReader = File.OpenText(inputFileName);

ElvesWithFood elvesWithFood = new();
int elfId = 1;
List<FoodItem> foodItems = new();

while (true)
{
    var lineItem = await fileReader.ReadLineAsync();
    if (lineItem is null)
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(lineItem))
    {
        ElfBag elfBagWithFood = new ElfBag(elfId, new List<FoodItem>(foodItems));
        elvesWithFood.Register(elfBagWithFood);

        elfId++;
        foodItems.Clear();
        continue;
    }

    var calories = int.Parse(lineItem);
    foodItems.Add(new FoodItem(calories));
}

(int elf, int totalCalories) = elvesWithFood.GetElfWithMostCalories();
Console.WriteLine($"Elf #{elf} has bag with containing {totalCalories} total calories");

internal record FoodItem(int Calories);
internal record ElfBag(int ElfId, List<FoodItem> FoodItems)
{
    public int TotalCaloriesInBag => FoodItems.Sum(i => i.Calories);
};

internal class ElvesWithFood
{
    private readonly PriorityQueue<ElfBag, int> _bagsByCalories = new();

    public void Register(ElfBag entry)
    {
        _bagsByCalories.Enqueue(entry, -entry.TotalCaloriesInBag);
    }

    public (int Elf, int TotalCalories) GetElfWithMostCalories()
    {
        ElfBag bagWithFood = _bagsByCalories.Dequeue();
        return (bagWithFood.ElfId, bagWithFood.TotalCaloriesInBag);
    }
}