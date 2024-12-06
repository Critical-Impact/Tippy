using System;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;

namespace Tippy
{
    /// <summary>
    /// IPC for Tippy plugin.
    /// </summary>
    public class TippyProvider
    {
        /// <summary>
        /// API Version.
        /// </summary>
        public const string LabelProviderApiVersion = "Tippy.APIVersion";

        /// <summary>
        /// IsInitialized state.
        /// </summary>
        public const string LabelProviderIsInitialized = "Tippy.IsInitialized";

        /// <summary>
        /// Register Tip.
        /// </summary>
        public const string LabelProviderRegisterTip = "Tippy.RegisterTip";

        /// <summary>
        /// Register Message.
        /// </summary>
        public const string LabelProviderRegisterMessage = "Tippy.RegisterMessage";

        private readonly IPluginLog pluginLog;

        /// <summary>
        /// API.
        /// </summary>
        public readonly ITippyAPI API;

        /// <summary>
        /// Provider API Version.
        /// </summary>
        public ICallGateProvider<int>? ProviderAPIVersion;

        /// <summary>
        /// Provider IsInitialized state.
        /// </summary>
        public ICallGateProvider<bool>? ProviderIsInitialized;

        /// <summary>
        /// Register Tip.
        /// </summary>
        public ICallGateProvider<string, bool>? ProviderRegisterTip;

        /// <summary>
        /// Register Message.
        /// </summary>
        public ICallGateProvider<string, bool>? ProviderRegisterMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TippyProvider"/> class.
        /// </summary>
        /// <param name="pluginInterface">plugin interface.</param>
        /// <param name="api">plugin api.</param>
        public TippyProvider(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog, ITippyAPI api)
        {
            this.pluginLog = pluginLog;
            this.API = api;

            try
            {
                this.ProviderAPIVersion = pluginInterface.GetIpcProvider<int>(LabelProviderApiVersion);
                this.ProviderAPIVersion.RegisterFunc(() => api.APIVersion);
            }
            catch (Exception ex)
            {
                pluginLog.Error($"Error registering IPC provider for {LabelProviderApiVersion}:\n{ex}");
            }

            try
            {
                this.ProviderIsInitialized = pluginInterface.GetIpcProvider<bool>(LabelProviderIsInitialized);
                this.ProviderIsInitialized.RegisterFunc(() => api.IsInitialized);
            }
            catch (Exception ex)
            {
                pluginLog.Error($"Error registering IPC provider for {LabelProviderIsInitialized}:\n{ex}");
            }

            try
            {
                this.ProviderRegisterTip = pluginInterface.GetIpcProvider<string, bool>(LabelProviderRegisterTip);
                this.ProviderRegisterTip.RegisterFunc(api.RegisterTip);
            }
            catch (Exception e)
            {
                pluginLog.Error($"Error registering IPC provider for {LabelProviderRegisterTip}:\n{e}");
            }

            try
            {
                this.ProviderRegisterMessage = pluginInterface.GetIpcProvider<string, bool>(LabelProviderRegisterMessage);
                this.ProviderRegisterMessage.RegisterFunc(api.RegisterMessage);
            }
            catch (Exception e)
            {
                pluginLog.Error($"Error registering IPC provider for {LabelProviderRegisterMessage}:\n{e}");
            }

            this.API.IsInitialized = true;
            this.ProviderIsInitialized?.SendMessage();
        }

        /// <summary>
        /// Dispose IPC.
        /// </summary>
        public void Dispose()
        {
            this.API.IsInitialized = false;
            this.ProviderIsInitialized?.SendMessage();
            this.ProviderAPIVersion?.UnregisterFunc();
            this.ProviderIsInitialized?.UnregisterFunc();
            this.ProviderRegisterTip?.UnregisterFunc();
            this.ProviderRegisterMessage?.UnregisterFunc();
        }
    }
}
