using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using jsnover.net.blazor.Models;

namespace UnitTests
{
    /// <summary>
    /// Integration tests for admin photo management operations.
    /// Tests CRUD operations: Create, Read, Update, Delete for photo gallery administration.
    /// </summary>
    [TestFixture]
    public class AdminPhotoManagementTests
    {
        private DbContextOptions<jsnoverdotnetdbContext> _options;

        [SetUp]
        public void SetUp()
        {
            _options = new DbContextOptionsBuilder<jsnoverdotnetdbContext>()
                .UseInMemoryDatabase(databaseName: $"AdminPhotoDb_{Guid.NewGuid()}")
                .Options;

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.Database.EnsureCreated();
            }
        }

        #region Create Photo Tests

        [Test]
        public async Task AddPhoto_WithAllRequiredFields_Succeeds()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Admin Test Photo",
                Description = "A photo for admin testing",
                Url = "https://example.com/admin-photo.jpg",
                ThumbnailUrl = "https://example.com/admin-thumb.jpg",
                UploadDate = DateTime.Now,
                DisplayOrder = 1,
                CreatedDate = DateTime.Now,
                IsPublished = false,
                Tags = "admin,test"
            };

            // Act
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                var result = await context.SaveChangesAsync();

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(1));
                Assert.That(photo.PhotoId, Is.GreaterThan(0));
            }
        }

        [Test]
        public async Task AddPhoto_MinimalRequiredFields_Succeeds()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Minimal Photo",
                Url = "https://example.com/minimal.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = false
            };

            // Act
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                var result = await context.SaveChangesAsync();

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public async Task AddPhoto_MultiplePhotos_CountIncreases()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                var photo = new StandalonePhoto
                {
                    Title = $"Photo {i}",
                    Url = $"https://example.com/photo{i}.jpg",
                    UploadDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsPublished = true
                };

                // Act
                using (var context = new jsnoverdotnetdbContext(_options))
                {
                    context.StandalonePhoto.Add(photo);
                    await context.SaveChangesAsync();
                }
            }

            // Act - Count total photos
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var count = await context.StandalonePhoto.CountAsync();

                // Assert
                Assert.That(count, Is.EqualTo(5));
            }
        }

        #endregion

        #region Read Photo Tests

        [Test]
        public async Task GetPhoto_ByPhotoId_ReturnsCorrectPhoto()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Find Me Photo",
                Url = "https://example.com/findme.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var retrieved = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                // Assert
                Assert.That(retrieved, Is.Not.Null);
                Assert.That(retrieved.Title, Is.EqualTo("Find Me Photo"));
            }
        }

        [Test]
        public async Task GetPhoto_NonexistentId_ReturnsNull()
        {
            // Act
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var retrieved = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == 9999);

                // Assert
                Assert.That(retrieved, Is.Null);
            }
        }

        [Test]
        public async Task ListPhotos_ReturnsAllPhotos()
        {
            // Arrange
            for (int i = 1; i <= 3; i++)
            {
                var photo = new StandalonePhoto
                {
                    Title = $"List Photo {i}",
                    Url = $"https://example.com/list{i}.jpg",
                    UploadDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };

                using (var context = new jsnoverdotnetdbContext(_options))
                {
                    context.StandalonePhoto.Add(photo);
                    await context.SaveChangesAsync();
                }
            }

            // Act
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photos = await context.StandalonePhoto.ToListAsync();

                // Assert
                Assert.That(photos.Count, Is.EqualTo(3));
            }
        }

        [Test]
        public async Task ListPhotos_WithFilter_ReturnsFilteredPhotos()
        {
            // Arrange
            for (int i = 1; i <= 3; i++)
            {
                var isPublished = i % 2 == 0; // Alternating
                var photo = new StandalonePhoto
                {
                    Title = $"Filter Photo {i}",
                    Url = $"https://example.com/filter{i}.jpg",
                    UploadDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsPublished = isPublished
                };

                using (var context = new jsnoverdotnetdbContext(_options))
                {
                    context.StandalonePhoto.Add(photo);
                    await context.SaveChangesAsync();
                }
            }

            // Act
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var publishedPhotos = await context.StandalonePhoto
                    .Where(p => p.IsPublished)
                    .ToListAsync();

                // Assert
                Assert.That(publishedPhotos.Count, Is.EqualTo(1));
            }
        }

        #endregion

        #region Update Photo Tests

        [Test]
        public async Task UpdatePhoto_Title_UpdatesSuccessfully()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Original Title",
                Url = "https://example.com/update.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act - Update title
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photoToUpdate = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                photoToUpdate.Title = "Updated Title";
                context.StandalonePhoto.Update(photoToUpdate);
                await context.SaveChangesAsync();
            }

            // Act - Verify update
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var updatedPhoto = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                // Assert
                Assert.That(updatedPhoto.Title, Is.EqualTo("Updated Title"));
            }
        }

        [Test]
        public async Task UpdatePhoto_Description_UpdatesSuccessfully()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Photo",
                Description = "Old description",
                Url = "https://example.com/desc.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act - Update description
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photoToUpdate = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                photoToUpdate.Description = "New description with better content";
                context.StandalonePhoto.Update(photoToUpdate);
                await context.SaveChangesAsync();
            }

            // Act - Verify update
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var updatedPhoto = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                // Assert
                Assert.That(updatedPhoto.Description, Is.EqualTo("New description with better content"));
            }
        }

        [Test]
        public async Task UpdatePhoto_MultipleFields_AllUpdateSuccessfully()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Original",
                Description = "Old",
                Url = "https://example.com/original.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = false
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act - Update multiple fields
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photoToUpdate = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                photoToUpdate.Title = "Updated Title";
                photoToUpdate.Description = "Updated Description";
                photoToUpdate.IsPublished = true;
                context.StandalonePhoto.Update(photoToUpdate);
                await context.SaveChangesAsync();
            }

            // Act - Verify all updates
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var updatedPhoto = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                // Assert
                Assert.That(updatedPhoto.Title, Is.EqualTo("Updated Title"));
                Assert.That(updatedPhoto.Description, Is.EqualTo("Updated Description"));
                Assert.That(updatedPhoto.IsPublished, Is.True);
            }
        }

        #endregion

        #region Delete Photo Tests

        [Test]
        public async Task DeletePhoto_PhotoRemoved()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Delete Me",
                Url = "https://example.com/delete.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act - Delete photo
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photoToDelete = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                context.StandalonePhoto.Remove(photoToDelete);
                await context.SaveChangesAsync();
            }

            // Act - Verify deletion
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var deletedPhoto = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                // Assert
                Assert.That(deletedPhoto, Is.Null);
            }
        }

        [Test]
        public async Task DeletePhoto_CountDecreases()
        {
            // Arrange
            var photo1 = new StandalonePhoto
            {
                Title = "Photo 1",
                Url = "https://example.com/photo1.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            var photo2 = new StandalonePhoto
            {
                Title = "Photo 2",
                Url = "https://example.com/photo2.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo1);
                context.StandalonePhoto.Add(photo2);
                await context.SaveChangesAsync();
            }

            var photo1Id = photo1.PhotoId;

            // Act - Check initial count
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var countBefore = await context.StandalonePhoto.CountAsync();
                Assert.That(countBefore, Is.EqualTo(2));
            }

            // Act - Delete one photo
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photoToDelete = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photo1Id);

                context.StandalonePhoto.Remove(photoToDelete);
                await context.SaveChangesAsync();
            }

            // Act - Check count after deletion
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var countAfter = await context.StandalonePhoto.CountAsync();

                // Assert
                Assert.That(countAfter, Is.EqualTo(1));
            }
        }

        #endregion

        #region Validation Tests

        [Test]
        public async Task AddPhoto_NullTitle_SucceedsButTitleIsNull()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = null, // Null title
                Url = "https://example.com/notitle.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            // Act & Assert - Database will accept, but this should be validated at service level
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                var result = await context.SaveChangesAsync();

                Assert.That(result, Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public async Task AddPhoto_NullUrl_SucceedsButUrlIsNull()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "No URL Photo",
                Url = null, // Null URL
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            // Act & Assert - Database will accept, but this should be validated at service level
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                var result = await context.SaveChangesAsync();

                Assert.That(result, Is.GreaterThanOrEqualTo(1));
            }
        }

        #endregion
    }
}
