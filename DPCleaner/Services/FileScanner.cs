using DPCleaner.Models;
using Spectre.Console;

namespace DPCleaner.Services;

public class FileScanner
{
    private readonly ScanOptions _options;
    private readonly ConsoleUI _ui;
    private ScanResult _result;
    private DateTime _startTime;
    private int _progressCounter;

    public FileScanner(ScanOptions options, ConsoleUI ui)
    {
        _options = options;
        _ui = ui;
        _result = new ScanResult();
        _progressCounter = 0;
    }

    public ScanResult Execute()
    {
        _startTime = DateTime.Now;
        _result = new ScanResult();
        
        ScanDirectory(_options.RootPath);
        
        _result.ElapsedTime = DateTime.Now - _startTime;
        return _result;
    }

    private void ScanDirectory(string directoryPath)
    {
        try
        {
            _result.DirectoriesScanned++;
            _progressCounter++;

            // Actualizar spinner
            if (_progressCounter % 5 == 0)
            {
                _ui.UpdateSpinner($"Escaneando: [cyan]{Markup.Escape(GetRelativePath(directoryPath))}[/] ([green]{_result.FilesFound}[/] archivos)");
            }

            ProcessFiles(directoryPath);

            // Buscar en subcarpetas recursivamente
            string[] subdirectories = Directory.GetDirectories(directoryPath);
            foreach (string subdirectory in subdirectories)
            {
                ScanDirectory(subdirectory);
            }
        }
        catch (UnauthorizedAccessException)
        {
            _result.ErrorsCount++;
            string relativePath = GetRelativePath(directoryPath);
            _ui.LogWarning("Acceso denegado a carpeta", relativePath);
        }
        catch (Exception ex)
        {
            _result.ErrorsCount++;
            string relativePath = GetRelativePath(directoryPath);
            _ui.LogError("Error al procesar carpeta", relativePath, ex.Message);
        }
    }

    private void ProcessFiles(string directoryPath)
    {
        try
        {
            string[] files = Directory.GetFiles(directoryPath, _options.TargetFilename);

            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);
                    long fileSize = fileInfo.Length;
                    _result.FilesFound++;
                    _result.TotalSizeBytes += fileSize;

                    if (!_options.DryRunMode)
                    {
                        File.Delete(file);
                        _result.FilesDeleted++;
                        _ui.IncrementProgress();
                    }

                    string relativePath = GetRelativePath(Path.GetDirectoryName(file) ?? "");
                    string sizeFormatted = FormatBytes(fileSize);

                    if (_options.DryRunMode)
                    {
                        _ui.LogSimulation("Archivo encontrado", relativePath, sizeFormatted);
                    }
                    else
                    {
                        _ui.LogSuccess("Archivo eliminado", relativePath, sizeFormatted);
                    }
                }
                catch (Exception ex)
                {
                    _result.ErrorsCount++;
                    string relativePath = GetRelativePath(Path.GetDirectoryName(file) ?? "");
                    string action = _options.DryRunMode ? "leer" : "eliminar";
                    _ui.LogError($"Error al {action} archivo", relativePath, ex.Message);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            _result.ErrorsCount++;
            string relativePath = GetRelativePath(directoryPath);
            _ui.LogWarning("Acceso denegado al listar archivos", relativePath);
        }
        catch (Exception ex)
        {
            _result.ErrorsCount++;
            string relativePath = GetRelativePath(directoryPath);
            _ui.LogError("Error al listar archivos", relativePath, ex.Message);
        }
    }

    private string GetRelativePath(string fullPath)
    {
        if (string.IsNullOrEmpty(_options.RootPath) || string.IsNullOrEmpty(fullPath))
            return fullPath;

        try
        {
            string relative = Path.GetRelativePath(_options.RootPath, fullPath);
            return relative == "." ? "[raíz]" : relative;
        }
        catch
        {
            return fullPath;
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
