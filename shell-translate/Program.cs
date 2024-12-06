using DeepL;
using DotNetEnv;

Env.Load();

var authKey = Environment.GetEnvironmentVariable("AUTH_KEY");
var translator = new Translator(authKey);

var translatedText = await translator.TranslateTextAsync(
    "Action required",
    LanguageCode.English,
    LanguageCode.German
);

Console.WriteLine(translatedText);