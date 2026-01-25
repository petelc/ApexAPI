using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Upload profile picture for current user
/// </summary>
public class UploadProfilePictureEndpoint : Endpoint<UploadProfilePictureRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadProfilePictureEndpoint> _logger;

    public UploadProfilePictureEndpoint(
        UserManager<User> userManager,
        ICurrentUserService currentUserService,
        IWebHostEnvironment environment,
        ILogger<UploadProfilePictureEndpoint> logger)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _environment = environment;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users/me/profile-picture");
        AllowFileUploads();
        
        Description(b => b
            .WithTags("Users")
            .WithSummary("Upload profile picture")
            .WithDescription("Uploads a profile picture for the current user. Accepts JPEG, PNG, GIF. Max 5MB."));
    }

    public override async Task HandleAsync(UploadProfilePictureRequest req, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        
        if (userId == Guid.Empty)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not authenticated" }, ct);
            return;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString().ToUpperInvariant());

        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not found" }, ct);
            return;
        }

        var file = req.File;

        if (file == null || file.Length == 0)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "No file uploaded" }, ct);
            return;
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                error = "Invalid file type. Only JPEG, PNG, and GIF are allowed." 
            }, ct);
            return;
        }

        // Validate file size (5MB max)
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new 
            { 
                error = "File too large. Maximum size is 5MB." 
            }, ct);
            return;
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsPath);

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var oldFileName = Path.GetFileName(user.ProfileImageUrl);
                var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            // Generate unique filename
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            // Update user profile
            var imageUrl = $"/uploads/profiles/{fileName}";
            user.ProfileImageUrl = imageUrl;
            user.LastModifiedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Profile picture uploaded: UserId={UserId}, File={FileName}", 
                    userId, fileName);

                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    profileImageUrl = imageUrl,
                    message = "Profile picture uploaded successfully"
                }, ct);
            }
            else
            {
                // Delete uploaded file if user update failed
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _logger.LogWarning("Failed to update user with profile picture: UserId={UserId}", userId);
                
                HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Failed to update user profile"
                }, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture: UserId={UserId}", userId);
            
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "An error occurred while uploading the file"
            }, ct);
        }
    }
}

public class UploadProfilePictureRequest
{
    public IFormFile? File { get; set; }
}
