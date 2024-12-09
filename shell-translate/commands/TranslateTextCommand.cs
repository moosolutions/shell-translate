using System.ComponentModel;
using DeepL;
using DeepL.Model;
using Spectre.Console;
using Spectre.Console.Cli;

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
        
        [CommandArgument(0, "<TEXT>")]
        [Description("Text to be translated.")]
        public string Text { get; set; }
    }

    public TranslateTextCommand(Translator translator)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .Start("Translating...", ctx => {
                var translated = TranslateText(settings.Text, settings.From, settings.To);
                AnsiConsole.MarkupLine($"{settings.Text} = [blueviolet bold]{Markup.Escape(translated.Result.Text)}[/]");
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