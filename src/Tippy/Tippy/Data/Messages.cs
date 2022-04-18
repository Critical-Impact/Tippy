using Dalamud;

#pragma warning disable CS1591
namespace Tippy
{
    public static class Messages
    {
        public static readonly Message[] IntroMessages =
        {
            new("INT-000001", Localization.Localize("INT-000001", "Hi, I'm Tippy! I'm your new friend and assistant. I will help you get better at FFXIV!")),
            new("INT-000002", Localization.Localize("INT-000002", "Do you know that I now have configuration options?"), AnimationType.Exclamation),
            new("INT-000003", Localization.Localize("INT-000003", "Ready for a new tip? You can right click on me to request another one!"), AnimationType.Read),
            new("INT-000004", Localization.Localize("INT-000004", "Bored of a tip? You can right click on me to hide it forever!"), AnimationType.TrashTornado),
            new("INT-000005", Localization.Localize("INT-000005", "Other plugins can now send me messages! Ask your favorite plugin developer to integrate with Tippy!"), AnimationType.Writing),
        };
    }
}
