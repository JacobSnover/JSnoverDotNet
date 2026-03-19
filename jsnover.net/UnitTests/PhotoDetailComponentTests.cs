using Bunit;
using jsnover.net.blazor.Components;
using jsnover.net.blazor.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace jsnover.net.blazor.UnitTests
{
    [TestFixture]
    public class PhotoDetailComponentTests
    {
        private TestContext ctx;

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
        public void PhotoDetail_RenderWithoutError_NullPhoto()
        {
            // Arrange
            StandalonePhoto photo = null;

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            Assert.NotNull(cut);
        }

        [Test]
        public void PhotoDetail_RenderWithoutError_PopulatedPhoto()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            Assert.NotNull(cut);
            var detailContent = cut.Find(".detail-content");
            Assert.NotNull(detailContent);
        }

        [Test]
        public void PhotoDetail_DisplaysPhotoTitle()
        {
            // Arrange
            var testTitle = "Sunset Over Mountains";
            var photo = CreateTestPhoto(title: testTitle);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var title = cut.Find(".detail-title");
            Assert.That(title?.TextContent, Is.EqualTo(testTitle));
        }

        [Test]
        public void PhotoDetail_DisplaysPhotoDescription()
        {
            // Arrange
            var testDescription = "A beautiful sunset painting the sky in golden hues.";
            var photo = CreateTestPhoto(description: testDescription);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var description = cut.Find(".description");
            Assert.That(description?.TextContent, Is.EqualTo(testDescription));
        }

        [Test]
        public void PhotoDetail_DisplaysUploadDate()
        {
            // Arrange
            var uploadDate = new DateTime(2026, 3, 15);
            var photo = CreateTestPhoto(uploadDate: uploadDate);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var dateElement = cut.Find(".upload-date");
            Assert.That(dateElement?.TextContent, Does.Contain("March 15, 2026"));
        }

        [Test]
        public void PhotoDetail_DisplaysTags()
        {
            // Arrange
            var tags = "landscape, sunset, mountains";
            var photo = CreateTestPhoto(tags: tags);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var tagElements = cut.FindAll(".tag");
            Assert.That(tagElements.Count, Is.EqualTo(3));
            Assert.That(tagElements[0].TextContent, Is.EqualTo("landscape"));
            Assert.That(tagElements[1].TextContent, Is.EqualTo("sunset"));
            Assert.That(tagElements[2].TextContent, Is.EqualTo("mountains"));
        }

        [Test]
        public void PhotoDetail_EmptyTagsNotDisplayed()
        {
            // Arrange
            var photo = CreateTestPhoto(tags: null);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var tagsSection = cut.FindAll(".tags");
            Assert.That(tagsSection.Count, Is.EqualTo(0));
        }

        [Test]
        public void PhotoDetail_LoadsPhotoImageWithLazyLoading()
        {
            // Arrange
            var imageUrl = "https://example.com/large-photo.jpg";
            var photo = CreateTestPhoto(url: imageUrl);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var image = cut.Find(".detail-photo");
            Assert.That(image?.GetAttribute("src"), Is.EqualTo(imageUrl));
            Assert.That(image?.GetAttribute("loading"), Is.EqualTo("lazy"));
        }

        [Test]
        public void PhotoDetail_PhotoImageHasCorrectAltText()
        {
            // Arrange
            var photoTitle = "Mountain Peak";
            var photo = CreateTestPhoto(title: photoTitle);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var image = cut.Find(".detail-photo");
            Assert.That(image?.GetAttribute("alt"), Is.EqualTo(photoTitle));
        }

        [Test]
        public void PhotoDetail_RendersPhotoReactionButtonsComponent()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var reactionsSection = cut.Find(".reactions-section");
            Assert.NotNull(reactionsSection);
            // Look for the PhotoReactionButtons component
            var reactionComponent = cut.FindComponent<PhotoReactionButtons>();
            Assert.NotNull(reactionComponent);
        }

        [Test]
        public void PhotoDetail_PassesPhotoIdToReactionButtons()
        {
            // Arrange
            var photoId = 42;
            var photo = CreateTestPhoto(photoId: photoId);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var reactionComponent = cut.FindComponent<PhotoReactionButtons>();
            Assert.That(reactionComponent.Instance.PhotoId, Is.EqualTo(photoId));
        }

        [Test]
        public void PhotoDetail_RendersPhotoCommentSectionComponent()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var commentsSection = cut.Find(".comments-section");
            Assert.NotNull(commentsSection);
            // Look for the PhotoCommentSection component
            var commentComponent = cut.FindComponent<PhotoCommentSection>();
            Assert.NotNull(commentComponent);
        }

        [Test]
        public void PhotoDetail_PassesPhotoIdToCommentSection()
        {
            // Arrange
            var photoId = 55;
            var photo = CreateTestPhoto(photoId: photoId);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var commentComponent = cut.FindComponent<PhotoCommentSection>();
            Assert.That(commentComponent.Instance.PhotoId, Is.EqualTo(photoId));
        }

        [Test]
        public async Task PhotoDetail_CloseButtonCallsOnCloseCallback()
        {
            // Arrange
            var photo = CreateTestPhoto();
            var callbackInvoked = false;

            var cut = ctx.RenderComponent<PhotoDetail>(parameters => parameters
                .Add(p => p.Photo, photo)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () =>
                {
                    callbackInvoked = true;
                }))
            );

            // Act
            var closeButton = cut.Find(".btn-close");
            closeButton?.Click();
            await cut.InvokeAsync(() => { });

            // Assert
            Assert.True(callbackInvoked, "OnClose callback should be invoked");
        }

        [Test]
        public void PhotoDetail_CloseButtonIsPresent()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var closeButton = cut.Find(".btn-close");
            Assert.NotNull(closeButton);
            Assert.That(closeButton?.TextContent, Is.EqualTo("✕"));
        }

        [Test]
        public void PhotoDetail_CloseButtonHasAriaLabel()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var closeButton = cut.Find(".btn-close");
            Assert.That(closeButton?.GetAttribute("aria-label"), Is.EqualTo("Close photo detail"));
        }

        [Test]
        public void PhotoDetail_HeaderContainsTitle()
        {
            // Arrange
            var testTitle = "Beautiful Landscape";
            var photo = CreateTestPhoto(title: testTitle);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var header = cut.Find(".detail-header");
            Assert.That(header?.TextContent, Does.Contain(testTitle));
        }

        [Test]
        public void PhotoDetail_TagsAreSplitCorrectly()
        {
            // Arrange
            var tags = "landscape,sunset,mountains,nature, forest";
            var photo = CreateTestPhoto(tags: tags);

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var tagElements = cut.FindAll(".tag");
            // Should handle extra spaces
            Assert.That(tagElements.Count, Is.GreaterThan(0));
        }

        [Test]
        public void PhotoDetail_InfoSectionsRender()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var infoSections = cut.FindAll(".info-section");
            // Should have at least Description and Uploaded sections
            Assert.That(infoSections.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void PhotoDetail_DetailPhotoWrapperRenders()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var photoWrapper = cut.Find(".detail-photo-wrapper");
            Assert.NotNull(photoWrapper);
            var image = photoWrapper?.QuerySelector(".detail-photo");
            Assert.NotNull(image);
        }

        [Test]
        public void PhotoDetail_NoContentWhenPhotoIsNull()
        {
            // Arrange
            StandalonePhoto photo = null;

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var detailContent = cut.FindAll(".detail-content");
            Assert.That(detailContent.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task PhotoDetail_UpdatePhotoParameter()
        {
            // Arrange
            var photo1 = CreateTestPhoto(photoId: 1, title: "Photo 1");
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo1)
            );

            // Act - Update to different photo
            var photo2 = CreateTestPhoto(photoId: 2, title: "Photo 2");
            await cut.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                { "Photo", photo2 }
            }));

            // Assert
            var title = cut.Find(".detail-title");
            Assert.That(title?.TextContent, Is.EqualTo("Photo 2"));
        }

        [Test]
        public void PhotoDetail_ComponentContainer()
        {
            // Arrange
            var photo = CreateTestPhoto();

            // Act
            var cut = ctx.RenderComponent<PhotoDetail>(
                ComponentParameter.CreateParameter("Photo", photo)
            );

            // Assert
            var container = cut.Find(".photodetail");
            Assert.NotNull(container);
        }

        // Helper method to create test photos
        private StandalonePhoto CreateTestPhoto(
            int photoId = 1,
            string title = "Test Photo",
            string description = "Test Description",
            string url = "https://example.com/photo.jpg",
            DateTime? uploadDate = null,
            string tags = null)
        {
            return new StandalonePhoto
            {
                PhotoId = photoId,
                Title = title,
                Description = description,
                Url = url,
                ThumbnailUrl = "https://example.com/thumb.jpg",
                UploadDate = uploadDate ?? DateTime.Now,
                Tags = tags,
                IsPublished = true,
                DisplayOrder = 1,
                CreatedDate = DateTime.Now
            };
        }
    }
}
