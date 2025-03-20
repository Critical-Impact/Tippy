using System.Collections.Generic;

using Dalamud.Configuration;

namespace Tippy
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class TippyConfig : IPluginConfiguration
    {
        private bool isDirty;
        private bool isEnabled = true;
        private bool isLocked;
        private bool isSoundEnabled = true;
        private bool useClassicFont;
        private bool showIntroMessages = true;
        private bool showDebugWindow;
        private int tipCooldown = 300000;
        private int tipTimeout = 60000;
        private int messageTimeout = 5000;
        private List<string> bannedTipIds = new();
        private int version;
        private string currentAgent = "tippy";

        /// <summary>
        /// Gets or sets a value indicating whether tippy is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get => this.isEnabled;

            set
            {
                this.isEnabled = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tippy is locked in-place.
        /// </summary>
        public bool IsLocked
        {
            get => this.isLocked;

            set
            {
                this.isLocked = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tippy will make sounds.
        /// </summary>
        public bool IsSoundEnabled
        {
            get => this.isSoundEnabled;
            set
            {
                this.isSoundEnabled = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use classic font.
        /// </summary>
        public bool UseClassicFont
        {
            get => this.useClassicFont;
            set
            {
                this.useClassicFont = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show intro messages.
        /// </summary>
        public bool ShowIntroMessages
        {
            get => this.showIntroMessages;
            set
            {
                this.showIntroMessages = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show debug window.
        /// </summary>
        public bool ShowDebugWindow
        {
            get => this.showDebugWindow;
            set
            {
                this.showDebugWindow = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating delay between tips.
        /// </summary>
        public int TipCooldown
        {
            get => this.tipCooldown;
            set
            {
                this.tipCooldown = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how long to keep tip open.
        /// </summary>
        public int TipTimeout
        {
            get => this.tipTimeout;
            set
            {
                this.tipTimeout = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how long to keep message open.
        /// </summary>
        public int MessageTimeout
        {
            get => this.messageTimeout;
            set
            {
                this.messageTimeout = value;
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets list of banned tip ids.
        /// </summary>
        public List<string> BannedTipIds
        {
            get => this.bannedTipIds;
            set
            {
                this.bannedTipIds = value;
                this.IsDirty = true;
            }
        }

        /// <inheritdoc />
        public int Version
        {
            get => this.version;
            set
            {
                this.version = value;
                this.IsDirty = true;
            }
        }

        public bool IsDirty
        {
            get => this.isDirty;
            set => this.isDirty = value;
        }

        public string CurrentAgent
        {
            get => this.currentAgent;
            set
            {
                this.currentAgent = value;
                this.IsDirty = true;
            }
        }

        public void AddBannedTipId(string tipId)
        {
            this.bannedTipIds.Add(tipId);
            this.isDirty = true;
        }

        public void RemoveBannedTipId(string tipId)
        {
            this.bannedTipIds.Remove(tipId);
            this.isDirty = true;
        }
    }
}
