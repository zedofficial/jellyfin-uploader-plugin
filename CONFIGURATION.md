# Mobile Uploader Plugin - Configuration Guide

## Quick Setup

1. **Install the plugin** in Jellyfin
2. **Restart Jellyfin** server
3. **Configure the plugin** in Jellyfin Admin → Plugins → Mobile Uploader
4. **Set up your mobile app** with the API credentials
5. **Users log in** with their existing Jellyfin accounts

## Essential Configuration

### 1. Security Settings (Required)

Generate unique values for these settings:

```
API Key: "your-unique-secret-api-key-2025"
Allowed App Package: "com.yourcompany.yourmobileapp"
Security Token: "your-additional-security-token"
```

**Important**: Change these default values! Use long, random strings for security.

### 2. Upload Settings

```
Enable Uploads: ✓ (checked)
Max File Size (MB): 100 (adjust based on your needs)
Allow Folder Creation: ✓ (recommended)
Max Folder Depth: 3 (prevents deep folder nesting)
```

### 3. File Type Settings

Customize allowed file extensions by media type:

```
Photo Extensions: .jpg,.jpeg,.png,.gif,.webp,.bmp,.tiff,.heic,.raw
Video Extensions: .mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.3gp,.mpg,.mpeg
Movie Extensions: .mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.ts,.m2ts,.iso,.img,.vob,.ifo,.bup
TV Show Extensions: .mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.ts,.m2ts,.mpg,.mpeg,.ogv
Anime Extensions: .mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.ogv,.rm,.rmvb,.asf
Music Extensions: .mp3,.flac,.wav,.aac,.ogg,.wma,.m4a,.opus,.ape,.dsd,.dsf,.dff
Book Extensions: .pdf,.epub,.mobi,.azw,.azw3,.cbr,.cbz,.cb7,.cbt,.mp3,.m4a,.m4b,.aax,.aa,.flac
```

### 4. Premium Subscription Settings

Configure upload limits for free vs premium users:

```
Free User Daily Upload Limit: 10 files per day (0 = unlimited)
Free User Daily Size Limit: 500 MB per day (0 = unlimited)  
Free User Max File Size: 50 MB per file
Premium Verification Endpoint: (optional - for external validation)
Premium API Key: (optional - for simple token validation)
Enable Premium Bypass: ✓ (for testing - disables premium validation)
```

## API Endpoints

The plugin creates these endpoints for your mobile app:

### Authentication Endpoints

```
POST /api/mobile-uploader/login      (login with Jellyfin credentials)
POST /api/mobile-uploader/logout     (logout and invalidate session)
GET  /api/mobile-uploader/verify     (verify current session)
```

### Upload Endpoints

```
GET  /api/mobile-uploader/libraries     (list available libraries)
GET  /api/mobile-uploader/folders       (list folders in a library)
POST /api/mobile-uploader/create-folder (create new folder)
POST /api/mobile-uploader/upload        (upload files with premium validation)
GET  /api/mobile-uploader/user-limits   (get user's upload limits and premium status)
```

## User Authentication Flow

### 1. User Logs In with Jellyfin Credentials

```json
POST /api/mobile-uploader/login
{
    "username": "jellyfin_username",
    "password": "jellyfin_password"
}
```

Returns a Jellyfin session token for subsequent API calls.

### 2. All API Calls Use Session Token

```
Authorization: Bearer jellyfin_session_token_here
```

### 3. Upload Files to Libraries

The user can upload to any Jellyfin library they have access to based on their Jellyfin permissions.

## Mobile App Integration

### Required Headers

Every API request must include:

```
X-API-Key: your-configured-api-key
X-App-Package: your-configured-app-package  
X-Security-Token: your-configured-security-token
User-Agent: YourMobileApp/1.0 (must contain "MobileUploader")
Authorization: Bearer session_token (for authenticated endpoints)
```

### Example Mobile Integration

```javascript
// Configure your app
const API_BASE = "http://your-jellyfin-server:8096/api/mobile-uploader";
const API_KEY = "your-unique-secret-api-key-2025";
const APP_PACKAGE = "com.yourcompany.yourmobileapp";
const SECURITY_TOKEN = "your-additional-security-token";

// Login with Jellyfin credentials
async function loginUser(username, password) {
    const response = await fetch(`${API_BASE}/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-API-Key': API_KEY,
            'X-App-Package': APP_PACKAGE,
            'X-Security-Token': SECURITY_TOKEN,
            'User-Agent': 'YourMobileApp/1.0 MobileUploader'
        },
        body: JSON.stringify({ username, password })
    });
    const result = await response.json();
    // Store result.sessionToken for authenticated requests
    return result;
}

