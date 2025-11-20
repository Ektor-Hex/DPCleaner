using System.Text;
using Spectre.Console;
using DPCleaner.Models;

namespace DPCleaner.Services;

public class ConsoleUI
{
    private StatusContext? _currentStatusContext;
    private ProgressTask? _currentProgressTask;

    public ConsoleUI()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
    }

    public void ShowHeader()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("DP CLEANER")
                .Centered()
                .Color(Color.Cyan1));
        
        var panel = new Panel("[bold white]Eliminador Recursivo de Archivos v2.0[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Expand();
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void ShowPhaseHeader(string phaseNumber, string phaseTitle, ConsoleColor color = ConsoleColor.Cyan)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule($"[bold {ToSpectreColor(color)}]{phaseNumber} │ {phaseTitle}[/]");
        rule.Style = Style.Parse(ToSpectreColor(color));
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public void ShowInfoBox(params string[] lines)
    {
        var content = string.Join(Environment.NewLine, lines);
        var panel = new Panel(content)
            .Border(BoxBorder.Rounded)
            .Padding(1, 1);
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public string PromptInput(string prompt, string? placeholder = null)
    {
        var p = new TextPrompt<string>($"[yellow]{prompt}[/]")
            .PromptStyle("white")
            .AllowEmpty();

        if (!string.IsNullOrEmpty(placeholder))
        {
            p.DefaultValue(placeholder);
            p.ShowDefaultValue();
        }

        return AnsiConsole.Prompt(p);
    }

    public bool PromptConfirmation(string message, int filesCount, string sizeFormatted)
    {
        AnsiConsole.WriteLine();
        var panel = new Panel($"[bold yellow]⚠ CONFIRMACIÓN REQUERIDA[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Yellow)
            .Expand();
        AnsiConsole.Write(panel);

        AnsiConsole.MarkupLine($"  Se encontraron [cyan]{filesCount:N0}[/] archivo(s) que ocupan [yellow]{sizeFormatted}[/].");
        AnsiConsole.MarkupLine($"  [red bold]ESTOS ARCHIVOS SERÁN ELIMINADOS PERMANENTEMENTE.[/]");
        AnsiConsole.WriteLine();

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(message)
                .PageSize(3)
                .AddChoices(new[] {
                    "🔴 ELIMINAR ARCHIVOS",
                    "🟢 Cancelar Operación"
                }));

        return selection == "🔴 ELIMINAR ARCHIVOS";
    }

    // --- Progress Methods ---

    public void RunSpinner(string title, Action action)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start(title, ctx =>
            {
                _currentStatusContext = ctx;
                try
                {
                    action();
                }
                finally
                {
                    _currentStatusContext = null;
                }
            });
    }

    public void UpdateSpinner(string message)
    {
        if (_currentStatusContext != null)
        {
            _currentStatusContext.Status(message);
        }
    }

    public void RunProgressBar(string title, int totalItems, Action action)
    {
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[] 
            {
                new TaskDescriptionColumn(),    // Task description
                new ProgressBarColumn(),        // Progress bar
                new PercentageColumn(),         // Percentage
                new SpinnerColumn(),            // Spinner
            })
            .Start(ctx =>
            {
                _currentProgressTask = ctx.AddTask($"[green]{title}[/]", new ProgressTaskSettings
                {
                    MaxValue = totalItems,
                    AutoStart = true
                });

                try
                {
                    action();
                }
                finally
                {
                    _currentProgressTask = null;
                }
            });
    }

    public void IncrementProgress(double amount = 1)
    {
        _currentProgressTask?.Increment(amount);
    }

    // --- Logging Methods ---

    public void LogSuccess(string action, string location, string? details = null)
    {
        string msg = $"[green]✓ {action}[/] [cyan]{Markup.Escape(location)}[/]";
        if (!string.IsNullOrEmpty(details)) msg += $" [grey]({Markup.Escape(details)})[/]";
        AnsiConsole.MarkupLine(msg);
    }

    public void LogWarning(string action, string location)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠ {action}[/] [cyan]{Markup.Escape(location)}[/]");
    }

    public void LogError(string action, string location, string error)
    {
        AnsiConsole.MarkupLine($"[red]✗ {action}[/] [cyan]{Markup.Escape(location)}[/]");
        AnsiConsole.MarkupLine($"      └─ [grey]{Markup.Escape(error)}[/]");
    }

    public void LogSimulation(string action, string location, string? details = null)
    {
        // Formato más limpio: Icono - Archivo (Detalles)
        string msg = $"  [yellow]⟲[/] [cyan]{Markup.Escape(location)}[/]";
        if (!string.IsNullOrEmpty(details)) msg += $" [grey]({Markup.Escape(details)})[/]";
        AnsiConsole.MarkupLine(msg);
    }

    public void ShowMetrics(ScanResult result, bool isDryRun)
    {
        AnsiConsole.WriteLine();
        var table = new Table();
        table.Border(TableBorder.Rounded);
        
        string title = isDryRun ? "RESULTADOS DEL ANÁLISIS" : "MÉTRICAS FINALES";
        table.Title($"[bold cyan]{title}[/]");

        table.AddColumn("Métrica");
        table.AddColumn("Valor");

        table.AddRow("📁 Carpetas analizadas", result.DirectoriesScanned.ToString("N0"));
        table.AddRow("🔍 Archivos encontrados", $"[cyan]{result.FilesFound:N0}[/]");
        
        if (!isDryRun && result.FilesDeleted > 0)
        {
            table.AddRow("✓ Archivos eliminados", $"[green]{result.FilesDeleted:N0}[/]");
        }

        string sizeLabel = isDryRun ? "Espacio a liberar" : "Espacio liberado";
        table.AddRow($"💾 {sizeLabel}", $"[yellow]{FormatBytes(result.TotalSizeBytes)}[/]");

        if (result.ErrorsCount > 0)
        {
            table.AddRow("✗ Errores", $"[red]{result.ErrorsCount:N0}[/]");
        }

        table.AddRow("⏱ Tiempo", $"[magenta]{result.ElapsedTime.TotalSeconds:F2}s[/]");
        
        AnsiConsole.Write(table);
        
        AnsiConsole.MarkupLine($"[grey]⚡ Velocidad: {result.DirectoriesPerSecond:F1} dirs/s │ {result.FilesPerSecond:F1} files/s[/]");
        AnsiConsole.WriteLine();
    }

    public void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓ {message}[/]");
    }

    public void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red bold]✗ ERROR:[/] {message}");
    }

    public void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[white]{message}[/]");
    }

    public void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠ {message}[/]");
    }

    public void WaitForExit()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Presione cualquier tecla para salir...[/]");
        Console.ReadKey(true);
    }

    private string ToSpectreColor(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Red => "red",
            ConsoleColor.Green => "green",
            ConsoleColor.Blue => "blue",
            ConsoleColor.Cyan => "cyan",
            ConsoleColor.Yellow => "yellow",
            ConsoleColor.Magenta => "magenta",
            ConsoleColor.White => "white",
            ConsoleColor.Gray => "grey",
            _ => "white"
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
