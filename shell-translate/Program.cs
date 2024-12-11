using DeepL;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using shell_translate1.commands;
using shell_translate1.di;
using Spectre.Console.Cli;


public static class Program
{
    public static void Main(string[] args)
    {
        Env.Load();

        var authKey = Environment.GetEnvironmentVariable("AUTH_KEY");
        var options = new TranslatorOptions
        {
            appInfo = new AppInfo { AppName = "shell-translate", AppVersion = "1.0.0" }
        };

        var registrations = new ServiceCollection();
        registrations.AddSingleton<Translator>(x => new Translator(authKey, options));
        var registrar = new TypeRegistrar(registrations);

        var app = new CommandApp(registrar);
        app.Configure(config =>
        {
            config.AddCommand<TranslateTextCommand>("text")
                .WithAlias("tt")
                .WithDescription(@"Translates text from one language to another language.")
                .WithExample("text", "Hello World!", "--from", "EN", "--to", "DE");
            config.AddCommand<TranslateFileCommand>("file")
                .WithAlias("f")
                .WithDescription(@"Translates file from one language to another language.")
                .WithExample("file", "C:\\translate.en.txt", "--output", "C:\\translate.de.txt", 
                    "--from", "EN", "--to", "DE");
        });

        app.Run(args);
    }
}