// Get available libraries
async function getLibraries(sessionToken) {
    const response = await fetch(`${API_BASE}/libraries`, {
        method: 'GET',
        headers: {
            'X-API-Key': API_KEY,
            'X-App-Package': APP_PACKAGE,
            'X-Security-Token': SECURITY_TOKEN,
            'User-Agent': 'YourMobileApp/1.0 MobileUploader',
            'Authorization': `Bearer ${sessionToken}`
        }
    });
    return response.json();
}

// Upload file with premium validation
async function uploadFile(sessionToken, file, libraryId, folderId, isPremium = false, premiumToken = null) {
    const formData = new FormData();
    formData.append('files', file);
    
    let url = `${API_BASE}/upload?libraryId=${libraryId}`;
    if (folderId) url += `&folderId=${folderId}`;
    if (isPremium) url += `&isPremium=true&premiumToken=${premiumToken}`;
    
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'X-API-Key': API_KEY,
            'X-App-Package': APP_PACKAGE,
            'X-Security-Token': SECURITY_TOKEN,
            'User-Agent': 'YourMobileApp/1.0 MobileUploader',
            'Authorization': `Bearer ${sessionToken}`
        },
        body: formData
    });
    return response.json();
}

// Get user's current limits and premium status
async function getUserLimits(sessionToken, isPremium = false, premiumToken = null) {
    let url = `${API_BASE}/user-limits`;
    if (isPremium) url += `?isPremium=true&premiumToken=${premiumToken}`;
    
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'X-API-Key': API_KEY,
            'X-App-Package': APP_PACKAGE,
            'X-Security-Token': SECURITY_TOKEN,
            'User-Agent': 'YourMobileApp/1.0 MobileUploader',
            'Authorization': `Bearer ${sessionToken}`
        }
    });
    return response.json();
}
```

## Premium Subscription Features

The plugin supports free and premium tiers for your mobile app:

### Free User Limitations:
- **Daily Upload Limit**: 10 files per day (configurable)
- **Daily Size Limit**: 500 MB per day (configurable)  
- **Max File Size**: 50 MB per file (configurable)
- All media types supported based on library permissions

### Premium User Benefits:
- **Unlimited daily uploads** (no file count or size limits)
- **Larger file sizes** (up to configured maximum, e.g., 2GB)
- **Priority support** (faster uploads, no throttling)
- Access to all media libraries they have Jellyfin permissions for

### Premium Validation Options:

**Option 1: External API Validation**
```
Premium Verification Endpoint: https://your-payment-provider.com/verify
Premium API Key: your-payment-provider-api-key
```

**Option 2: Simple Token Validation (for testing)**
```
Premium API Key: premium-test-token-123
Enable Premium Bypass: ✓ (bypasses all validation for testing)
```

**Option 3: No Premium Features**
```
Leave Premium settings empty to disable premium features entirely
```

## User Permissions

Since the plugin uses Jellyfin's native authentication, users need appropriate Jellyfin permissions:
2. Edit the user that will upload files
3. Under **Media** section, ensure they have access to the libraries they need to upload to
4. No special upload permissions are required - the plugin handles file uploads automatically

## Troubleshooting

### Common Issues

1. **"Unauthorized" errors**: 
   - Check that all required headers (API key, security token, app package) match configuration
   - Verify Jellyfin username/password are correct
   - Ensure session token is valid and not expired

2. **"Invalid Jellyfin credentials"**: 
   - User must have a valid Jellyfin account
   - Check username and password are correct
   - Ensure Jellyfin user account is not disabled

3. **"Session expired"**: 
   - User needs to log in again to get a new Jellyfin session token
   - Jellyfin sessions typically last 30 days

4. **Upload failures**: 
   - Check file size doesn't exceed MaxFileSizeMB setting
   - Verify file extension is in allowed extensions list
   - Ensure user has access to the target library in Jellyfin
   - Check that target folder exists or folder creation is enabled

### Security Best Practices

- Use HTTPS in production
- Generate strong, unique API keys and security tokens
- Regularly review Jellyfin user permissions
- Monitor upload activity through Jellyfin logs
- Consider restricting mobile app usage to specific user groups in Jellyfin

### Plugin Data Location

Configuration is stored in:
```
[Jellyfin-Data-Folder]/plugins/configurations/Jellyfin.Plugin.Uploader.xml
```

Backup this file when updating or migrating the plugin.

## API Response Examples

### Successful Login Response
```json
{
    "success": true,
    "message": "Login successful",
    "sessionToken": "jellyfin-session-token-here",
    "username": "jellyfin_user",
    "userId": "user-guid-here"
}
```

### Libraries List Response
```json
{
    "success": true,
    "libraries": [
        {
            "id": "library-guid-1",
            "name": "Movies",
            "path": "/media/movies",
            "type": "movies"
        },
        {
            "id": "library-guid-2", 
            "name": "Photos",
            "path": "/media/photos",
            "type": "photos"
        }
    ]
}
```

### Upload Success Response
```json
{
    "success": true,
    "message": "Successfully uploaded 1 file(s)",
    "uploadedFiles": [
        {
            "fileName": "movie.mp4",
            "size": 1048576000,
            "path": "/media/movies/movie.mp4"
        }
    ],
    "isPremiumUpload": true
}
```

### User Limits Response (Free User)
```json
{
    "success": true,
    "isPremium": false,
    "dailyUploadLimit": 10,
    "dailySizeLimitMB": 500,
    "maxFileSizeMB": 50,
    "filesUploadedToday": 3,
    "sizeUploadedTodayMB": 125.5,
    "remainingFiles": 7,
    "remSizeMB": 374.5
}
```

### User Limits Response (Premium User)
```json
{
    "success": true,
    "isPremium": true,
    "dailyUploadLimit": 0,
    "dailySizeLimitMB": 0,
    "maxFileSizeMB": 2048,
    "filesUploadedToday": 25,
    "sizeUploadedTodayMB": 1250.0,
    "remainingFiles": -1,
    "remSizeMB": -1
}
```

### Upload Limit Exceeded Response
```json
{
    "error": "Daily upload limit exceeded. You can upload 2 more files today. Upgrade to premium for unlimited uploads.",
    "upgradeRequired": true
}
```

## Testing Your Setup

### 1. Test Login

```bash
curl -X POST "http://your-jellyfin-server:8096/api/mobile-uploader/login" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-unique-secret-api-key-2025" \
  -H "X-App-Package: com.yourcompany.yourmobileapp" \
  -H "X-Security-Token: your-additional-security-token" \
  -H "User-Agent: TestApp/1.0 MobileUploader" \
  -d '{"username":"your-jellyfin-username","password":"your-jellyfin-password"}'
