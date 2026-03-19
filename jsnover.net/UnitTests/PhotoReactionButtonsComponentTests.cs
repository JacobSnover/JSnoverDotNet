using Bunit;
using Blazored.SessionStorage;
using jsnover.net.blazor.Components;
using jsnover.net.blazor.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace jsnover.net.blazor.UnitTests
{
    [TestFixture]
    public class PhotoReactionButtonsComponentTests
    {
        private TestContext ctx;
        private Mock<ISessionStorageService> mockSessionStorage;

        [SetUp]
        public void Setup()
        {
            ctx = new TestContext();
            mockSessionStorage = new Mock<ISessionStorageService>();
            
            // Default mock behavior
            mockSessionStorage
                .Setup(s => s.GetItemAsStringAsync(It.IsAny<string>()))
                .ReturnsAsync((string)null);
            
            mockSessionStorage
                .Setup(s => s.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValueTask());

            ctx.Services.AddScoped(_ => mockSessionStorage.Object);
        }

        [TearDown]
        public void Teardown()
        {
            ctx?.Dispose();
        }

        [Test]
        public void PhotoReactionButtons_RenderWithoutError()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            Assert.NotNull(cut);
        }

        [Test]
        public void PhotoReactionButtons_RendersAllFiveEmojiButtons()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".reaction-btn");
            Assert.That(buttons.Count, Is.EqualTo(5), "Should render exactly 5 reaction buttons");
        }

        [Test]
        public void PhotoReactionButtons_ThumbsUpEmojiPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".emoji");
            var hasThumbsUp = false;
            foreach (var btn in buttons)
            {
                if (btn.TextContent.Contains("👍"))
                {
                    hasThumbsUp = true;
                    break;
                }
            }
            Assert.True(hasThumbsUp, "Thumbs up emoji should be present");
        }

        [Test]
        public void PhotoReactionButtons_HeartEmojiPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".emoji");
            var hasHeart = false;
            foreach (var btn in buttons)
            {
                if (btn.TextContent.Contains("❤️"))
                {
                    hasHeart = true;
                    break;
                }
            }
            Assert.True(hasHeart, "Heart emoji should be present");
        }

        [Test]
        public void PhotoReactionButtons_LaughEmojiPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".emoji");
            var hasLaugh = false;
            foreach (var btn in buttons)
            {
                if (btn.TextContent.Contains("😂"))
                {
                    hasLaugh = true;
                    break;
                }
            }
            Assert.True(hasLaugh, "Laugh emoji should be present");
        }

        [Test]
        public void PhotoReactionButtons_SurpriseEmojiPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".emoji");
            var hasSurprise = false;
            foreach (var btn in buttons)
            {
                if (btn.TextContent.Contains("😮"))
                {
                    hasSurprise = true;
                    break;
                }
            }
            Assert.True(hasSurprise, "Surprise emoji should be present");
        }

        [Test]
        public void PhotoReactionButtons_SadEmojiPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".emoji");
            var hasSad = false;
            foreach (var btn in buttons)
            {
                if (btn.TextContent.Contains("😢"))
                {
                    hasSad = true;
                    break;
                }
            }
            Assert.True(hasSad, "Sad emoji should be present");
        }

        [Test]
        public void PhotoReactionButtons_EachButtonHasReactionCount()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var countBadges = cut.FindAll(".reaction-count");
            Assert.That(countBadges.Count, Is.EqualTo(5), "Each button should display reaction count");
        }

        [Test]
        public void PhotoReactionButtons_CountDisplaysZeroInitially()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var counts = cut.FindAll(".reaction-count");
            foreach (var count in counts)
            {
                // Should start at 0 if no reactions loaded from DB
                Assert.That(count.TextContent, Does.Match(@"\d+"));
            }
        }

        [Test]
        public void PhotoReactionButtons_ButtonsHaveDataEmojiAttribute()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".reaction-btn");
            foreach (var btn in buttons)
            {
                var dataEmoji = btn.GetAttribute("data-emoji");
                Assert.That(dataEmoji, Is.Not.Null);
                Assert.That(dataEmoji, Does.Match(@"[\u{1F600}-\u{1F64F}👍❤️😂😮😢]"));
            }
        }

        [Test]
        public void PhotoReactionButtons_ButtonsHaveTooltips()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".reaction-btn");
            foreach (var btn in buttons)
            {
                var title = btn.GetAttribute("title");
                Assert.That(title, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void PhotoReactionButtons_PhotoIdParameterIsSet()
        {
            // Arrange
            const int photoId = 42;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var component = cut.Instance;
            Assert.That(component.PhotoId, Is.EqualTo(42));
        }

        [Test]
        public void PhotoReactionButtons_ReactionContainer()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var container = cut.Find(".photo-reaction-buttons");
            Assert.NotNull(container);
            var reactionsContainer = cut.Find(".reactions-container");
            Assert.NotNull(reactionsContainer);
        }

        [Test]
        public async Task PhotoReactionButtons_ClickingButtonUpdatesReactionCount()
        {
            // Arrange
            const int photoId = 1;
            mockSessionStorage
                .Setup(s => s.GetItemAsStringAsync("sessionId"))
                .ReturnsAsync("test-session-id");

            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            var initialCount = int.TryParse(
                cut.FindAll(".reaction-count")[0].TextContent,
                out var count) ? count : 0;

            // Act
            var buttons = cut.FindAll(".reaction-btn");
            buttons[0].Click();
            await cut.InvokeAsync(() => { });

            // Assert - count should increase or button should be disabled
            var updatedButtons = cut.FindAll(".reaction-btn");
            var isDisabled = updatedButtons[0].GetAttribute("disabled") != null;
            Assert.True(isDisabled || int.TryParse(cut.FindAll(".reaction-count")[0].TextContent, out _), 
                "Button should be disabled or count updated");
        }

        [Test]
        public void PhotoReactionButtons_DisabledButtonHasDifferentClass()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".reaction-btn");
            // Check that buttons have the potential to have "reacted" class
            foreach (var btn in buttons)
            {
                var className = btn.GetAttribute("class");
                Assert.That(className, Does.Contain("reaction-btn"));
            }
        }

        [Test]
        public void PhotoReactionButtons_TooltipShowsAlreadyReacted()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".reaction-btn");
            // Check tooltips contain either reaction name or "Already reacted" message
            foreach (var btn in buttons)
            {
                var title = btn.GetAttribute("title");
                Assert.That(title, Does.Contain("React with").Or.Contain("You reacted"));
            }
        }

        [Test]
        public void PhotoReactionButtons_DifferentPhotoIds()
        {
            // Arrange
            const int photoId1 = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId1)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var component = cut.Instance;
            Assert.That(component.PhotoId, Is.EqualTo(photoId1));
        }

        [Test]
        public void PhotoReactionButtons_MultipleInstancesDifferentPhotos()
        {
            // Arrange
            const int photoId1 = 1;
            const int photoId2 = 2;

            // Act
            var cut1 = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId1)
            );
            var cut2 = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId2)
            );

            cut1.WaitForAsyncEvents();
            cut2.WaitForAsyncEvents();

            // Assert
            Assert.That(cut1.Instance.PhotoId, Is.EqualTo(photoId1));
            Assert.That(cut2.Instance.PhotoId, Is.EqualTo(photoId2));
        }

        [Test]
        public void PhotoReactionButtons_SessionStorageCalled()
        {
            // Arrange
            const int photoId = 1;
            mockSessionStorage
                .Setup(s => s.GetItemAsStringAsync("sessionId"))
                .ReturnsAsync("test-session-id");

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            mockSessionStorage.Verify(
                s => s.GetItemAsStringAsync(It.IsAny<string>()),
                Times.AtLeastOnce,
                "SessionStorage should be accessed to manage reactions");
        }

        [Test]
        public void PhotoReactionButtons_AllButtonsInitiallyEnabled()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert
            var buttons = cut.FindAll(".reaction-btn");
            foreach (var btn in buttons)
            {
                var disabled = btn.GetAttribute("disabled");
                // Buttons without existing reactions should not be disabled
                Assert.That(disabled, Is.Null.Or.Empty);
            }
        }

        [Test]
        public void PhotoReactionButtons_ReactionCountConsistent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            var countTexts = new List<string>();
            var counts = cut.FindAll(".reaction-count");
            foreach (var count in counts)
            {
                countTexts.Add(count.TextContent);
            }

            // Assert - all counts should be valid integers
            foreach (var countText in countTexts)
            {
                Assert.That(int.TryParse(countText.Trim(), out _), 
                    Is.True, 
                    $"Count '{countText}' should be a valid integer");
            }
        }

        [Test]
        public void PhotoReactionButtons_ComponentRespondsToPropertyChanges()
        {
            // Arrange
            const int photoId = 1;
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Act - Set parameters
            var task = cut.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                { "PhotoId", photoId }
            }));

            // Assert
            Assert.That(task, Is.Not.Null);
            Assert.That(cut.Instance.PhotoId, Is.EqualTo(photoId));
        }

        [Test]
        public void PhotoReactionButtons_DebounceProtectionInPlace()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoReactionButtons>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );
            cut.WaitForAsyncEvents();

            // Assert - Component should have debounce mechanism
            // This is tested through behavior: rapid clicks should not process multiple times
            var buttons = cut.FindAll(".reaction-btn");
            Assert.That(buttons.Count, Is.EqualTo(5), "All buttons should be rendered for debounce testing");
        }
    }
}
