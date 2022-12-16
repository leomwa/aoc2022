const string inputFileName = "treetop-tree-house-input.txt";
using StreamReader fileReader = File.OpenText(inputFileName);

Dictionary<int, List<int>> mapInput = new();
int gridRow = 0;
while (true)
{
    var input = await fileReader.ReadLineAsync();
    if (input is null)
    {
        break;
    }

    IEnumerable<int> heights = input.Select(c => Convert.ToInt32(c.ToString()));
    mapInput.TryAdd(gridRow, heights.ToList());
    gridRow++;
}

int y = mapInput.Count;
int x = mapInput.First().Value.Count;

EdgeOfGrid edgeOfGrid = new(
    TopLeft: new(0, 0),
    TopRight: new(0, x - 1),
    BottomRight: new(y - 1, x - 1),
    BottomLeft: new(0, y - 1));

int visibleTreeTops = 0;
int highestScenicScore = 0;

for (int yAxis = 0; yAxis < mapInput.Count; yAxis++) // col
{
    for (int xAxis = 0; xAxis < mapInput[yAxis].Count; xAxis++) // row
    {
        Coordinates position = new(xAxis, yAxis);
        int height = mapInput[yAxis][xAxis];
        TreeTop currentTree = new(position, height);

        var row = mapInput[yAxis].Select((h, xIndex) => new TreeTop(new(xIndex, yAxis), h))
            .Where(tree => tree.Position != currentTree.Position)
            .ToList();

        var column = mapInput.Keys.Select((key, keyIndex) => new TreeTop(new(xAxis, keyIndex), mapInput[key][xAxis]))
            .Where(tree => tree.Position != currentTree.Position)
            .ToList();

        currentTree.CalculateVisibilityFromEdge(row, column, edgeOfGrid);
        currentTree.CalculateScenicScore(row, column, edgeOfGrid);
        
        visibleTreeTops += currentTree.VisibleFromOutsideTheGrid ? 1 : 0;
        highestScenicScore = currentTree.ScenicScore > highestScenicScore ? currentTree.ScenicScore : highestScenicScore;
    }
}

Console.WriteLine($"Total number of tree tops visible from outside the grid: {visibleTreeTops}");
Console.WriteLine($"Highest scenic score for any tree: {highestScenicScore}");

public class TreeTop
{
    private int Height { get; }
    public Coordinates Position { get; }
    public bool VisibleFromOutsideTheGrid { get; private set; }
    public int ScenicScore { get; private set; }

    public TreeTop(Coordinates position, int height)
    {
        Position = position;
        Height = height;
    }

    public void CalculateVisibilityFromEdge(IEnumerable<TreeTop> row, IEnumerable<TreeTop> column, EdgeOfGrid edgeOfGrid)
    {
        if (TreeIsOnTheEdgeOfGrid(edgeOfGrid))
        {
            VisibleFromOutsideTheGrid = true;
            return;
        }

        VisibleFromOutsideTheGrid = TreeIsVisibleInItsRow(row) || TreeIsVisibleInItsColumn(column);
    }

    public void CalculateScenicScore(IEnumerable<TreeTop> row, IEnumerable<TreeTop> column, EdgeOfGrid edgeOfGrid)
    {
        if (TreeIsOnTheEdgeOfGrid(edgeOfGrid))
        {
            ScenicScore = 0;
            return;
        }

        var (left, right) = CalculateRowScenicScore(row);
        var (top, bottom) = CalculateColumnScenicScore(column);

        ScenicScore = left * right * top * bottom;
    }

    private bool TreeIsOnTheEdgeOfGrid(EdgeOfGrid edgeOfGrid)
    {
        return new[] { edgeOfGrid.TopLeft, edgeOfGrid.TopRight, edgeOfGrid.BottomRight, edgeOfGrid.BottomLeft }
            .Any(edge => Position.X == edge.X || Position.Y == edge.Y);
    }

