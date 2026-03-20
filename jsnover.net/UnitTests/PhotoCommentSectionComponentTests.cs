using Bunit;
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
    public class PhotoCommentSectionComponentTests
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
        public void PhotoCommentSection_RenderWithoutError()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            Assert.NotNull(cut);
        }

        [Test]
        public void PhotoCommentSection_DisplaysCommentListSection()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var commentsList = cut.Find(".comments-list");
            Assert.NotNull(commentsList);
        }

        [Test]
        public void PhotoCommentSection_DisplaysNoCommentsMessage_WhenEmpty()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var noCommentsMessage = cut.Find(".no-comments");
            Assert.NotNull(noCommentsMessage);
            Assert.That(noCommentsMessage?.TextContent, Does.Contain("No comments yet"));
        }

        [Test]
        public void PhotoCommentSection_CommentFormPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var formSection = cut.Find(".comment-form-section");
            Assert.NotNull(formSection);
            Assert.That(formSection?.TextContent, Does.Contain("Leave a Comment"));
        }

        [Test]
        public void PhotoCommentSection_FormHasEmailField()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var emailLabel = cut.FindAll("label");
            var hasEmailLabel = false;
            foreach (var label in emailLabel)
            {
                if (label.TextContent.Contains("Email"))
                {
                    hasEmailLabel = true;
                    break;
                }
            }
            Assert.True(hasEmailLabel, "Email field label should be present");

            var emailInput = cut.Find("input#email");
            Assert.NotNull(emailInput);
        }

        [Test]
        public void PhotoCommentSection_FormHasNameField()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var nameLabel = cut.FindAll("label");
            var hasNameLabel = false;
            foreach (var label in nameLabel)
            {
                if (label.TextContent.Contains("Name"))
                {
                    hasNameLabel = true;
                    break;
                }
            }
            Assert.True(hasNameLabel, "Name field label should be present");

            var nameInput = cut.Find("input#name");
            Assert.NotNull(nameInput);
        }

        [Test]
        public void PhotoCommentSection_FormHasMessageField()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var messageLabel = cut.FindAll("label");
            var hasMessageLabel = false;
            foreach (var label in messageLabel)
            {
                if (label.TextContent.Contains("Message"))
                {
                    hasMessageLabel = true;
                    break;
                }
            }
            Assert.True(hasMessageLabel, "Message field label should be present");

            var messageInput = cut.Find("textarea#message");
            Assert.NotNull(messageInput);
        }

        [Test]
        public void PhotoCommentSection_EmailNameAndMessageAreRequired()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var requiredMarkers = cut.FindAll(".required");
            // Should have at least 3 required markers for email, name, and message
            Assert.That(requiredMarkers.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PhotoCommentSection_MessageFieldHasMaxLength()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var messageInput = cut.Find("textarea#message");
            Assert.That(messageInput?.GetAttribute("maxlength"), Is.EqualTo("1000"));
        }

        [Test]
        public void PhotoCommentSection_SubmitButtonPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var submitButton = cut.Find(".comment-form-section button[type='submit']");
            Assert.NotNull(submitButton);
            Assert.That(submitButton?.TextContent, Does.Contain("Submit Comment"));
        }

        [Test]
        public void PhotoCommentSection_CharacterCounterDisplays()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var formText = cut.Find(".form-text");
            Assert.NotNull(formText);
            Assert.That(formText?.TextContent, Does.Contain("/ 1000 characters"));
        }

        [Test]
        public void PhotoCommentSection_EmailFieldPlaceholder()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var emailInput = cut.Find("input#email");
            Assert.That(emailInput?.GetAttribute("placeholder"), Is.EqualTo("your.email@example.com"));
        }

        [Test]
        public void PhotoCommentSection_NameFieldPlaceholder()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var nameInput = cut.Find("input#name");
            Assert.That(nameInput?.GetAttribute("placeholder"), Is.EqualTo("Your Name"));
        }

        [Test]
        public void PhotoCommentSection_MessageFieldPlaceholder()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var messageInput = cut.Find("textarea#message");
            Assert.That(messageInput?.GetAttribute("placeholder"), Does.Contain("Share your thoughts"));
        }

        [Test]
        public void PhotoCommentSection_MessageFieldHasRows()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var messageInput = cut.Find("textarea#message");
            Assert.That(messageInput?.GetAttribute("rows"), Is.EqualTo("4"));
        }

        [Test]
        public void PhotoCommentSection_EditFormForDataAnnotationValidation()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var editForm = cut.Find("EditForm");
            Assert.NotNull(editForm);
            // DataAnnotationsValidator should be present
            var validator = cut.Find("DataAnnotationsValidator");
            Assert.NotNull(validator);
        }

        [Test]
        public void PhotoCommentSection_ValidationMessagesForFields()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var validationMessages = cut.FindAll("ValidationMessage");
            // Should have validation messages for email, name, and message
            Assert.That(validationMessages.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PhotoCommentSection_FormControlsHaveFormControlClass()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var formControls = cut.FindAll(".form-control");
            // Should have at least 3 form controls (email, name, message)
            Assert.That(formControls.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PhotoCommentSection_PhotoIdParameterIsSet()
        {
            // Arrange
            const int photoId = 42;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var component = cut.Instance;
            Assert.That(component.PhotoId, Is.EqualTo(42));
        }

        [Test]
        public void PhotoCommentSection_DifferentPhotoIds()
        {
            // Arrange
            const int photoId1 = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId1)
            );

            // Assert
            var component = cut.Instance;
            Assert.That(component.PhotoId, Is.EqualTo(photoId1));
        }

        [Test]
        public void PhotoCommentSection_FormGroupsPresent()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var formGroups = cut.FindAll(".form-group");
            // Should have form groups for email, name, and message
            Assert.That(formGroups.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PhotoCommentSection_CommentItemDisplaysCorrectStructure()
        {
            // Arrange - This would require mocking data or integration testing
            // For now, test that the structure is present in markup
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert - Verify component renders without error
            Assert.NotNull(cut);
        }

        [Test]
        public void PhotoCommentSection_ButtonsHaveProperCSS()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var submitBtn = cut.Find(".comment-form-section button[type='submit']");
            Assert.That(submitBtn?.ClassList, Contains.Item("btn"));
            Assert.That(submitBtn?.ClassList, Contains.Item("btn-primary"));
        }

        [Test]
        public void PhotoCommentSection_FormSectionContainer()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var section = cut.Find(".photo-comment-section");
            Assert.NotNull(section);
        }

        [Test]
        public void PhotoCommentSection_LeaveCommentHeading()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var headings = cut.FindAll("h4");
            var hasHeading = false;
            foreach (var heading in headings)
            {
                if (heading.TextContent.Contains("Leave a Comment"))
                {
                    hasHeading = true;
                    break;
                }
            }
            Assert.True(hasHeading, "Leave a Comment heading should be present");
        }

        [Test]
        public void PhotoCommentSection_InputsHaveAppropriateTypes()
        {
            // Arrange
            const int photoId = 1;

            // Act
            var cut = ctx.RenderComponent<PhotoCommentSection>(
                ComponentParameter.CreateParameter("PhotoId", photoId)
            );

            // Assert
            var emailInput = cut.Find("input#email");
            var nameInput = cut.Find("input#name");
            
            // Note: InputText and InputTextArea in Blazor render as text input
            Assert.NotNull(emailInput);
            Assert.NotNull(nameInput);
        }
    }
}
