using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using vj0.Framework.Models;

namespace vj0.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly Random _random = new();

    public readonly string[] TagLines =
    [
        "the ultimate datamining experience",
        "buy us some time (and coffee ☕)",
        "developed by a dedicated community of contributors",
        "powered by Avalonia and CUEParse",
        "powered by UEDB",
        "bringing clarity to the chaos",
        "look closer.",
        "accidentally pushed into prod",
        "developed with coffee ☕",
        "powered by frustration and breakthroughs",
        "we speak fluent .uasset",
        "we lost the documentation years ago",
        "your .pak files fear us",
        "we run on hope and async",
        "waking up at 3AM to fix one line",
        "open-source, open-minds",
        "the logs know everything...",
        "todo: fix this - 1993",
        "fix scheduled for Q3 1997",
    ];
    
    public readonly string[] Tips =
    [
        "Hover over buttons to see if they have keybinds",
        "Shortcuts make everything faster. Use them",
        "Keyboard shortcuts are your best friend",
        "This tool is community-driven — feedback matters!",
        "Check the logs — they're full of secrets"
    ];
    
    public async void StartRotation(string[] phrases, int rotateTime, TextBlock textBlock, Control? fadingControl = null!, bool useRandom = false)
    {
        if (phrases.Length == 0) return;

        var index = 0;
        var lastIndex = -1;
        fadingControl ??= textBlock;

        var initialPhrase = useRandom ? GetRandomPhrase(phrases, lastIndex, out lastIndex) : phrases[index];
        
        fadingControl.Opacity = 0;
        textBlock.Text = initialPhrase;
        await FadeIn(fadingControl);
        
        if (!useRandom) index++;

        while (true)
        {
            await Task.Delay(rotateTime);

            string nextPhrase;
            if (useRandom)
            {
                nextPhrase = GetRandomPhrase(phrases, lastIndex, out lastIndex);
            }
            else
            {
                nextPhrase = phrases[index % phrases.Length];
                index++;
            }

            await FadeOut(fadingControl);
            textBlock.Text = nextPhrase;
            await FadeIn(fadingControl);
        }
    }
    
    private string GetRandomPhrase(string[] phrases, int lastIndex, out int newIndex)
    {
        if (phrases.Length == 1)
        {
            newIndex = 0;
            return phrases[0];
        }

        var availableIndices = Enumerable.Range(0, phrases.Length)
            .Where(i => i != lastIndex)
            .ToArray();

        newIndex = availableIndices[_random.Next(availableIndices.Length)];
        return phrases[newIndex];
    }
    
    private static async Task FadeOut(Control control)
    {
        const int steps = 10;
        const int delay = 25;

        for (var i = 0; i < steps; i++)
        {
            control.Opacity = 1 - (i / (float)steps);
            await Task.Delay(delay);
        }

        control.Opacity = 0;
    }

    private static async Task FadeIn(Control control)
    {
        const int steps = 10;
        const int delay = 25;

        for (var i = 0; i < steps; i++)
        {
            control.Opacity = i / (float)steps;
            await Task.Delay(delay);
        }

        control.Opacity = 1;
    }
}
