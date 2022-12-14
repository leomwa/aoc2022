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
            StartOfPacketMarkerDetected = CheckForStartOfMarker(_packetChars, _startOfPacketChars);
        }

        if (!StartOfMessageMarkerDetected)
        {
            _startOfMessageIndex++;
            _messageChars.Add(c);
            StartOfMessageMarkerDetected = CheckForStartOfMarker(_messageChars, _startOfMessageChars);
        }
    }

    private bool CheckForStartOfMarker(List<char> charsToCheck, int charCount)
    {
        if (charsToCheck.Count < charCount)
        {
            // not enough characters to create marker
            return false;
        }

        var marker = new HashSet<char>();
        if (charsToCheck.Count >= charCount)
        {
            // slice array of seen chars by required length
            // ^charCount.. means N chars from the end
            char[] sliceToCheck = charsToCheck.ToArray()[^charCount..];
            
            if (sliceToCheck.Any(c => !marker.Add(c)))
            {
                // duplicate character detected, not the marker
                return false;
            }
        }
        
        Console.WriteLine($"Marker found {string.Join(string.Empty, marker)}");
        return true;
    }

    public void CreateReport()
    {
        Console.WriteLine($"Characters processed before start-of-marker packet: {_startOfPacketIndex}");
        Console.WriteLine($"Characters processed before start-of-message packet: {_startOfMessageIndex}");
    }
}