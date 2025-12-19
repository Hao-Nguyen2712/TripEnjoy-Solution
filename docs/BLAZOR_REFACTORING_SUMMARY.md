# TripEnjoy.Client - Blazor WebAssembly Refactoring

## Overview
The TripEnjoy.Client project has been successfully refactored from an ASP.NET Core MVC application to a modern Blazor WebAssembly application.

## Technology Stack

### Frontend Framework
- **Blazor WebAssembly** (.NET 8.0)
- **MudBlazor 8.15.0** - Material Design component library
- **Blazored.LocalStorage 4.5.0** - Client-side storage for JWT tokens

### Authentication & Authorization
- **Microsoft.AspNetCore.Components.Authorization 8.0.12** - Authentication state management
- **System.IdentityModel.Tokens.Jwt 8.15.0** - JWT token parsing
- **Microsoft.Extensions.Http 10.0.1** - HTTP client extensions

## Project Structure

```
TripEnjoy.Client/
├── Layout/
│   ├── MainLayout.razor         # Main application layout with MudBlazor
│   └── NavMenu.razor            # Navigation menu
├── Models/
│   ├── ApiResponse.cs           # Generic API response wrapper
│   └── AuthModels.cs            # Authentication request/response models
├── Pages/
│   ├── Home.razor               # Landing page
│   └── Login.razor              # Two-step login with OTP
├── Services/
│   ├── AuthenticationService.cs # Authentication API service
│   ├── CustomAuthenticationStateProvider.cs # Auth state provider
│   └── LocalStorageService.cs   # Placeholder for local storage
├── Shared/
│   └── RedirectToLogin.razor    # Redirect component for unauthorized access
├── wwwroot/
│   ├── index.html               # Main HTML file with MudBlazor references
│   └── appsettings.json         # API base address configuration
├── App.razor                    # Root component with routing
├── Program.cs                   # Application entry point
└── _Imports.razor               # Global using directives
```

## Key Features Implemented

### 1. Authentication System
- **Two-step login** process with OTP verification
- **JWT token management** using local storage
- **Automatic authentication state** tracking
- **Protected routes** with authorization

### 2. UI Components
- **MudBlazor theme** integration
- **Responsive navigation** with drawer
- **Material Design** components throughout
- **Snackbar notifications** for user feedback

### 3. API Integration
- **Base API URL**: https://localhost:7199
- **HTTP Client** configured for API calls
- **Service layer** for clean separation of concerns

## Configuration

### API Endpoints Used
- `POST /api/v1/auth/login-step-one` - Initial login with email/password
- `POST /api/v1/auth/login-step-two` - OTP verification
- `POST /api/v1/auth/refresh-token` - Token refresh
- `POST /api/v1/auth/register` - User registration (to be implemented in UI)
- `POST /api/v1/auth/forgot-password` - Password reset (to be implemented in UI)

### Launch Settings
- **HTTPS URL**: https://localhost:7100
- **HTTP URL**: http://localhost:5100
- **Debug Inspector**: Blazor WebAssembly debugging support

## Migration Notes

### Removed Components (Old MVC)
- Controllers (AuthController, HomeController, Partner Controllers)
- Razor Views (.cshtml files)
- ViewModels (replaced with Models)
- MVC Areas (Partner area)
- Cookie-based authentication (replaced with JWT)
- Server-side rendering

### New Components (Blazor WASM)
- Razor Components (.razor files)
- Client-side routing
- JWT token authentication
- Local storage for persistence
- MudBlazor UI components
- Client-side state management

## Development Guidelines

### Adding New Pages
1. Create a new `.razor` file in the `Pages/` directory
2. Add `@page "/route"` directive at the top
3. Use MudBlazor components for UI consistency
4. Inject required services (e.g., `@inject IAuthenticationService AuthService`)

### Authentication Flow
1. User enters email/password on Login page
2. `LoginStepOne` sends credentials to API
3. API sends OTP to user's email
4. User enters OTP on same page
5. `LoginStepTwo` validates OTP and receives JWT tokens
6. Tokens stored in browser's local storage
7. `CustomAuthenticationStateProvider` manages authentication state

### Service Pattern
- All API calls go through dedicated service classes
- Services return `ApiResponse<T>` for consistency
- Error handling is centralized in services
- UI components only handle presentation logic

## Next Steps

### Immediate Priorities
1. **Add remaining auth pages**: Register, Forgot Password
2. **Implement Partner pages**: Property management, Document upload
3. **Add booking functionality**: Property listing, booking creation
4. **Error handling**: Better error display and logging
5. **Loading states**: Skeleton screens and loading indicators

### Future Enhancements
1. **Offline support**: Service workers and PWA features
2. **Real-time updates**: SignalR integration
3. **Advanced search**: Property filtering and search
4. **User profile**: Account management pages
5. **Responsive design**: Mobile-first improvements

## CORS Configuration

The API at https://localhost:7199 needs to allow requests from https://localhost:7100. Ensure CORS is configured in the API's `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:7100")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// Before app.UseAuthorization()
app.UseCors();
```

## Running the Application

### Prerequisites
- .NET 8 SDK
- API server running at https://localhost:7199

### Commands
```bash
# Navigate to the project directory
cd src/TripEnjoyServer/TripEnjoy.Client

# Run the application
dotnet run

# Or run with HTTPS profile
dotnet run --launch-profile https

# Build the project
dotnet build

# Publish for deployment
dotnet publish -c Release
```

### Browser Access
- Navigate to: https://localhost:7100
- The application will load in the browser
- Login page is the entry point for authentication

## Troubleshooting

### Build Issues
- Ensure all NuGet packages are restored: `dotnet restore`
- Clean build artifacts: `dotnet clean`
- Rebuild: `dotnet build --no-incremental`

### Runtime Issues
- Check browser console for JavaScript errors
- Verify API is running and accessible
- Check CORS configuration in API
- Verify JWT tokens in browser's local storage (F12 > Application > Local Storage)

### Authentication Issues
- Clear local storage and try again
- Verify API endpoints are correct in appsettings.json
- Check token expiration times
- Ensure OTP email delivery is working

## Summary

The refactoring from ASP.NET MVC to Blazor WebAssembly provides:
- ✅ **Modern SPA architecture** with client-side rendering
- ✅ **Better user experience** with instant page transitions
- ✅ **Reduced server load** as rendering happens on client
- ✅ **Material Design** with MudBlazor components
- ✅ **Type-safe development** with C# throughout
- ✅ **Component reusability** with Blazor components
- ✅ **Improved maintainability** with clearer separation of concerns

The foundation is now in place for building out the remaining features of the TripEnjoy platform.
