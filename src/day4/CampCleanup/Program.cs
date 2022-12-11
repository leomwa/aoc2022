const string inputFileName = "cleanup-area-pairs.txt";
using StreamReader fileReader = File.OpenText(inputFileName);

int fullOverlaps = 0;
int anyOverlaps = 0;
while (true)
{
    string? contents = await fileReader.ReadLineAsync();
    if (contents is null)
    {
        break;
    }

    IEnumerable<Section> assignedSections = contents.Split(',').Select(s =>
    {
        var sectionIds = s.Split('-');
        return new Section(Convert.ToInt32(sectionIds[0]), Convert.ToInt32(sectionIds[1]));
    });

    AssignedGroup assignedGroup = new(assignedSections);
    if (assignedGroup.ContainsFullOverlaps())
    {
        fullOverlaps++;
    }
    if (!assignedGroup.ContainsNoOverlaps())
    {
        anyOverlaps++;
    }
}

Console.WriteLine($"Count of assigned sections with full overlaps: {fullOverlaps}");
Console.WriteLine($"Count of assigned sections with any overlaps: {anyOverlaps}");


internal record Section(int StartId, int EndId);

internal class AssignedGroup
{
    private readonly List<Section> _sections;

    public AssignedGroup(IEnumerable<Section> sections)
    {
        _sections = sections.ToList();
    }

    public bool ContainsFullOverlaps()
    {
        var (aRange, bRange) = GetRangesMinMax();

        return (aRange.Max() >= bRange.Max() &&
                bRange.Min() >= aRange.Min()) ||
               (bRange.Max() >= aRange.Max() &&
                aRange.Min() >= bRange.Min());
    }

    public bool ContainsNoOverlaps()
    {
        var (aRange, bRange) = GetRangesMinMax();

        // given two ranges aX-aN,bY-bN
        // where X and Y are minimums for both, N is max
        // the ranges do not overlap as long as
        // aN < bY OR bN < aX
        
        return 
            aRange.Max() < bRange.Min() || 
            bRange.Max() < aRange.Min();
    }
    
    private (int[] aRange, int[] bRange) GetRangesMinMax()
    {
        var a = _sections[0];
        var b = _sections[1];
        var aRange = new[] { a.StartId, a.EndId };
        var bRange = new[] { b.StartId, b.EndId };
        return (aRange, bRange);
    }
}