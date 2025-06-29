using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Uploader
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// API key required for authentication - should match your mobile app
        /// </summary>
        public string ApiKey { get; set; } = "your-unique-api-key-here";

        /// <summary>
        /// Your mobile app's package name for additional validation
        /// </summary>
        public string AllowedAppPackage { get; set; } = "com.yourcompany.yourapp";

        /// <summary>
        /// Additional security token that changes periodically
        /// </summary>
        public string SecurityToken { get; set; } = "your-security-token";

        /// <summary>
        /// Enable/disable the upload functionality
        /// </summary>
        public bool EnableUploads { get; set; } = true;

        /// <summary>
        /// Maximum file size in MB
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 100;

        /// <summary>
        /// Allow users to create new folders dynamically
        /// </summary>
        public bool AllowFolderCreation { get; set; } = true;

        /// <summary>
        /// Allowed photo file extensions (comma-separated, with or without dots)
        /// </summary>
        public string PhotoExtensions { get; set; } = ".jpg,.jpeg,.png,.gif,.webp,.bmp,.tiff,.heic,.raw";

        /// <summary>
        /// Allowed video file extensions (comma-separated, with or without dots)
        /// </summary>
        public string VideoExtensions { get; set; } = ".mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.3gp,.mpg,.mpeg";

        /// <summary>
        /// Allowed movie file extensions (comma-separated, with or without dots)
        /// </summary>
        public string MovieExtensions { get; set; } = ".mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.ts,.m2ts,.iso,.img,.vob,.ifo,.bup";

        /// <summary>
        /// Allowed TV show file extensions (comma-separated, with or without dots)
        /// </summary>
        public string TvShowExtensions { get; set; } = ".mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.ts,.m2ts,.mpg,.mpeg,.ogv";

        /// <summary>
        /// Allowed anime file extensions (comma-separated, with or without dots)
        /// </summary>
        public string AnimeExtensions { get; set; } = ".mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.ogv,.rm,.rmvb,.asf";

        /// <summary>
        /// Allowed music file extensions (comma-separated, with or without dots)
        /// </summary>
        public string MusicExtensions { get; set; } = ".mp3,.flac,.wav,.aac,.ogg,.wma,.m4a,.opus,.ape,.dsd,.dsf,.dff";

        /// <summary>
        /// Allowed book/audiobook file extensions (comma-separated, with or without dots)
        /// </summary>
        public string BookExtensions { get; set; } = ".pdf,.epub,.mobi,.azw,.azw3,.cbr,.cbz,.cb7,.cbt,.mp3,.m4a,.m4b,.aax,.aa,.flac";

        /// <summary>
        /// Maximum folder depth allowed for uploads (prevents deep nesting)
        /// </summary>
        public int MaxFolderDepth { get; set; } = 3;

        /// <summary>
        /// Maximum number of files free users can upload per day (0 = unlimited)
        /// </summary>
        public int FreeUserDailyUploadLimit { get; set; } = 10;

        /// <summary>
        /// Maximum total file size in MB for free users per day (0 = unlimited)
        /// </summary>
        public int FreeUserDailySizeLimitMB { get; set; } = 500;

        /// <summary>
        /// Maximum file size in MB for free users per single upload
        /// </summary>
        public int FreeUserMaxFileSizeMB { get; set; } = 50;

        /// <summary>
        /// Premium subscription verification endpoint (optional)
        /// </summary>
        public string PremiumVerificationEndpoint { get; set; } = "";

        /// <summary>
        /// Premium subscription API key for verification (optional)
        /// </summary>
        public string PremiumApiKey { get; set; } = "";

        /// <summary>
        /// Enable premium features bypass (for testing or local premium)
        /// </summary>
        public bool EnablePremiumBypass { get; set; } = false;

        /// <summary>
        /// Maximum number of files premium users can upload (0 = unlimited for premium bypass)
        /// </summary>
        public int PremiumUserMaxFiles { get; set; } = 0;

        /// <summary>
        /// Maximum total size in MB for premium users (0 = unlimited)
        /// </summary>
        public int PremiumUserMaxSizeMB { get; set; } = 0;
    }
}