using System;
using System.IO;
using System.Media;

// The code searches a few common places and plays the first file it finds.
public class Audioplay
{
    // file name to look for (default: greeting.wav)
    private readonly string _fileName;

    // Constructor: store the file name but do not play automatically
    public Audioplay(string fileName = "greeting.wav")
    {
        _fileName = fileName;
    }

    // Try to find the WAV file and play it (non-blocking)
    public void PlayGreeting()
    {
        // Find the actual file path (may be in obj folder)
        var path = ResolveObjPath(_fileName);
        Console.WriteLine(path ?? "FILE NOT FOUND");
        if (path == null) return; // file not found, nothing to do

        try
        {
            // Create a SoundPlayer for the file and start playback
            using var player = new SoundPlayer(path);
            player.PlaySync(); // Play() returns immediately
        }
        catch
        {
            // Ignore any errors so the app continues running
        }
    }

    // Search for fileName in a few likely locations, including parent 'obj' folders
    private string? ResolveObjPath(string fileName)
    {
        // 1) Current working directory
        var cwdCandidate = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, fileName));
        if (File.Exists(cwdCandidate))
            return cwdCandidate;

        // 2) App base directory (where the exe is running)
        var baseCandidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, fileName));
        if (File.Exists(baseCandidate))
            return baseCandidate;

        // 3) Walk up from the app base and look for obj, obj/Debug, obj/Release
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 10 && dir != null; i++)
        {
            var objPath = Path.Combine(dir.FullName, "obj", fileName);
            if (File.Exists(objPath))
                return Path.GetFullPath(objPath);

            var objDebug = Path.Combine(dir.FullName, "obj", "Debug", fileName);
            if (File.Exists(objDebug))
                return Path.GetFullPath(objDebug);

            var objRelease = Path.Combine(dir.FullName, "obj", "Release", fileName);
            if (File.Exists(objRelease))
                return Path.GetFullPath(objRelease);

            // move to parent directory and try again
            dir = dir.Parent;
        }

        // not found
        return null;
    }
}
