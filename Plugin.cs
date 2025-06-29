using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Uploader
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public override string Name => "Uploader";

        public override Guid Id => Guid.Parse("12345678-1234-5678-9012-123456789012");

        public override string Description => "Provides file upload functionality with custom API endpoints";

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) 
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public static Plugin? Instance { get; private set; }
    }
}