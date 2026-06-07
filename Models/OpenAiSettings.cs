namespace WebTgBotAssistant.Models;

public class OpenAiSettings
{
    public string Token { get; set; }

    public string Prompt { get; set; }

    public string Uri { get; set; }

    public string Model { get; set; }

    public bool Enabled { get; set; }

    public int MaximumTextLength { get; set; } = 120;
    
    public string MaximumTextAnswer { get; set; }

    public bool IsEnabled() => Enabled
                               && !string.IsNullOrEmpty(Token)
                               && !string.IsNullOrEmpty(Prompt)
                               && !string.IsNullOrEmpty(Uri)
                               && !string.IsNullOrEmpty(Model);
}