using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using DeepL;
using DeepL.Model;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

namespace shell_translate1.commands;

public class TranslateTextCommand : Command<TranslateTextCommand.Settings>
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
        
        [CommandArgument(0, "[Text]")]
        [Description("Text to be translated.")]
        public string Text { get; set; }
    }

    public TranslateTextCommand(Translator translator)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var text = settings.Text;
        if (string.IsNullOrEmpty(text))
        {
            text = ClipboardService.GetText();
            if (string.IsNullOrEmpty(text))
            {
                AnsiConsole.Background = Color.Red;
                AnsiConsole.Foreground = Color.Black;
                AnsiConsole.MarkupLine("[red]Error: No text provided for translation.[/]");
                return 0;
            }
            
            var useClipboard = AnsiConsole.Prompt(
                new SelectionPrompt<bool>()
                    .Title("Would you use your content from clipboard?")
                    .AddChoices(new[] { true, false }));
            
            if (!useClipboard)
            {
                text = "";
                return 0;
            }
        }
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .Start("Translating...", ctx => {
                var translated = TranslateText(text, settings.From, settings.To);
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                AnsiConsole.Write(new FigletText($"Shell-Translator v{version}")
                    .Centered()
                    .Color(Color.Green)
                );
                
                var layout = new Layout("Translator")
                    .SplitColumns(
                        new Layout("Left"),
                        new Layout("Right")
                    );

                var leftPanel = new Panel(
                    Align.Left(
                        new Markup(text)
                    )
                );
                leftPanel.Expand = true;
                leftPanel.Header = new PanelHeader(new CultureInfo(settings.From).DisplayName);
                
                layout["Left"].Update(
                    leftPanel
                );
                
                var rightPanel = new Panel(
                    Align.Left(
                        new Markup($"[blueviolet bold]{Markup.Escape(translated.Result.Text)}[/]")
                    )
                );
                rightPanel.Expand = true;
                rightPanel.Header = new PanelHeader(new CultureInfo(settings.To).DisplayName);
                
                layout["Right"].Update(
                    rightPanel
                );
                AnsiConsole.Write(layout);
            });
        return 0;
    }

    private async Task<TextResult> TranslateText(string text, string from, string to)
    {
        var translatedText = await _translator.TranslateTextAsync(
            text, from, to
        );
        return translatedText;
    }
}