using System;
using Tippy.Services;

#pragma warning disable CS1591
namespace Tippy
{
    public class Messages : IDisposable
    {
        private readonly TranslationService translationService;

        public Message[] IntroMessages { get; private set; }

        public Messages(TranslationService translationService)
        {
            this.translationService = translationService;
            this.InitializeMessages();
            translationService.OnNewLanguageLoaded += this.NewLanguageLoaded;
        }

        private void NewLanguageLoaded()
        {
            this.InitializeMessages();
        }

        private void InitializeMessages()
        {
            this.IntroMessages =
            [
                new Message("INT-000001", CheapLoc.Loc.Localize("INT-000001", "Hi, I'm Tippy! I'm your new friend and assistant. I will help you get better at FFXIV!")),
                new Message("INT-000002", CheapLoc.Loc.Localize("INT-000002", "Do you know that I now have configuration options?"), AnimationCategory.Happy),
                new Message("INT-000003", CheapLoc.Loc.Localize("INT-000003", "Ready for a new tip? You can right click on me to request another one!"), AnimationCategory.Attention),
                new Message("INT-000004", CheapLoc.Loc.Localize("INT-000004", "Bored of a tip? You can right click on me to hide it forever!"), AnimationCategory.Random),
                new Message("INT-000005", CheapLoc.Loc.Localize("INT-000005", "Other plugins can now send me messages! Ask your favorite plugin developer to integrate with Tippy!"), AnimationCategory.Searching),
                new Message("INT-000006", CheapLoc.Loc.Localize("INT-000006", "Did you know I lost my voice for awhile? Ask Phil in goat place discord to explain.")),
                new Message("INT-000007", CheapLoc.Loc.Localize("INT-000007", "If you get bored of me, you can change to another agent in settings!"), AnimationCategory.Happy)
            ];
        }

        public void Dispose()
        {
            this.translationService.OnNewLanguageLoaded -= this.NewLanguageLoaded;
        }
    }
}
