namespace DPCleaner.Models;

public class ScanResult
{
    public int FilesFound { get; set; }
    public int FilesDeleted { get; set; }
    public int DirectoriesScanned { get; set; }
    public int ErrorsCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    
    public double FilesPerSecond => FilesFound / (ElapsedTime.TotalSeconds > 0 ? ElapsedTime.TotalSeconds : 1);
    public double DirectoriesPerSecond => DirectoriesScanned / (ElapsedTime.TotalSeconds > 0 ? ElapsedTime.TotalSeconds : 1);
}
