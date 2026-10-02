namespace jsnover.net.blazor.Infrastructure.Services
{
    public sealed class FamilyPhotoDigestOptions
    {
        public const string SectionName = "FamilyPhotoDigest";

        public string Recipients { get; set; } = string.Empty;
        public string GalleryUrl { get; set; } = "https://jsnover.net/family/photos";
    }
}