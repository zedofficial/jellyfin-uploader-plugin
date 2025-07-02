# Mobile Uploader Plugin for Jellyfin

A secure, mobile-friendly Jellyfin plugin that provides a REST API for uploading files directly to your Jellyfin libraries from mobile applications.

> 🚧 **Note:** The API is currently under active development and will be **exclusively accessible to our official mobile app**.  
> Unauthorized third-party access will be blocked through API key, token, and app package validation.

---

## Features

- 🔐 **Jellyfin Authentication** – Uses existing Jellyfin user accounts and permissions  
- 📱 **Mobile-Optimized API** – RESTful endpoints tailored for mobile app integration  
- 📁 **Dynamic Library Access** – Upload to any Jellyfin library the user has permission for  
- 🛡️ **Multi-Layer Security** – API key, security token, and app package enforcement  
- 📂 **Folder Management** – Browse, create, and organize folders  
- 🎯 **File Type Control** – Configurable file extension restrictions  
- 📊 **Upload Progress** – Real-time progress feedback during uploads  

---

## Development Roadmap

The plugin and API are actively being developed with the following goals:

- 🔒 **Exclusive App Lockdown** – API access will be restricted to our official mobile app using app package and token validation.  
- 🚀 **Stable v1 API** – Core endpoints for login, library browsing, folder control, and file uploads.  
- 🔄 **Planned v2 API** – Future features include background upload queueing, media tagging, and automatic media processing.  

This ensures a secure and seamless experience for verified app users only.

---

## Quick Start

1. **Install** the plugin in Jellyfin  
2. **Configure** API credentials and app restrictions in plugin settings  
3. **Integrate** with your mobile app using the provided REST API  
4. **Log in** with your existing Jellyfin account to begin uploading  

---

## API Endpoints

### 🔐 Authentication

- `POST /api/mobile-uploader/login` – Log in with Jellyfin credentials  
- `POST /api/mobile-uploader/logout` – Log out and invalidate the session  
- `GET /api/mobile-uploader/verify` – Verify the current session and token  

### 📁 Library & Folder Management

- `GET /api/mobile-uploader/libraries` – List accessible Jellyfin libraries  
- `GET /api/mobile-uploader/folders` – Browse folders in a selected library  
- `POST /api/mobile-uploader/create-folder` – Create new folders in a library  

### ⬆️ File Upload

- `POST /api/mobile-uploader/upload` – Upload a file to a specified library/folder  

---

## Security Features

- 🔑 **API Key Authentication** – Only authorized apps can access the API  
- 🔐 **Security Token Validation** – Rotating token system for added security  
- 📦 **App Package Verification** – Enforces access from your official mobile app  
- 👤 **Jellyfin User Permissions** – Honors Jellyfin access control for each user  
- ⏱️ **Session Management** – Auto-expiring secure session tokens  

---

## Plugin Configuration

Inside Jellyfin, go to **Admin → Plugins → Mobile Uploader** and configure:

```
✓ API Key: Unique identifier for your mobile app  
✓ Security Token: Additional token required for access  
✓ App Package: Official app identifier to whitelist  
✓ File Size Limits: Maximum upload size per file  
✓ File Type Restrictions: Define allowed file extensions  
✓ Folder Creation: Enable or disable user-created folders  
```

---

## Mobile App Integration

Supports integration with **Flutter**, **React Native**, and **native Android/iOS** clients:

```javascript
// Example: Login Request
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

// Example: Upload File
const uploadResponse = await fetch('/api/mobile-uploader/upload?libraryId=123', {
    method: 'POST',
    headers: {
        'Authorization': `Bearer ${sessionToken}`,
        // additional headers as needed
    },
    body: formData
});
```

---

## Use Cases

- 📷 **Photo Backup** – Sync mobile photos to Jellyfin libraries  
- 🎥 **Video Uploads** – Upload personal videos directly from your device  
- 📄 **Document Sharing** – Store PDFs or other docs into Jellyfin  
- 🎵 **Music Uploads** – Add new music files to your collection  
- 🔄 **Auto-Sync** – Automatically sync files to your media server  

---

## Benefits Over Alternatives

- ✅ **Native Jellyfin Integration** – No separate account system required  
- 🔐 **Multiple Security Layers** – API key, token, and app checks  
- 📱 **Mobile-First Experience** – Designed with mobile in mind  
- 🧠 **Permission-Aware** – Fully respects Jellyfin user and library access rules  
- 🔧 **Flexible Library Support** – Works with Movies, TV Shows, Music, Photos, and more  

---

## Installation

1. Download the latest `.jpl` plugin release  
2. Place it in your Jellyfin server's `plugins` directory  
3. Restart Jellyfin  
4. Navigate to **Admin → Plugins → Mobile Uploader**  
5. Configure your credentials and usage options  
6. Integrate your mobile app using the API  

---

## Requirements

- Jellyfin Server **v10.8.0+**  
- .NET 6.0 Runtime  
- Mobile app with HTTP client support  
- Valid Jellyfin user account  

---

## Documentation

- [Configuration Guide](CONFIGURATION.md) – How to set up the plugin  
- [API Reference](CONFIGURATION.md#api-endpoints) – All available endpoints  
- [Security Practices](CONFIGURATION.md#security-best-practices) – How to deploy securely  

---

## Support

- 🛠️ Open issues via GitHub if you're an approved app partner  
- 🧪 Check Jellyfin logs for runtime errors  
- ✅ Verify plugin configuration matches your app integration  

---

## License

This plugin is provided as-is for exclusive use with our official mobile app.  
Distribution or reuse outside of authorized applications is **not permitted**.

---

**Built for**: Jellyfin users who want seamless, secure media uploads from their mobile devices — with native login, folder support, and real-time sync.
