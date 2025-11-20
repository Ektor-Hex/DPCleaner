using DPCleaner.Models;
using DPCleaner.Services;

namespace DPCleaner;

class Application
{
    private readonly ConsoleUI _ui;

    public Application()
    {
        _ui = new ConsoleUI();
    }

    public void Run()
    {
        _ui.ShowHeader();

        // Solicitar datos del usuario
        var options = GetOptionsFromUser();
        if (options == null)
        {
            _ui.WaitForExit();
            return;
        }

        // FASE 1: Análisis y Simulación
        _ui.ShowPhaseHeader("FASE 1", "ANÁLISIS Y SIMULACIÓN", ConsoleColor.Yellow);
        _ui.ShowInfoBox(
            $"📂 Carpeta objetivo: {options.RootPath}",
            $"🔍 Archivo objetivo: {options.TargetFilename}"
        );

        options.DryRunMode = true;
        var scanner = new FileScanner(options, _ui);
        ScanResult analysisResult = new ScanResult();

        _ui.RunSpinner("Analizando sistema de archivos...", () => 
        {
            analysisResult = scanner.Execute();
        });

        _ui.ShowMetrics(analysisResult, isDryRun: true);

        if (analysisResult.FilesFound == 0)
        {
            _ui.ShowInfo($"No se encontraron archivos '{options.TargetFilename}'.");
            _ui.WaitForExit();
            return;
        }

        // FASE 2: Confirmación
        if (!_ui.PromptConfirmation(
            "¿Desea proceder con la eliminación?",
            analysisResult.FilesFound,
            FormatBytes(analysisResult.TotalSizeBytes)))
        {
            _ui.ShowWarning("Operación cancelada por el usuario.");
            _ui.WaitForExit();
            return;
        }

        // FASE 3: Eliminación Real
        _ui.ShowPhaseHeader("FASE 2", "ELIMINACIÓN REAL", ConsoleColor.Red);

        options.DryRunMode = false;
        scanner = new FileScanner(options, _ui);
        ScanResult deletionResult = new ScanResult();

        _ui.RunProgressBar("Eliminando archivos...", analysisResult.FilesFound, () => 
        {
            deletionResult = scanner.Execute();
        });

        _ui.ShowMetrics(deletionResult, isDryRun: false);
        _ui.ShowSuccess($"Operación completada exitosamente. {deletionResult.FilesDeleted:N0} archivo(s) eliminado(s).");

        _ui.WaitForExit();
    }

    private ScanOptions? GetOptionsFromUser()
    {
        // Solicitar nombre del archivo
        string? filename = _ui.PromptInput("Ingrese el nombre del archivo a eliminar:", "@Ekt0r.txt");
        if (string.IsNullOrWhiteSpace(filename))
        {
            _ui.ShowError("Nombre de archivo no proporcionado.");
            return null;
        }

        // Solicitar ruta
        string? path = _ui.PromptInput("Ingrese la ruta de la carpeta a limpiar:", "C:\\Temp");
        if (string.IsNullOrWhiteSpace(path))
        {
            _ui.ShowError("Ruta no proporcionada.");
            return null;
        }

        // Validar que la ruta existe
        if (!Directory.Exists(path))
        {
            _ui.ShowError($"La ruta '{path}' no existe o no es accesible.");
            return null;
        }

        return new ScanOptions
        {
            TargetFilename = filename.Trim(),
            RootPath = path
        };
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
