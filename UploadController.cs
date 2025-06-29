using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using MediaBrowser.Controller.Entities;
using System.Text.Json;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Users;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.Uploader.Controllers
{
    [ApiController]
    [Route("api/mobile-uploader")]
    public class MobileUploaderController : ControllerBase
    {
        private readonly ILogger<MobileUploaderController> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IFileSystem _fileSystem;
        private readonly ISessionManager _sessionManager;

        // In-memory storage for user upload tracking (for simplicity)
        private static readonly Dictionary<string, UserDailyUsage> _userDailyUsage = new();

        public MobileUploaderController(
            ILogger<MobileUploaderController> logger,
            ILibraryManager libraryManager,
            IFileSystem fileSystem,
            ISessionManager sessionManager)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _fileSystem = fileSystem;
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Login with Jellyfin credentials - returns session token for API calls
        /// Note: This is a simplified version that requires external authentication
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate app authentication first
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                // Validate request
                if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                // For this simplified version, we'll return a message that users should 
                // authenticate through Jellyfin's standard API first, then use that session token
                // In a production version, you would integrate with Jellyfin's user manager
                
                _logger.LogInformation("Login attempt for user: {Username}", request.Username);

                return Ok(new AuthResponse
                {
                    Success = false,
                    Message = "Please authenticate through Jellyfin's standard API first, then use the session token with other endpoints",
                    SessionToken = null,
                    Username = request.Username,
                    UserId = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { error = "Internal server error during login" });
            }
        }

        /// <summary>
        /// Logout and invalidate session
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult<BaseResponse>> Logout()
        {
            try
            {
                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo != null)
                {
                    // For this version, we'll just log the logout
                    // In a full implementation, you would properly invalidate the session
                    _logger.LogInformation("User logged out: {Username}", sessionInfo.UserName);
                }

                return Ok(new BaseResponse { Success = true, Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "Internal server error during logout" });
            }
        }

        /// <summary>
        /// Verify current session token
        /// </summary>
        [HttpGet("verify")]
        public async Task<ActionResult<VerifyResponse>> Verify()
        {
            try
            {
                // Validate app authentication
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo == null)
                {
                    return Unauthorized(new { error = "Invalid or expired session" });
                }

                var config = Plugin.Instance?.Configuration;
                return Ok(new VerifyResponse
                {
                    Success = true,
                    Message = "Session valid",
                    Username = sessionInfo.UserName,
                    UserId = sessionInfo.UserId.ToString(),
                    UploadsEnabled = config?.EnableUploads ?? false,
                    MaxFileSizeMB = config?.MaxFileSizeMB ?? 100
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session verification");
                return StatusCode(500, new { error = "Internal server error during verification" });
            }
        }

        /// <summary>
        /// Get available libraries for the authenticated user
        /// </summary>
        [HttpGet("libraries")]
        public async Task<ActionResult<LibrariesResponse>> GetLibraries()
        {
            try
            {
                // Validate app authentication and session
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo == null)
                {
                    return Unauthorized(new { error = "Invalid or expired session" });
                }

                var libraries = GetAvailableLibraries();
                
                return Ok(new LibrariesResponse
                {
                    Success = true,
                    Libraries = libraries
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving libraries");
                return StatusCode(500, new { error = "Internal server error retrieving libraries" });
            }
        }

        /// <summary>
        /// Get folders within a specific library
        /// </summary>
        [HttpGet("folders")]
        public async Task<ActionResult<FoldersResponse>> GetFolders([FromQuery] string libraryId, [FromQuery] string? path = null)
        {
            try
            {
                // Validate authentication
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo == null)
                {
                    return Unauthorized(new { error = "Invalid or expired session" });
                }

                if (string.IsNullOrEmpty(libraryId))
                {
                    return BadRequest(new { error = "LibraryId parameter is required" });
                }

                var library = GetLibraryById(libraryId);
                if (library == null)
                {
                    return BadRequest(new { error = $"Library not found: {libraryId}" });
                }

                var folders = GetFoldersInLibrary(library, path);
                
                return Ok(new FoldersResponse
                {
                    Success = true,
                    Folders = folders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving folders");
                return StatusCode(500, new { error = "Internal server error retrieving folders" });
            }
        }

        /// <summary>
        /// Create a new folder in a library
        /// </summary>
        [HttpPost("create-folder")]
        public async Task<ActionResult<BaseResponse>> CreateFolder([FromBody] CreateFolderRequest request)
        {
            try
            {
                // Validate authentication
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo == null)
                {
                    return Unauthorized(new { error = "Invalid or expired session" });
                }

                var config = Plugin.Instance?.Configuration;
                if (!config?.AllowFolderCreation == true)
                {
                    return BadRequest(new { error = "Folder creation is disabled" });
                }

                if (request == null || string.IsNullOrEmpty(request.LibraryId) || string.IsNullOrEmpty(request.FolderName))
                {
                    return BadRequest(new { error = "LibraryId and FolderName are required" });
                }

                var library = GetLibraryById(request.LibraryId);
                if (library == null)
                {
                    return BadRequest(new { error = $"Library not found: {request.LibraryId}" });
                }

                var libraryPath = GetLibraryPath(library);
                var folderPath = string.IsNullOrEmpty(request.ParentPath) 
                    ? Path.Combine(libraryPath, request.FolderName)
                    : Path.Combine(libraryPath, request.ParentPath, request.FolderName);

                if (Directory.Exists(folderPath))
                {
                    return BadRequest(new { error = "Folder already exists" });
                }

                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Created folder: {FolderPath} by user {Username}", folderPath, sessionInfo.UserName);

                return Ok(new BaseResponse { Success = true, Message = "Folder created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder");
                return StatusCode(500, new { error = "Internal server error creating folder" });
            }
        }

        /// <summary>
        /// Upload files to a specific library and folder with premium subscription validation
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<UploadResponse>> Upload(
            [FromQuery] string libraryId,
            [FromQuery] string? folderId = null,
            [FromQuery] bool isPremium = false,
            [FromQuery] string? premiumToken = null,
            [FromForm] IFormFile[] files = null!)
        {
            try
            {
                // Validate authentication
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo == null)
                {
                    return Unauthorized(new { error = "Invalid or expired session" });
                }

                // Check if uploads are enabled
                var config = Plugin.Instance?.Configuration;
                if (config?.EnableUploads != true)
                {
                    return BadRequest(new { error = "Upload functionality is currently disabled" });
                }

                // Validate parameters
                if (string.IsNullOrEmpty(libraryId))
                {
                    return BadRequest(new { error = "LibraryId parameter is required" });
                }

                if (files == null || files.Length == 0)
                {
                    return BadRequest(new { error = "No files provided" });
                }

                // Premium validation
                var userPremiumStatus = await ValidatePremiumStatus(sessionInfo.UserId.ToString(), isPremium, premiumToken);
                
                // Check upload limits for free users
                if (!userPremiumStatus.IsPremium)
                {
                    var limitCheck = await CheckUserUploadLimits(sessionInfo.UserId.ToString(), files);
                    if (!limitCheck.CanUpload)
                    {
                        return BadRequest(new { error = limitCheck.ErrorMessage, upgradeRequired = true });
                    }
                }

                // Find the library
                var library = GetLibraryById(libraryId);
                if (library == null)
                {
                    return BadRequest(new { error = $"Library not found: {libraryId}" });
                }

                var libraryPath = GetLibraryPath(library);
                var targetPath = string.IsNullOrEmpty(folderId) 
                    ? libraryPath 
                    : Path.Combine(libraryPath, folderId);

                // Ensure target directory exists
                if (!Directory.Exists(targetPath))
                {
                    if (config.AllowFolderCreation)
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                    else
                    {
                        return BadRequest(new { error = "Target folder does not exist and folder creation is disabled" });
                    }
                }

                var uploadedFiles = new List<UploadedFileInfo>();
                var totalUploadedSize = 0L;

                foreach (var file in files)
                {
                    // Apply file size limits based on subscription
                    var maxFileSize = userPremiumStatus.IsPremium 
                        ? config.MaxFileSizeMB 
                        : config.FreeUserMaxFileSizeMB;

                    if (file.Length > maxFileSize * 1024 * 1024)
                    {
                        var message = userPremiumStatus.IsPremium 
                            ? $"File {file.FileName} exceeds maximum size of {maxFileSize}MB"
                            : $"File {file.FileName} exceeds free user limit of {maxFileSize}MB. Upgrade to premium for larger files.";
                        return BadRequest(new { error = message, upgradeRequired = !userPremiumStatus.IsPremium });
                    }

                    // Validate file extension based on library type
                    var allowedExtensions = GetAllowedExtensionsForLibrary(config, library);
                    if (allowedExtensions.Any() && !IsFileExtensionAllowed(file.FileName, allowedExtensions))
                    {
                        return BadRequest(new { error = $"File type not allowed for this library: {Path.GetExtension(file.FileName)}" });
                    }

                    var filePath = Path.Combine(targetPath, file.FileName);
                    
                    // Handle file conflicts
                    if (System.IO.File.Exists(filePath))
                    {
                        filePath = GetUniqueFilePath(filePath);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    totalUploadedSize += file.Length;

                    uploadedFiles.Add(new UploadedFileInfo
                    {
                        FileName = Path.GetFileName(filePath),
                        Size = file.Length,
                        Path = filePath
                    });

                    _logger.LogInformation("File uploaded: {FileName} by user {Username} (Premium: {IsPremium})", 
                        filePath, sessionInfo.UserName, userPremiumStatus.IsPremium);
                }

                // Record upload for free users (for daily limits)
                if (!userPremiumStatus.IsPremium)
                {
                    await RecordUserUpload(sessionInfo.UserId.ToString(), uploadedFiles.Count, totalUploadedSize);
                }

                // Refresh library to pick up new files
                await _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);

                return Ok(new UploadResponse
                {
                    Success = true,
                    Message = $"Successfully uploaded {uploadedFiles.Count} file(s)",
                    UploadedFiles = uploadedFiles,
                    IsPremiumUpload = userPremiumStatus.IsPremium
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file upload");
                return StatusCode(500, new { error = "Internal server error during upload" });
            }
        }

        /// <summary>
        /// Get user's current upload limits and premium status
        /// </summary>
        [HttpGet("user-limits")]
        public async Task<ActionResult<UserLimitsResponse>> GetUserLimits([FromQuery] bool isPremium = false, [FromQuery] string? premiumToken = null)
        {
            try
            {
                if (!ValidateAppAuthentication())
                {
                    return Unauthorized(new { error = "Invalid app authentication" });
                }

                var sessionInfo = await GetCurrentSessionInfo();
                if (sessionInfo == null)
                {
                    return Unauthorized(new { error = "Invalid or expired session" });
                }

                var config = Plugin.Instance?.Configuration;
                if (config == null)
                {
                    return StatusCode(500, new { error = "Plugin configuration not available" });
                }

                var userPremiumStatus = await ValidatePremiumStatus(sessionInfo.UserId.ToString(), isPremium, premiumToken);
                var dailyUsage = await GetUserDailyUsage(sessionInfo.UserId.ToString());

                return Ok(new UserLimitsResponse
                {
                    Success = true,
                    IsPremium = userPremiumStatus.IsPremium,
                    DailyUploadLimit = userPremiumStatus.IsPremium ? 0 : config.FreeUserDailyUploadLimit, // 0 = unlimited
                    DailySizeLimitMB = userPremiumStatus.IsPremium ? 0 : config.FreeUserDailySizeLimitMB,
                    MaxFileSizeMB = userPremiumStatus.IsPremium ? config.MaxFileSizeMB : config.FreeUserMaxFileSizeMB,
                    FilesUploadedToday = dailyUsage.FilesUploaded,
                    SizeUploadedTodayMB = dailyUsage.SizeUploadedMB,
                    RemainingFiles = userPremiumStatus.IsPremium ? -1 : Math.Max(0, config.FreeUserDailyUploadLimit - dailyUsage.FilesUploaded),
                    RemainingSizeMB = userPremiumStatus.IsPremium ? -1 : Math.Max(0, config.FreeUserDailySizeLimitMB - dailyUsage.SizeUploadedMB)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user limits");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #region Helper Methods

        private bool ValidateAppAuthentication()
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null) return false;

            // Check API Key
            if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyValues) || 
                apiKeyValues.FirstOrDefault() != config.ApiKey)
            {
                return false;
            }

            // Check Security Token
            if (!Request.Headers.TryGetValue("X-Security-Token", out var tokenValues) || 
                tokenValues.FirstOrDefault() != config.SecurityToken)
            {
                return false;
            }

            // Check App Package
            if (!Request.Headers.TryGetValue("X-App-Package", out var packageValues) || 
                packageValues.FirstOrDefault() != config.AllowedAppPackage)
            {
                return false;
            }

            // Check User Agent
            var userAgent = Request.Headers.UserAgent.FirstOrDefault();
            if (string.IsNullOrEmpty(userAgent) || !userAgent.Contains("MobileUploader"))
            {
                return false;
            }

            return true;
        }

        private Task<SessionInfo?> GetCurrentSessionInfo()
        {
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out var authValues))
                {
                    return Task.FromResult<SessionInfo?>(null);
                }

                var authHeader = authValues.FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Task.FromResult<SessionInfo?>(null);
                }

                var token = authHeader.Substring("Bearer ".Length);
                var sessions = _sessionManager.Sessions.Where(s => s.Id == token);
                
                return Task.FromResult(sessions.FirstOrDefault());
            }
            catch
            {
                return Task.FromResult<SessionInfo?>(null);
            }
        }

        private List<LibraryInfo> GetAvailableLibraries()
        {
            var libraries = new List<LibraryInfo>();
            
            foreach (var folder in _libraryManager.RootFolder.Children.OfType<Folder>())
            {
                if (folder.IsRoot) continue;
                
                libraries.Add(new LibraryInfo
                {
                    Id = folder.Id.ToString(),
                    Name = folder.Name,
                    Path = folder.Path,
                    Type = GetLibraryType(folder)
                });
            }

            return libraries;
        }

        private string GetLibraryType(BaseItem folder)
        {
            return folder switch
            {
                CollectionFolder collectionFolder => collectionFolder.CollectionType ?? "mixed",
                _ => "folder"
            };
        }

        private BaseItem? GetLibraryById(string libraryId)
        {
            if (Guid.TryParse(libraryId, out var id))
            {
                return _libraryManager.GetItemById(id);
            }
            return null;
        }

        private string GetLibraryPath(BaseItem library)
        {
            return library.Path;
        }

        private List<FolderInfo> GetFoldersInLibrary(BaseItem library, string? subPath = null)
        {
            var folders = new List<FolderInfo>();
            var basePath = string.IsNullOrEmpty(subPath) ? library.Path : Path.Combine(library.Path, subPath);
            
            if (!Directory.Exists(basePath)) return folders;

            try
            {
                var directories = Directory.GetDirectories(basePath);
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    folders.Add(new FolderInfo
                    {
                        Name = dirInfo.Name,
                        Path = Path.GetRelativePath(library.Path, dir),
                        IsDirectory = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading folders in {Path}", basePath);
            }

            return folders;
        }

        private List<string> GetAllowedExtensionsForLibrary(PluginConfiguration config, BaseItem library)
        {
            var extensions = new List<string>();
            var libraryType = GetLibraryType(library);
            
            switch (libraryType.ToLowerInvariant())
            {
                case "photos":
                    if (!string.IsNullOrEmpty(config.PhotoExtensions))
                    {
                        extensions.AddRange(config.PhotoExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ext => ext.Trim().ToLowerInvariant()));
                    }
                    break;
                case "movies":
                    if (!string.IsNullOrEmpty(config.MovieExtensions))
                    {
                        extensions.AddRange(config.MovieExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ext => ext.Trim().ToLowerInvariant()));
                    }
                    break;
                case "tvshows":
                    if (!string.IsNullOrEmpty(config.TvShowExtensions))
                    {
                        extensions.AddRange(config.TvShowExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ext => ext.Trim().ToLowerInvariant()));
                    }
                    break;
                case "music":
                    if (!string.IsNullOrEmpty(config.MusicExtensions))
                    {
                        extensions.AddRange(config.MusicExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ext => ext.Trim().ToLowerInvariant()));
                    }
                    break;
                case "books":
                    if (!string.IsNullOrEmpty(config.BookExtensions))
                    {
                        extensions.AddRange(config.BookExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(ext => ext.Trim().ToLowerInvariant()));
                    }
                    break;
                default:
                    // For mixed or unknown libraries, allow all configured extensions
                    var allExtensions = new[] { config.PhotoExtensions, config.VideoExtensions, config.MovieExtensions, 
                                              config.TvShowExtensions, config.AnimeExtensions, config.MusicExtensions, config.BookExtensions };
                    foreach (var extGroup in allExtensions)
                    {
                        if (!string.IsNullOrEmpty(extGroup))
                        {
                            extensions.AddRange(extGroup.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(ext => ext.Trim().ToLowerInvariant()));
                        }
                    }
                    break;
            }

            return extensions.Distinct().ToList();
        }

        private bool IsFileExtensionAllowed(string fileName, List<string> allowedExtensions)
        {
            if (!allowedExtensions.Any()) return true; // No restrictions
            
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension) || allowedExtensions.Contains(extension.TrimStart('.'));
        }

        private string GetUniqueFilePath(string originalPath)
        {
            var directory = Path.GetDirectoryName(originalPath);
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);
            
            var counter = 1;
            string newPath;
            
            do
            {
                newPath = Path.Combine(directory!, $"{fileName}_{counter}{extension}");
                counter++;
            } while (System.IO.File.Exists(newPath));
            
            return newPath;
        }

        private Task<PremiumStatus> ValidatePremiumStatus(string userId, bool isPremium, string? premiumToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return Task.FromResult(new PremiumStatus { IsPremium = false, ErrorMessage = "Configuration not available" });
            }

            // If premium bypass is enabled, treat as premium
            if (config.EnablePremiumBypass)
            {
                return Task.FromResult(new PremiumStatus { IsPremium = true });
            }

            // If not claiming premium, return as free user
            if (!isPremium)
            {
                return Task.FromResult(new PremiumStatus { IsPremium = false });
            }

            // Simple token validation
            if (!string.IsNullOrEmpty(config.PremiumApiKey) && premiumToken == config.PremiumApiKey)
            {
                return Task.FromResult(new PremiumStatus { IsPremium = true });
            }

            // External API validation would go here
            if (!string.IsNullOrEmpty(config.PremiumVerificationEndpoint))
            {
                // TODO: Implement external API validation
                _logger.LogWarning("Premium verification endpoint configured but not implemented");
            }

            return Task.FromResult(new PremiumStatus { IsPremium = false, ErrorMessage = "Invalid premium token" });
        }

        private Task<UploadLimitCheck> CheckUserUploadLimits(string userId, IFormFile[] files)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return Task.FromResult(new UploadLimitCheck { CanUpload = false, ErrorMessage = "Configuration not available" });
            }

            var dailyUsage = GetUserDailyUsage(userId).Result;
            var totalFileSize = files.Sum(f => f.Length);
            var totalFileSizeMB = (int)(totalFileSize / (1024 * 1024));

            // Check daily file count limit
            if (config.FreeUserDailyUploadLimit > 0 && 
                dailyUsage.FilesUploaded + files.Length > config.FreeUserDailyUploadLimit)
            {
                return Task.FromResult(new UploadLimitCheck 
                { 
                    CanUpload = false, 
                    ErrorMessage = $"Daily file upload limit exceeded. Limit: {config.FreeUserDailyUploadLimit}, Current: {dailyUsage.FilesUploaded}" 
                });
            }

            // Check daily size limit
            if (config.FreeUserDailySizeLimitMB > 0 && 
                dailyUsage.SizeUploadedMB + totalFileSizeMB > config.FreeUserDailySizeLimitMB)
            {
                return Task.FromResult(new UploadLimitCheck 
                { 
                    CanUpload = false, 
                    ErrorMessage = $"Daily size upload limit exceeded. Limit: {config.FreeUserDailySizeLimitMB}MB, Current: {dailyUsage.SizeUploadedMB}MB" 
                });
            }

            return Task.FromResult(new UploadLimitCheck { CanUpload = true });
        }

        private Task<UserDailyUsage> GetUserDailyUsage(string userId)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var key = $"{userId}_{today}";
            
            if (_userDailyUsage.TryGetValue(key, out var usage))
            {
                return Task.FromResult(usage);
            }

            return Task.FromResult(new UserDailyUsage { FilesUploaded = 0, SizeUploadedMB = 0 });
        }

        private Task RecordUserUpload(string userId, int fileCount, long totalSize)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var key = $"{userId}_{today}";
            var sizeMB = (int)(totalSize / (1024 * 1024));
            
            if (_userDailyUsage.TryGetValue(key, out var usage))
            {
                usage.FilesUploaded += fileCount;
                usage.SizeUploadedMB += sizeMB;
            }
            else
            {
                _userDailyUsage[key] = new UserDailyUsage
                {
                    FilesUploaded = fileCount,
                    SizeUploadedMB = sizeMB
                };
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Data Models

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class CreateFolderRequest
        {
            public string LibraryId { get; set; } = string.Empty;
            public string FolderName { get; set; } = string.Empty;
            public string? ParentPath { get; set; }
        }

        public class AuthResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? SessionToken { get; set; }
            public string? Username { get; set; }
            public string? UserId { get; set; }
        }

        public class BaseResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class VerifyResponse : BaseResponse
        {
            public string? Username { get; set; }
            public string? UserId { get; set; }
            public bool UploadsEnabled { get; set; }
            public int MaxFileSizeMB { get; set; }
        }

        public class LibrariesResponse : BaseResponse
        {
            public List<LibraryInfo> Libraries { get; set; } = new();
        }

        public class LibraryInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
        }

        public class FoldersResponse : BaseResponse
        {
            public List<FolderInfo> Folders { get; set; } = new();
        }

        public class FolderInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public bool IsDirectory { get; set; }
        }

        public class UploadResponse : BaseResponse
        {
            public List<UploadedFileInfo> UploadedFiles { get; set; } = new();
            public bool IsPremiumUpload { get; set; }
        }

        public class UploadedFileInfo
        {
            public string FileName { get; set; } = string.Empty;
            public long Size { get; set; }
            public string Path { get; set; } = string.Empty;
        }

        public class UserLimitsResponse : BaseResponse
        {
            public bool IsPremium { get; set; }
            public int DailyUploadLimit { get; set; }
            public int DailySizeLimitMB { get; set; }
            public int MaxFileSizeMB { get; set; }
            public int FilesUploadedToday { get; set; }
            public int SizeUploadedTodayMB { get; set; }
            public int RemainingFiles { get; set; }
            public int RemainingSizeMB { get; set; }
        }

        public class PremiumStatus
        {
            public bool IsPremium { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class UploadLimitCheck
        {
            public bool CanUpload { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public class UserDailyUsage
        {
            public int FilesUploaded { get; set; }
            public int SizeUploadedMB { get; set; }
        }

        #endregion
    }
}
