namespace jsnover.net.blazor.Infrastructure.Authorization
{
    /// <summary>
    /// Central policy definitions for photo gallery authorization.
    /// Defines authorization policies and role names for role-based access control.
    /// </summary>
    public static class PhotoGalleryAuthorizationPolicy
    {
        /// <summary>
        /// Policy names for authorization policies registered in Startup.cs
        /// </summary>
        public static class PolicyNames
        {
            /// <summary>
            /// Anyone can view the photo gallery (public access)
            /// </summary>
            public const string ViewPhotoGallery = "ViewPhotoGallery";

            /// <summary>
            /// Guests and authenticated users can submit comments (with verification)
            /// </summary>
            public const string SubmitComment = "SubmitComment";

            /// <summary>
            /// Guests and authenticated users can add reactions
            /// </summary>
            public const string AddReaction = "AddReaction";

            /// <summary>
            /// Admin only: Manage photos (create, edit, delete)
            /// </summary>
            public const string ManagePhotos = "ManagePhotos";

            /// <summary>
            /// Admin only: Moderate comments (approve, reject, delete)
            /// </summary>
            public const string ModerateComments = "ModerateComments";

            /// <summary>
            /// Admin only: Access admin dashboard pages
            /// </summary>
            public const string AdminDashboard = "AdminDashboard";
        }

        /// <summary>
        /// Role names used in the application
        /// </summary>
        public static class Roles
        {
            /// <summary>
            /// Administrator role - Full access to all operations
            /// </summary>
            public const string Admin = "Admin";

            /// <summary>
            /// Authenticated user role - Special privileges for registered users
            /// </summary>
            public const string AuthenticatedUser = "AuthenticatedUser";

            /// <summary>
            /// Guest role - Default for unauthenticated users (implicit, not assigned)
            /// </summary>
            public const string Guest = "Guest";
        }

        /// <summary>
        /// Rate limit settings by privilege level
        /// </summary>
        public static class RateLimits
        {
            /// <summary>
            /// Guest users: Maximum images per minute
            /// </summary>
            public const int GuestImagesPerMinute = 10;

            /// <summary>
            /// Guest users: Maximum reactions per minute
            /// </summary>
            public const int GuestReactionsPerMinute = 5;

            /// <summary>
            /// Authenticated users: Not rate limited (0 = unlimited)
            /// </summary>
            public const int AuthenticatedImagesPerMinute = 0;

            /// <summary>
            /// Authenticated users: Not rate limited (0 = unlimited)
            /// </summary>
            public const int AuthenticatedReactionsPerMinute = 0;

            /// <summary>
            /// Admin users: Not rate limited (0 = unlimited)
            /// </summary>
            public const int AdminImagesPerMinute = 0;

            /// <summary>
            /// Admin users: Not rate limited (0 = unlimited)
            /// </summary>
            public const int AdminReactionsPerMinute = 0;
        }

        /// <summary>
        /// Comment workflow settings
        /// </summary>
        public static class CommentWorkflow
        {
            /// <summary>
            /// Guest comments require email verification before admin review
            /// </summary>
            public const bool GuestRequireVerification = true;

            /// <summary>
            /// Guest comments require admin approval
            /// </summary>
            public const bool GuestRequireApproval = true;

            /// <summary>
            /// Authenticated user comments skip email verification
            /// </summary>
            public const bool AuthenticatedSkipVerification = true;

            /// <summary>
            /// Authenticated user comments can auto-approve (optional, via config)
            /// </summary>
            public const bool AuthenticatedAutoApprove = false;

            /// <summary>
            /// Admin comments auto-approve
            /// </summary>
            public const bool AdminAutoApprove = true;
        }
    }
}
