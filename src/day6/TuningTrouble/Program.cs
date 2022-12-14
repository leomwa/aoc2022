/*
 * For each character, add to tracker object
 * count number of character seen so far
 * then compute if last 4 characters are unique
 * if true, break and report
 */

const string inputFileName = "tuning-trouble-input.txt";
using StreamReader fileReader = File.OpenText(inputFileName);
string input = await fileReader.ReadToEndAsync();

StartOfMarkerDetector markerDetector = new(4, 14);
foreach (char c in input)
{
    markerDetector.Add(c);
}

markerDetector.CreateReport();

public class StartOfMarkerDetector
{
    private readonly int _startOfPacketChars;
    private readonly int _startOfMessageChars;
    private int _startOfPacketIndex;
    private int _startOfMessageIndex;
    private readonly List<char> _packetChars = new();
    private readonly List<char> _messageChars = new();

    private bool StartOfPacketMarkerDetected { get; set; }
    private bool StartOfMessageMarkerDetected { get; set; }


    public StartOfMarkerDetector(int startOfPacketChars, int startOfMessageChars)
    {
        _startOfPacketChars = startOfPacketChars;
        _startOfMessageChars = startOfMessageChars;
    }

    public void Add(char c)
    {
        if (!StartOfPacketMarkerDetected)
        {
            _startOfPacketIndex++;
            _packetChars.Add(c);
            CheckForStartOfPacketMarker();
        }

        if (!StartOfMessageMarkerDetected)
        {
            _startOfMessageIndex++;
            _messageChars.Add(c);
            CheckForStartOfMessageMarker();
        }
    }

    private void CheckForStartOfPacketMarker()
    {
        if (_packetChars.Count < _startOfPacketChars)
        {
            return;
        }

        if (_packetChars.Count >= _startOfPacketChars)
        {
            var set = new HashSet<char>();

            if (_packetChars.ToArray()[^_startOfPacketChars..].Any(c => !set.Add(c)))
            {
                return;
            }

            if (set.Count != _startOfPacketChars)
            {
                return;
            }
        }

        StartOfPacketMarkerDetected = true;
    }

    private void CheckForStartOfMessageMarker()
    {
        if (_messageChars.Count < _startOfMessageChars)
        {
            return;
        }

        if (_messageChars.Count >= _startOfMessageChars)
        {
            var set = new HashSet<char>();

            if (_messageChars.ToArray()[^_startOfMessageChars..].Any(c => !set.Add(c)))
            {
                return;
            }

            if (set.Count != _startOfMessageChars)
            {
                return;
            }
        }

        StartOfMessageMarkerDetected = true;
    }

    public void CreateReport()
    {
        Console.WriteLine($"Characters processed before start-of-marker packet: {_startOfPacketIndex}");
        Console.WriteLine($"Characters processed before start-of-message packet: {_startOfMessageIndex}");
    }
}