using System;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy
{
    /// <summary>
    /// IPC for Tippy plugin.
    /// </summary>
    public class TippyProvider : IHostedService, IDisposable
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

        private readonly IDalamudPluginInterface pluginInterface;
        private readonly IPluginLog pluginLog;

        /// <summary>
        /// API.
        /// </summary>
        private readonly ITippyAPI api;

        /// <summary>
        /// Provider API Version.
        /// </summary>
        private ICallGateProvider<int>? providerApiVersion;

        /// <summary>
        /// Provider IsInitialized state.
        /// </summary>
        private ICallGateProvider<bool>? providerIsInitialized;

        /// <summary>
        /// Register Tip.
        /// </summary>
        private ICallGateProvider<string, bool>? providerRegisterTip;

        /// <summary>
        /// Register Message.
        /// </summary>
        private ICallGateProvider<string, bool>? providerRegisterMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TippyProvider"/> class.
        /// </summary>
        /// <param name="pluginInterface">plugin interface.</param>
        /// <param name="api">plugin api.</param>
        public TippyProvider(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog, ITippyAPI api)
        {
            this.pluginInterface = pluginInterface;
            this.pluginLog = pluginLog;
            this.api = api;
        }

        /// <summary>
        /// Dispose IPC.
        /// </summary>
        public void Dispose()
        {
            this.api.IsInitialized = false;
            this.providerIsInitialized?.SendMessage();
            this.providerApiVersion?.UnregisterFunc();
            this.providerIsInitialized?.UnregisterFunc();
            this.providerRegisterTip?.UnregisterFunc();
            this.providerRegisterMessage?.UnregisterFunc();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.providerApiVersion = this.pluginInterface.GetIpcProvider<int>(LabelProviderApiVersion);
                this.providerApiVersion.RegisterFunc(() => this.api.APIVersion);
            }
            catch (Exception ex)
            {
                this.pluginLog.Error($"Error registering IPC provider for {LabelProviderApiVersion}:\n{ex}");
            }

            try
            {
                this.providerIsInitialized = this.pluginInterface.GetIpcProvider<bool>(LabelProviderIsInitialized);
                this.providerIsInitialized.RegisterFunc(() => this.api.IsInitialized);
            }
            catch (Exception ex)
            {
                this.pluginLog.Error($"Error registering IPC provider for {LabelProviderIsInitialized}:\n{ex}");
            }

            try
            {
                this.providerRegisterTip = this.pluginInterface.GetIpcProvider<string, bool>(LabelProviderRegisterTip);
                this.providerRegisterTip.RegisterFunc(this.api.RegisterTip);
            }
            catch (Exception e)
            {
                this.pluginLog.Error($"Error registering IPC provider for {LabelProviderRegisterTip}:\n{e}");
            }

            try
            {
                this.providerRegisterMessage = this.pluginInterface.GetIpcProvider<string, bool>(LabelProviderRegisterMessage);
                this.providerRegisterMessage.RegisterFunc(this.api.RegisterMessage);
            }
            catch (Exception e)
            {
                this.pluginLog.Error($"Error registering IPC provider for {LabelProviderRegisterMessage}:\n{e}");
            }

            this.api.IsInitialized = true;
            this.providerIsInitialized?.SendMessage();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
