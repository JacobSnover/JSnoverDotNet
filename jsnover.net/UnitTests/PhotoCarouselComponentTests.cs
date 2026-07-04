using Bunit;
using jsnover.net.blazor.Components;
using jsnover.net.blazor.Models;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.blazor.UnitTests
{
    [TestFixture]
    public class PhotoCarouselComponentTests
    {
        private Bunit.TestContext ctx;

        [SetUp]
        public void Setup()
        {
            ctx = new Bunit.TestContext();
        }

        [TearDown]
        public void Teardown()
        {
            ctx?.Dispose();
        }

        [Test]
        public void PhotoCarousel_RenderWithoutError_EmptyPhotosList()
        {
            // Arrange
            var photos = new List<StandalonePhoto>();

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            Assert.NotNull(cut);
            var emptyMessage = cut.Find(".carousel-empty");
            Assert.That(emptyMessage?.TextContent, Does.Contain("No photos to display"));
        }

        [Test]
        public void PhotoCarousel_RenderWithoutError_PopulatedPhotosList()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto
                {
                    PhotoId = 1,
                    Title = "Photo 1",
                    Description = "First photo",
                    Url = "https://example.com/photo1.jpg",
                    UploadDate = DateTime.Now
                },
                new StandalonePhoto
                {
                    PhotoId = 2,
                    Title = "Photo 2",
                    Description = "Second photo",
                    Url = "https://example.com/photo2.jpg",
                    UploadDate = DateTime.Now
                }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            Assert.NotNull(cut);
            var carouselWrapper = cut.Find(".carousel-wrapper");
            Assert.NotNull(carouselWrapper);
        }

        [Test]
        public void PhotoCarousel_DisplaysFirstPhotoOnInitialLoad()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto
                {
                    PhotoId = 1,
                    Title = "First Photo",
                    Description = "First description",
                    Url = "https://example.com/photo1.jpg",
                    UploadDate = DateTime.Now
                },
                new StandalonePhoto
                {
                    PhotoId = 2,
                    Title = "Second Photo",
                    Description = "Second description",
                    Url = "https://example.com/photo2.jpg",
                    UploadDate = DateTime.Now
                }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var image = cut.Find(".carousel-image");
            Assert.That(image?.GetAttribute("src"), Is.EqualTo("https://example.com/photo1.jpg"));
            Assert.That(image?.GetAttribute("alt"), Is.EqualTo("First Photo"));
            
            var title = cut.Find(".carousel-title");
            Assert.That(title?.TextContent, Is.EqualTo("First Photo"));
        }

        [Test]
        public void PhotoCarousel_NextArrowIncrementsCurrentPhoto()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 3, Title = "Photo 3", Description = "Desc 3", Url = "url3.jpg", UploadDate = DateTime.Now }
            };
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Act
            var nextButton = cut.Find(".carousel-next");
            nextButton?.Click();


            // Assert
            var image = cut.Find(".carousel-image");
            Assert.That(image?.GetAttribute("alt"), Is.EqualTo("Photo 2"));
        }

        [Test]
        public void PhotoCarousel_PreviousArrowDecrementsCurrentPhoto()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 3, Title = "Photo 3", Description = "Desc 3", Url = "url3.jpg", UploadDate = DateTime.Now }
            };
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Act - Navigate to third photo first
            var nextButton = cut.Find(".carousel-next");
            nextButton?.Click();

            nextButton?.Click();


            // Now click previous
            var prevButton = cut.Find(".carousel-prev");
            prevButton?.Click();


            // Assert
            var image = cut.Find(".carousel-image");
            Assert.That(image?.GetAttribute("alt"), Is.EqualTo("Photo 2"));
        }

        [Test]
        public void PhotoCarousel_PlayPauseButtonTogglesFunctionality()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now }
            };
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Act - Initial state should show pause icon (playing)
            var controlButton = cut.Find(".carousel-control");
            Assert.That(controlButton?.TextContent, Does.Contain("⏸"));

            // Click to pause
            controlButton?.Click();


            // Assert pause state shows play icon
            controlButton = cut.Find(".carousel-control");
            Assert.That(controlButton?.TextContent, Does.Contain("▶"));

            // Click to resume
            controlButton?.Click();


            // Assert playing state shows pause icon
            controlButton = cut.Find(".carousel-control");
            Assert.That(controlButton?.TextContent, Does.Contain("⏸"));
        }

        [Test]
        public async Task PhotoCarousel_ClickPhotoCallsOnPhotoSelectedCallback()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now }
            };
            var callbackInvoked = false;
            var selectedIndex = -1;

            var cut = ctx.RenderComponent<PhotoCarousel>(parameters => parameters
                .Add(p => p.Photos, photos)
                .Add(p => p.OnPhotoSelected, EventCallback.Factory.Create<int>(this, (index) =>
                {
                    callbackInvoked = true;
                    selectedIndex = index;
                }))
            );

            // Act
            var indicators = cut.FindAll(".indicator");
            if (indicators.Count > 1)
            {
                indicators[1].Click();
                await cut.InvokeAsync(() => { });
            }

            // Assert
            Assert.True(callbackInvoked, "Callback should be invoked");
            Assert.That(selectedIndex, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void PhotoCarousel_DisplaysPhotoCounterCorrectly()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 3, Title = "Photo 3", Description = "Desc 3", Url = "url3.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 4, Title = "Photo 4", Description = "Desc 4", Url = "url4.jpg", UploadDate = DateTime.Now }
            };
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Act
            var counter = cut.Find(".carousel-counter");
            var initialText = counter?.TextContent;

            // Assert initial counter shows "1 / 4"
            Assert.That(initialText, Does.Contain("1 / 4"));

            // Act - navigate to second photo
            var nextButton = cut.Find(".carousel-next");
            nextButton?.Click();


            // Assert counter shows "2 / 4"
            counter = cut.Find(".carousel-counter");
            Assert.That(counter?.TextContent, Does.Contain("2 / 4"));
        }

        [Test]
        public void PhotoCarousel_IndicatorDotsRenderForEachPhoto()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 3, Title = "Photo 3", Description = "Desc 3", Url = "url3.jpg", UploadDate = DateTime.Now }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var indicators = cut.FindAll(".indicator");
            Assert.That(indicators.Count, Is.EqualTo(3));
            
            // First indicator should be active
            Assert.That(indicators[0].ClassList, Contains.Item("active"));
        }

        [Test]
        public void PhotoCarousel_IndicatorDotBecomesActiveWhenClicked()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 3, Title = "Photo 3", Description = "Desc 3", Url = "url3.jpg", UploadDate = DateTime.Now }
            };
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Act
            var indicators = cut.FindAll(".indicator");
            indicators[2].Click();


            // Assert - re-query indicators after render
            var updatedIndicators = cut.FindAll(".indicator");
            Assert.That(updatedIndicators[2].ClassList, Contains.Item("active"));
        }

        [Test]
        public void PhotoCarousel_TitleAndDescriptionDisplayCorrectly()
        {
            // Arrange
            var testTitle = "Beautiful Landscape";
            var testDescription = "A description of the beautiful landscape.";
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto
                {
                    PhotoId = 1,
                    Title = testTitle,
                    Description = testDescription,
                    Url = "https://example.com/photo1.jpg",
                    UploadDate = DateTime.Now
                }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var title = cut.Find(".carousel-title");
            var description = cut.Find(".carousel-description");
            Assert.That(title?.TextContent, Is.EqualTo(testTitle));
            Assert.That(description?.TextContent, Is.EqualTo(testDescription));
        }

        [Test]
        public void PhotoCarousel_ImageHasLazyLoadingAttribute()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var image = cut.Find(".carousel-image");
            Assert.That(image?.GetAttribute("loading"), Is.EqualTo("lazy"));
        }

        [Test]
        public void PhotoCarousel_NavigationArrowsHaveAriaLabels()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", Description = "Desc 1", Url = "url1.jpg", UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", Description = "Desc 2", Url = "url2.jpg", UploadDate = DateTime.Now }
            };

            // Act
            var cut = ctx.RenderComponent<PhotoCarousel>(
                ComponentParameter.CreateParameter("Photos", photos)
            );

            // Assert
            var prevButton = cut.Find(".carousel-prev");
            var nextButton = cut.Find(".carousel-next");
            Assert.That(prevButton?.GetAttribute("aria-label"), Is.Not.Null);
            Assert.That(nextButton?.GetAttribute("aria-label"), Is.Not.Null);
        }
    }
}
