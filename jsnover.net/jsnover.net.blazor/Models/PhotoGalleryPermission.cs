namespace jsnover.net.blazor.Models
{
    /// <summary>
    /// Photo Gallery Permission levels for role-based access control
    /// </summary>
    public static class PhotoGalleryPermission
    {
        public const string AdminRole = "Admin";
        public const string AuthenticatedUserRole = "AuthenticatedUser";
        public const string GuestRole = "Guest";

        public static class Operations
        {
            public const string ViewPhotos = "ViewPhotos";
            public const string ViewComments = "ViewComments";
            public const string AddComment = "AddComment";
            public const string AddReaction = "AddReaction";
            public const string ManagePhotos = "ManagePhotos";  // Admin only
            public const string ModerateComments = "ModerateComments";  // Admin only
        }
    }
}
