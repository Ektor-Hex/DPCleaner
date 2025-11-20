namespace DPCleaner.Models;

public class ScanOptions
{
    public string TargetFilename { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public bool DryRunMode { get; set; }
}