```

### 2. Test Libraries (with session token from login)

```bash
curl -X GET "http://your-jellyfin-server:8096/api/mobile-uploader/libraries" \
  -H "X-API-Key: your-unique-secret-api-key-2025" \
  -H "X-App-Package: com.yourcompany.yourmobileapp" \
  -H "X-Security-Token: your-additional-security-token" \
  -H "User-Agent: TestApp/1.0 MobileUploader" \
  -H "Authorization: Bearer YOUR_JELLYFIN_SESSION_TOKEN"
```

### 3. Test File Upload

```bash
curl -X POST "http://your-jellyfin-server:8096/api/mobile-uploader/upload?libraryId=YOUR_LIBRARY_ID" \
  -H "X-API-Key: your-unique-secret-api-key-2025" \
  -H "X-App-Package: com.yourcompany.yourmobileapp" \
  -H "X-Security-Token: your-additional-security-token" \
  -H "User-Agent: TestApp/1.0 MobileUploader" \
  -H "Authorization: Bearer YOUR_JELLYFIN_SESSION_TOKEN" \
  -F "files=@test-movie.mp4"
```

### 4. Test Premium Upload

```bash
curl -X POST "http://your-jellyfin-server:8096/api/mobile-uploader/upload?libraryId=YOUR_LIBRARY_ID&isPremium=true&premiumToken=premium-test-token-123" \
  -H "X-API-Key: your-unique-secret-api-key-2025" \
  -H "X-App-Package: com.yourcompany.yourmobileapp" \
  -H "X-Security-Token: your-additional-security-token" \
  -H "User-Agent: TestApp/1.0 MobileUploader" \
  -H "Authorization: Bearer YOUR_JELLYFIN_SESSION_TOKEN" \
  -F "files=@large-movie.mkv"
```

### 5. Test User Limits

```bash
curl -X GET "http://your-jellyfin-server:8096/api/mobile-uploader/user-limits" \
  -H "X-API-Key: your-unique-secret-api-key-2025" \
  -H "X-App-Package: com.yourcompany.yourmobileapp" \
  -H "X-Security-Token: your-additional-security-token" \
  -H "User-Agent: TestApp/1.0 MobileUploader" \
  -H "Authorization: Bearer YOUR_JELLYFIN_SESSION_TOKEN"
```

## Generating Strong Credentials

### Generate API Key (32 characters)
```bash
# Linux/Mac
openssl rand -base64 24

# Windows PowerShell
[System.Web.Security.Membership]::GeneratePassword(32, 0)
```

### Generate Security Token (24 characters)
```bash
# Linux/Mac
openssl rand -base64 18

