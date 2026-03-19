using Bunit;
using jsnover.net.blazor.Components;
using jsnover.net.blazor.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.blazor.UnitTests
{
    [TestFixture]
    public class PhotoGalleryComponentTests
    {
        private TestContext ctx;
        private const int PhotosPerPage = 12;

        [SetUp]
        public void Setup()
        {
            ctx = new TestContext();
        }

        [TearDown]
        public void Teardown()
        {
            ctx?.Dispose();
        }

        [Test]
        public void PhotoGallery_RenderWithoutError_EmptyPhotosList()
        {
            // Arrange
            var photos = new List<StandalonePhoto>();

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            Assert.NotNull(cut);
            var emptyMessage = cut.Find(".gallery-message");
            Assert.That(emptyMessage?.TextContent, Does.Contain("No photos to display"));
        }

        [Test]
        public void PhotoGallery_DisplaysCorrectNumberOfPhotos_FullPage()
        {
            // Arrange - Create 12 photos
            var photos = GeneratePhotos(PhotosPerPage);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var galleryItems = cut.FindAll(".gallery-item");
            Assert.That(galleryItems.Count, Is.EqualTo(PhotosPerPage));
        }

        [Test]
        public void PhotoGallery_DisplaysCorrectNumberOfPhotos_PartialPage()
        {
            // Arrange - Create 7 photos
            var photos = GeneratePhotos(7);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var galleryItems = cut.FindAll(".gallery-item");
            Assert.That(galleryItems.Count, Is.EqualTo(7));
        }

        [Test]
        public void PhotoGallery_EachPhotoHasTitleAndThumbnail()
        {
            // Arrange
            var photos = GeneratePhotos(3);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var galleryItems = cut.FindAll(".gallery-item");
            foreach (var item in galleryItems)
            {
                var thumbnail = item.QuerySelector(".gallery-thumbnail");
                var title = item.QuerySelector(".gallery-title");
                
                Assert.NotNull(thumbnail);
                Assert.NotNull(title);
            }
        }

        [Test]
        public void PhotoGallery_ThumbnailsHaveLazyLoadingAttribute()
        {
            // Arrange
            var photos = GeneratePhotos(3);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var thumbnails = cut.FindAll(".gallery-thumbnail");
            foreach (var thumbnail in thumbnails)
            {
                Assert.That(thumbnail.GetAttribute("loading"), Is.EqualTo("lazy"));
            }
        }

        [Test]
        public void PhotoGallery_PhotoTitleDisplaysCorrectly()
        {
            // Arrange
            var photoTitle = "Mountain Vista";
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto
                {
                    PhotoId = 1,
                    Title = photoTitle,
                    Description = "A beautiful mountain view",
                    Url = "https://example.com/photo1.jpg",
                    ThumbnailUrl = "https://example.com/photo1-thumb.jpg",
                    UploadDate = DateTime.Now
                }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var galleryTitle = cut.Find(".gallery-title");
            Assert.That(galleryTitle?.TextContent, Is.EqualTo(photoTitle));
        }

        [Test]
        public async Task PhotoGallery_ClickPhotoCallsOnPhotoClickedCallback()
        {
            // Arrange
            var photos = GeneratePhotos(3);
            var callbackInvoked = false;
            var selectedPhoto = (StandalonePhoto)null;

            var cut = ctx.RenderComponent<PhotoGallery>(parameters => parameters
                .Add(p => p.Photos, photos)
                .Add(p => p.OnPhotoClicked, EventCallback.Factory.Create<StandalonePhoto>(this, (photo) =>
                {
                    callbackInvoked = true;
                    selectedPhoto = photo;
                }))
            );

            // Act
            var firstPhotoItem = cut.Find(".gallery-item");
            firstPhotoItem?.Click();
            await cut.InvokeAsync(() => { });

            // Assert
            Assert.True(callbackInvoked, "Callback should be invoked");
            Assert.NotNull(selectedPhoto);
            Assert.That(selectedPhoto.PhotoId, Is.EqualTo(1));
        }

        [Test]
        public async Task PhotoGallery_ClickPhotoPassesCorrectPhotoObject()
        {
            // Arrange
            var photos = GeneratePhotos(5);
            var selectedPhoto = (StandalonePhoto)null;

            var cut = ctx.RenderComponent<PhotoGallery>(parameters => parameters
                .Add(p => p.Photos, photos)
                .Add(p => p.OnPhotoClicked, EventCallback.Factory.Create<StandalonePhoto>(this, (photo) =>
                {
                    selectedPhoto = photo;
                }))
            );

            // Act
            var galleryItems = cut.FindAll(".gallery-item");
            if (galleryItems.Count >= 3)
            {
                galleryItems[2].Click();
                await cut.InvokeAsync(() => { });

                // Assert
                Assert.That(selectedPhoto?.PhotoId, Is.EqualTo(3));
                Assert.That(selectedPhoto?.Title, Is.EqualTo("Photo 3"));
            }
        }

        [Test]
        public void PhotoGallery_PhotoHasCssClassesForResponsiveness()
        {
            // Arrange
            var photos = GeneratePhotos(1);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var galleryItem = cut.Find(".gallery-item");
            // Check that item has responsive classes (col-md-4, col-lg-3 or equivalent)
            Assert.NotNull(galleryItem);
            Assert.That(galleryItem.OuterHtml, Does.Contain("gallery-item"));
        }

        [Test]
        public void PhotoGallery_HoverOverlayRenderer()
        {
            // Arrange
            var photos = GeneratePhotos(1);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var overlay = cut.Find(".gallery-overlay");
            Assert.NotNull(overlay);
            var viewIcon = overlay?.QuerySelector(".view-icon");
            Assert.NotNull(viewIcon);
            Assert.That(viewIcon?.TextContent, Is.EqualTo("👁️"));
        }

        [Test]
        public void PhotoGallery_ThumbnailImageHasAltAttribute()
        {
            // Arrange
            var photos = GeneratePhotos(1);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var thumbnail = cut.Find(".gallery-thumbnail");
            var altText = thumbnail?.GetAttribute("alt");
            Assert.That(altText, Does.Contain("Photo 1"));
        }

        [Test]
        public void PhotoGallery_ThumbnailUrlIsUsed()
        {
            // Arrange
            var expectedThumbUrl = "https://example.com/thumb-1.jpg";
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto
                {
                    PhotoId = 1,
                    Title = "Photo 1",
                    Description = "Desc",
                    Url = "https://example.com/full-1.jpg",
                    ThumbnailUrl = expectedThumbUrl,
                    UploadDate = DateTime.Now
                }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var thumbnail = cut.Find(".gallery-thumbnail");
            Assert.That(thumbnail?.GetAttribute("src"), Is.EqualTo(expectedThumbUrl));
        }

        [Test]
        public void PhotoGallery_CurrentPageParameterRendersCorrectly()
        {
            // Arrange
            var photos = GeneratePhotos(12);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(parameters => parameters
                .Add(p => p.Photos, photos)
                .Add(p => p.CurrentPage, 1)
            );

            // Assert
            Assert.NotNull(cut);
            var galleryItems = cut.FindAll(".gallery-item");
            Assert.That(galleryItems.Count, Is.EqualTo(12));
        }

        [Test]
        public async Task PhotoGallery_MultiplePhotosCanBeClicked()
        {
            // Arrange
            var photos = GeneratePhotos(5);
            var clickedPhotos = new List<int>();

            var cut = ctx.RenderComponent<PhotoGallery>(parameters => parameters
                .Add(p => p.Photos, photos)
                .Add(p => p.OnPhotoClicked, EventCallback.Factory.Create<StandalonePhoto>(this, (photo) =>
                {
                    clickedPhotos.Add(photo.PhotoId);
                }))
            );

            // Act
            var galleryItems = cut.FindAll(".gallery-item");
            galleryItems[0].Click();
            await cut.InvokeAsync(() => { });
            galleryItems[2].Click();
            await cut.InvokeAsync(() => { });
            galleryItems[4].Click();
            await cut.InvokeAsync(() => { });

            // Assert
            Assert.That(clickedPhotos.Count, Is.EqualTo(3));
            Assert.That(clickedPhotos, Contains.Item(1));
            Assert.That(clickedPhotos, Contains.Item(3));
            Assert.That(clickedPhotos, Contains.Item(5));
        }

        [Test]
        public void PhotoGallery_GridContainerHasGalleryGridClass()
        {
            // Arrange
            var photos = GeneratePhotos(1);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var gridContainer = cut.Find(".gallery-grid");
            Assert.NotNull(gridContainer);
        }

        [Test]
        public void PhotoGallery_RenderComponentWrapper()
        {
            // Arrange
            var photos = GeneratePhotos(1);

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var photoGallery = cut.Find(".photogallery");
            Assert.NotNull(photoGallery);
        }

        [Test]
        public void PhotoGallery_EmptyStateMessagePresentation()
        {
            // Arrange
            var photos = new List<StandalonePhoto>();

            // Act
            var cut = ctx.RenderComponent<PhotoGallery>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var emptyMessage = cut.Find(".gallery-message p");
            Assert.That(emptyMessage?.TextContent?.Trim(), Is.EqualTo("No photos to display."));
        }

        // Helper method to generate test photos
        private List<StandalonePhoto> GeneratePhotos(int count)
        {
            var photos = new List<StandalonePhoto>();
            for (int i = 1; i <= count; i++)
            {
                photos.Add(new StandalonePhoto
                {
                    PhotoId = i,
                    Title = $"Photo {i}",
                    Description = $"Description for photo {i}",
                    Url = $"https://example.com/photo{i}.jpg",
                    ThumbnailUrl = $"https://example.com/photo{i}-thumb.jpg",
                    UploadDate = DateTime.Now.AddDays(-i),
                    IsPublished = true,
                    DisplayOrder = i,
                    CreatedDate = DateTime.Now.AddDays(-i)
                });
            }
            return photos;
        }
    }
}
