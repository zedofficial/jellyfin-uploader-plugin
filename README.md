# Mobile Uploader Plugin for Jellyfin

A secure, mobile-friendly Jellyfin plugin that provides a REST API for uploading files directly to your Jellyfin libraries from mobile applications.

## Features

🔐 **Jellyfin Authentication** - Uses existing Jellyfin user accounts and permissions
📱 **Mobile-Optimized API** - RESTful endpoints designed for mobile app integration  
📁 **Dynamic Library Access** - Upload to any Jellyfin library the user has access to
🛡️ **Multi-Layer Security** - API key, security token, and app package validation
📂 **Folder Management** - Browse, create, and organize folders within libraries
🎯 **File Type Control** - Configurable allowed file extensions
📊 **Upload Progress** - Real-time upload status and progress tracking

## Quick Start

1. **Install** the plugin in Jellyfin
2. **Configure** API credentials in plugin settings
3. **Integrate** with your mobile app using the provided API
4. **Users log in** with their existing Jellyfin accounts

## API Endpoints

### Authentication
- `POST /api/mobile-uploader/login` - Login with Jellyfin credentials
- `POST /api/mobile-uploader/logout` - Logout and invalidate session  
- `GET /api/mobile-uploader/verify` - Verify current session

### Library Management
- `GET /api/mobile-uploader/libraries` - List accessible libraries
- `GET /api/mobile-uploader/folders` - Browse folders within a library
- `POST /api/mobile-uploader/create-folder` - Create new folders

### File Upload
- `POST /api/mobile-uploader/upload` - Upload files to libraries

## Security Features

- **API Key Authentication** - Prevents unauthorized access to the API
- **Security Token Validation** - Additional security layer with rotating tokens
- **App Package Verification** - Ensures only your mobile app can access the API
- **Jellyfin User Permissions** - Respects existing Jellyfin library access controls
- **Session Management** - Secure session tokens with automatic expiration

## Configuration

The plugin provides a simple configuration interface in Jellyfin:

```
✓ API Key: Unique identifier for your mobile app
✓ Security Token: Additional security validation
✓ App Package: Your mobile app's package identifier
✓ File Size Limits: Maximum upload size per file
✓ File Type Restrictions: Allowed file extensions
✓ Folder Creation: Enable/disable dynamic folder creation
```

## Mobile App Integration

Perfect for Flutter, React Native, or native mobile apps:

```javascript
// Login with Jellyfin credentials
const loginResponse = await fetch('/api/mobile-uploader/login', {
    method: 'POST',
    headers: {
        'X-API-Key': 'your-api-key',
        'X-Security-Token': 'your-security-token',
        'X-App-Package': 'com.yourapp.package',
        'User-Agent': 'YourApp/1.0 MobileUploader'
    },
    body: JSON.stringify({ username, password })
});

// Upload files using session token
const uploadResponse = await fetch('/api/mobile-uploader/upload?libraryId=123', {
    method: 'POST',
    headers: {
        'Authorization': `Bearer ${sessionToken}`,
        // ... other security headers
    },
    body: formData
});
```

## Use Cases

- **Photo backup** from mobile devices to Jellyfin photo libraries
- **Video uploads** for personal media collections  
- **Document sharing** through Jellyfin's web interface
- **Music uploads** for personal audio libraries
- **Automatic syncing** of mobile content to home media server

## Benefits Over Alternatives

- **Native Jellyfin Integration** - No separate user management needed
- **Secure API Design** - Multiple layers of authentication and validation
- **Flexible Library Support** - Works with any Jellyfin library type
- **Mobile-First** - Designed specifically for mobile app development
- **Permission Aware** - Respects existing Jellyfin user access controls

## Installation

1. Download the plugin files to your Jellyfin plugins directory
2. Restart Jellyfin server
3. Navigate to Admin → Plugins → Mobile Uploader
4. Configure your API credentials and upload settings
5. Integrate with your mobile application

## Requirements

- Jellyfin Server 10.8.0 or higher
- .NET 6.0 runtime
- Mobile app with HTTP client capabilities
- Valid Jellyfin user accounts for uploaders

## Documentation

- [Configuration Guide](CONFIGURATION.md) - Detailed setup instructions
- [API Reference](CONFIGURATION.md#api-endpoints) - Complete endpoint documentation  
- [Security Best Practices](CONFIGURATION.md#security-best-practices) - Production deployment guide

## Support

- Report issues through Jellyfin plugin channels
- Check Jellyfin logs for detailed error information
- Verify API credentials match your configuration exactly

---

**Perfect for**: Mobile app developers wanting secure, user-friendly file upload functionality for Jellyfin servers without complex setup or separate authentication systems.