# Windows PowerShell
[System.Web.Security.Membership]::GeneratePassword(24, 0)
```

## Comprehensive Media Support

The plugin automatically detects library types and applies appropriate file extension filters:

### Supported Library Types:

**Movies Library:**
- Supports: `.mp4`, `.mkv`, `.avi`, `.mov`, `.wmv`, `.flv`, `.webm`, `.m4v`, `.ts`, `.m2ts`, `.iso`, `.img`, `.vob`, `.ifo`, `.bup`
- Perfect for: Feature films, documentaries, movie collections

**TV Shows Library:**  
- Supports: `.mp4`, `.mkv`, `.avi`, `.mov`, `.wmv`, `.flv`, `.webm`, `.m4v`, `.ts`, `.m2ts`, `.mpg`, `.mpeg`, `.ogv`
- Perfect for: TV series, episodes, seasonal content

**Anime Library:**
- Supports: `.mp4`, `.mkv`, `.avi`, `.mov`, `.wmv`, `.flv`, `.webm`, `.m4v`, `.ogv`, `.rm`, `.rmvb`, `.asf`
- Perfect for: Anime series, movies, OVAs

**Music Library:**
- Supports: `.mp3`, `.flac`, `.wav`, `.aac`, `.ogg`, `.wma`, `.m4a`, `.opus`, `.ape`, `.dsd`, `.dsf`, `.dff`
- Perfect for: Music albums, singles, podcasts

**Photos Library:**
- Supports: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`, `.tiff`, `.heic`, `.raw`
- Perfect for: Photo collections, family albums

**Books Library:**
- Supports: `.pdf`, `.epub`, `.mobi`, `.azw`, `.azw3`, `.cbr`, `.cbz`, `.cb7`, `.cbt`, `.mp3`, `.m4a`, `.m4b`, `.aax`, `.aa`, `.flac`
- Perfect for: E-books, audiobooks, comics

**Mixed Libraries:**
- Supports: All configured file types
- Perfect for: Multi-media collections

## Integration with Your Flutter App

Based on your existing Flutter app code, you can integrate this plugin by:

1. **Update your `MediaServer` model** to use the new API endpoints
2. **Modify your upload service** to use Jellyfin authentication instead of SFTP
3. **Update the authentication flow** to use Jellyfin login instead of registration
4. **Add premium subscription handling** to your payment flow
5. **Implement upload limit checking** before allowing uploads

### Premium Integration Flow:

```javascript
// 1. Check user's premium status and limits
const limits = await getUserLimits(sessionToken, isPremium, premiumToken);

// 2. Show appropriate UI based on limits
if (!limits.isPremium && limits.remainingFiles <= 0) {
    showUpgradePrompt("Daily upload limit reached");
    return;
}

// 3. Upload with premium validation
const result = await uploadFile(sessionToken, file, libraryId, folderId, isPremium, premiumToken);

// 4. Handle upgrade prompts
if (result.upgradeRequired) {
    showUpgradePrompt(result.error);
}
```

The plugin provides the same mobile-friendly API structure your app expects, but now uses Jellyfin's native user system and library management.

## Support

This plugin provides a comprehensive, mobile-friendly media upload API that:

### Core Features:
- ✅ **Jellyfin Native Authentication** - Uses existing user accounts
- ✅ **Library Permission Respect** - Honors Jellyfin access controls  
- ✅ **Dynamic Library Discovery** - Automatically detects available libraries
- ✅ **Comprehensive Media Support** - Movies, TV shows, anime, music, photos, books
- ✅ **Mobile Security Validation** - API keys, tokens, package validation

### Premium Features:
- ✅ **Free/Premium Tiers** - Configurable upload limits for monetization
- ✅ **Upload Limit Management** - Daily file and size restrictions for free users
- ✅ **Premium Validation** - External API or simple token validation
- ✅ **Upgrade Prompts** - Built-in messaging for premium conversion

### Media Library Support:
- 🎬 **Movies** - Full movie file support (MP4, MKV, AVI, ISO, etc.)
- 📺 **TV Shows** - Episode uploads with proper organization
- 🎌 **Anime** - Specialized anime format support
- 🎵 **Music** - High-quality audio formats (FLAC, MP3, etc.)
- 📸 **Photos** - Image collections including RAW formats
- 📚 **Books** - E-books and audiobooks support

### For Development Support:
- Check Jellyfin logs for detailed error messages
- Ensure all API credentials match configuration exactly
- Test with premium bypass enabled first
- Use provided curl examples for API testing

This plugin transforms your Jellyfin server into a powerful, monetizable mobile upload service while maintaining all security and permission controls.
