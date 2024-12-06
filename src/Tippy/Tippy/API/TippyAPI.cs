using Dalamud.Plugin.Services;

namespace Tippy
{
    /// <inheritdoc cref="ITippyAPI" />
    public class TippyAPI : ITippyAPI
    {
        private readonly TippyController tippyController;
        private readonly IPluginLog pluginLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="TippyAPI"/> class.
        /// </summary>
        public TippyAPI(TippyController tippyController, IPluginLog pluginLog)
        {
            this.tippyController = tippyController;
            this.pluginLog = pluginLog;
            this.IsInitialized = true;
        }

        /// <inheritdoc />
        public int APIVersion => 1;

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public bool RegisterTip(string text)
        {
            if (!this.CheckInitialized()) return false;
            return this.tippyController.AddTip(text, MessageSource.IPC);
        }

        /// <inheritdoc />
        public bool RegisterMessage(string text)
        {
            if (!this.CheckInitialized()) return false;
            return this.tippyController.AddMessage(text, MessageSource.IPC);
        }

        private bool CheckInitialized()
        {
            if (!this.IsInitialized)
            {
                this.pluginLog.Info("Tippy API is not initialized.");
                return false;
            }

            return true;
        }
    }
}
