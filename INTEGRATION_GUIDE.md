# Integration Guide for Your Flutter App

## What Changed

The plugin has been updated to use **Jellyfin's native authentication** instead of creating a separate user system. This aligns perfectly with your Flutter app's existing media server integration approach.

## Key Changes Made

### 1. Authentication Flow
- **Before**: Plugin had its own user registration/login system
- **After**: Uses existing Jellyfin user accounts and authentication
- **Your App**: Already designed for this approach with `MediaServer` model

### 2. API Endpoints
- **Login**: `POST /api/mobile-uploader/login` (uses Jellyfin credentials)
- **Verify**: `GET /api/mobile-uploader/verify` (checks session validity)
- **Logout**: `POST /api/mobile-uploader/logout` (invalidates session)
- **Libraries**: `GET /api/mobile-uploader/libraries` (lists accessible libraries)
- **Folders**: `GET /api/mobile-uploader/folders?libraryId=xxx` (browse folders)
- **Upload**: `POST /api/mobile-uploader/upload?libraryId=xxx&folderId=yyy` (upload files)

### 3. Security Headers
All requests require these headers:
```
X-API-Key: your-configured-api-key
X-App-Package: your-configured-app-package  
X-Security-Token: your-configured-security-token
User-Agent: YourApp/1.0 MobileUploader
Authorization: Bearer jellyfin_session_token (for authenticated endpoints)
```

## How to Integrate with Your Flutter App

### 1. Update MediaServer Model
Your existing `MediaServer` model in `lib/models/media_server.dart` is already perfect! No changes needed.

### 2. Update Upload Provider
In `lib/providers/upload_provider.dart`, modify the upload methods to use the new endpoints:

```dart
Future<void> pickAndUploadFiles(MediaServer server, {BuildContext? context, String? destinationPath}) async {
  // ... existing file picking logic ...
  
  // Use new endpoint
  final uploadUrl = '${server.url}/api/mobile-uploader/upload?libraryId=$libraryId';
  
  final response = await _dio.post(
    uploadUrl,
    data: formData,
    options: Options(
      headers: {
        'X-API-Key': 'your-configured-api-key',
        'X-App-Package': 'com.yourcompany.yourmobileapp',
        'X-Security-Token': 'your-configured-security-token',
        'User-Agent': 'YourApp/1.0 MobileUploader',
        'Authorization': 'Bearer ${server.token}', // Use Jellyfin session token
      },
    ),
  );
}
```

### 3. Update Folder Browser Service
In `lib/services/folder_browser_service.dart`, add support for the new folder browsing:

```dart
Future<List<ServerFolder>> browseFolders(MediaServer server, {String? libraryId, String? path}) async {
  final url = '${server.url}/api/mobile-uploader/folders?libraryId=$libraryId&path=${path ?? ''}';
  
  final response = await _dio.get(
    url,
    options: Options(
      headers: {
        'X-API-Key': 'your-configured-api-key',
        'X-App-Package': 'com.yourcompany.yourmobileapp', 
        'X-Security-Token': 'your-configured-security-token',
        'User-Agent': 'YourApp/1.0 MobileUploader',
        'Authorization': 'Bearer ${server.token}',
      },
    ),
  );
  
  // Parse response and return folders
}
```

### 4. Add Library Listing
Create a new method to get available libraries:

```dart
Future<List<LibraryInfo>> getLibraries(MediaServer server) async {
  final url = '${server.url}/api/mobile-uploader/libraries';
  
  final response = await _dio.get(
    url,
    options: Options(
      headers: {
        'X-API-Key': 'your-configured-api-key',
        'X-App-Package': 'com.yourcompany.yourmobileapp',
        'X-Security-Token': 'your-configured-security-token', 
        'User-Agent': 'YourApp/1.0 MobileUploader',
        'Authorization': 'Bearer ${server.token}',
      },
    ),
  );
  
  return (response.data['libraries'] as List)
      .map((lib) => LibraryInfo.fromJson(lib))
      .toList();
}
```

### 5. Remove SFTP Code
You can now remove all the SFTP-related code since everything goes through Jellyfin:
- `lib/services/network_share_service.dart` - Can be removed
- `lib/models/network_share.dart` - Can be removed  
- `lib/providers/share_provider.dart` - Can be removed
- SFTP-related screens and widgets

## Benefits for Your App

1. **Simplified Architecture**: No need for SFTP connections or network shares
2. **Unified Authentication**: Same login flow for all media server operations
3. **Better Security**: Uses Jellyfin's proven authentication system
4. **Dynamic Libraries**: Users can upload to any library they have access to
5. **Folder Management**: Built-in folder browsing and creation
6. **File Type Control**: Configurable file type restrictions

## Plugin Configuration

In Jellyfin Admin → Plugins → Mobile Uploader, configure:

```
API Key: "your-unique-secret-api-key-2025"
App Package: "com.yourcompany.yourmobileapp"  
Security Token: "your-additional-security-token"
Max File Size: 100 MB
Photo Extensions: .jpg,.jpeg,.png,.gif,.webp,.bmp,.tiff,.heic,.raw
Video Extensions: .mp4,.mkv,.avi,.mov,.wmv,.flv,.webm,.m4v,.3gp,.mpg,.mpeg
Allow Folder Creation: ✓
```

## Next Steps

1. **Configure the plugin** with your app's credentials
2. **Update your Flutter app** to use the new endpoints
3. **Remove SFTP dependencies** from your app
4. **Test the integration** with your Jellyfin server
5. **Users can immediately start uploading** with their existing Jellyfin accounts

This approach gives you a much cleaner, more secure, and easier-to-maintain solution that leverages Jellyfin's existing user management and permissions system!