    private bool TreeIsVisibleInItsRow(IEnumerable<TreeTop> row)
    {
        // order by position (X coordinate) to find edge trees using first/last in list
        var treeRow = row.OrderBy(r => r.Position.X).ToList();
        var left = treeRow.First();
        var right = treeRow.Last();

        // if both of the edge tree tops are same height or taller, it wont be visible
        if (left.Height >= Height && right.Height >= Height)
        {
            return false;
        }

        // get tallest trees and check if they are between me and the edges
        // start looking left, then right
        bool visibleFromTheLeft = !treeRow.Where(r => r.Height >= Height).Any(r => r.Position.X < Position.X);
        bool visibleFromTheRight = !treeRow.Where(r => r.Height >= Height).Any(r => r.Position.X > Position.X);

        return visibleFromTheLeft || visibleFromTheRight;
    }

    private bool TreeIsVisibleInItsColumn(IEnumerable<TreeTop> column)
    {
        // order by position (Y coordinate) to find edge trees using first/last in list
        var treeColumn = column.OrderBy(c => c.Position.X).ToList();
        var top = treeColumn.First();
        var bottom = treeColumn.Last();

        // if both of the edge tree tops are same height or taller, it wont be visible
        if (top.Height >= Height && bottom.Height >= Height)
        {
            return false;
        }

        // get tallest trees and check if they are between me and the edges
        // start looking top, then bottom
        bool visibleFromTheTop = !treeColumn.Where(r => r.Height >= Height).Any(r => r.Position.Y < Position.Y);
        bool visibleFromTheBottom = !treeColumn.Where(r => r.Height >= Height).Any(r => r.Position.Y > Position.Y);

        return visibleFromTheTop || visibleFromTheBottom;
    }

    private (int scenicScoreLeft, int scenicScoreRight) CalculateRowScenicScore(IEnumerable<TreeTop> rows)
    {
        var rowsOrdered = rows.OrderBy(r => r.Position.Y).ToList();
        var leftmost = rowsOrdered.First();
        var rightmost = rowsOrdered.Last();

        // for both, the default values should be distance to edge - if no blockers
        var scenicScoreLeft = rowsOrdered.Where(r => r.Height >= Height && r.Position.X < Position.X)
            .Select(p => Position.X - p.Position.X)
            .LastOrDefault(Position.X - leftmost.Position.X); // last = closest to current position
        
        var scenicScoreRight = rowsOrdered.Where(r => r.Height >= Height && r.Position.X > Position.X)
            .Select(p => p.Position.X - Position.X)
            .FirstOrDefault(rightmost.Position.X - Position.X); // first = closest to current position

        return (scenicScoreLeft, scenicScoreRight);
    }

    private (int scenicScoreTop, int scenicScoreBottom) CalculateColumnScenicScore(IEnumerable<TreeTop> cols)
    {
        var colsOrdered = cols.OrderBy(c => c.Position.X).ToList();
        var topmost = colsOrdered.First();
        var bottommost = colsOrdered.Last();

        // for both, the default values should be distance to edge - if no blockers
        var scenicScoreTop = colsOrdered.Where(r => r.Height >= Height && r.Position.Y < Position.Y)
            .Select(p => Position.Y - p.Position.Y)
            .LastOrDefault(Position.Y - topmost.Position.Y); // last = closest to current position
        
        var scenicScoreBottom = colsOrdered.Where(r => r.Height >= Height && r.Position.Y > Position.Y)
            .Select(p => p.Position.Y - Position.Y)
            .FirstOrDefault(bottommost.Position.Y - Position.Y); // first = closest to current position

        return (scenicScoreTop, scenicScoreBottom);
    }
}

public readonly record struct Coordinates(int X, int Y);

public record struct EdgeOfGrid(Coordinates TopLeft, Coordinates TopRight, Coordinates BottomLeft, Coordinates BottomRight);