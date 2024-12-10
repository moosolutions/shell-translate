using System.ComponentModel;
using DeepL;
using DeepL.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shell_translate1.commands;

public class TranslateFileCommand : Command<TranslateFileCommand.Settings>
{
    private readonly Translator _translator;
    
    public class Settings : CommandSettings
    {
        [CommandOption("-f|--from <from>")]
        [Description("Language from which to translate text.")]
        public string From { get; set; }
        
        [CommandOption("-t|--to <to>")]
        [DefaultValue(LanguageCode.German)]
        [Description("Language to be translated.")]
        public string To { get; set; }
        
        [CommandOption("-o|--output <output>")]
        [Description("Path to output file.")]
        public string OutputFile { get; set; }
        
        [CommandArgument(0, "<SourceFileName>")]
        [Description("File to be translated.")]
        public string SourceFileName { get; set; }
    }

    public TranslateFileCommand(Translator translator)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .Start("Translating...", ctx => {
                if (string.IsNullOrEmpty(settings.OutputFile))
                {
                    settings.OutputFile = Path.GetTempFileName();
                }
                var translated = TranslateFile(settings.SourceFileName, settings.OutputFile, settings.From, settings.To);
                translated.Wait();
                AnsiConsole.MarkupLine($"Source file: {settings.SourceFileName} translated to file [blueviolet bold]{settings.OutputFile}[/]");
            });
        return 0;
    }

    private async Task TranslateFile(string sourceFileName, string outputFile, string from, string to)
    {
        try
        {
            await _translator.TranslateDocumentAsync(
                new FileInfo(sourceFileName),
                new FileInfo(outputFile),
                from,
                to,
                new DocumentTranslateOptions { Formality = Formality.More }
            );
        }
        catch (DocumentTranslationException e)
        {
            if (e.DocumentHandle != null) {
                var handle = e.DocumentHandle.Value;
                AnsiConsole.MarkupLine("[yellow]Document uploaded correctly but isn't translated yet.[/]");
                AnsiConsole.MarkupLine($"Document ID: {Markup.Escape(handle.DocumentId)}, Document key: {Markup.Escape(handle.DocumentKey)}");
            }
            else
            {
                AnsiConsole.Write(
                    new FigletText("Exception while translating file.")
                        .LeftJustified()
                        .Color(Color.Red));

                var panel = new Panel(e.Message)
                {
                    Border = BoxBorder.Rounded,
                    Padding = new Padding(5, 5, 5, 5),
                    Expand = true,
                    Header = new PanelHeader("Exception message")
                };
                AnsiConsole.Render(panel);
            }
        }
    }
}