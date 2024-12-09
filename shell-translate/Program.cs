using System.Runtime.InteropServices.Swift;
using DeepL;
using DeepL.Model;
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

        var registrations = new ServiceCollection();
        registrations.AddSingleton<Translator>(x => new Translator(authKey));
        var registrar = new TypeRegistrar(registrations);

        var app = new CommandApp(registrar);
        app.Configure(config =>
        {
            config.AddCommand<TranslateTextCommand>("text")
                .WithAlias("tt")
                .WithDescription(@"Translates text from one language to another language.")
                .WithExample("text", "Hello World!", "--from", "EN", "--to", "DE");
        });

        app.Run(args);
    }
}

