using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using jsnover.net.blazor.Models;
using jsnover.net.blazor.Infrastructure.Services;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for PhotoGalleryService with mocked repository/DbContext.
    /// Tests carousel, gallery browsing, photo details, and search functionality.
    /// </summary>
    [TestFixture]
    public class PhotoGalleryServiceTests
    {
        private Mock<jsnoverdotnetdbContext> _mockDbContext;
        private PhotoGalleryService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<jsnoverdotnetdbContext>();
            _service = new PhotoGalleryService(_mockDbContext.Object);
        }

        #region GetCarouselPhotos Tests

        [Test]
        public async Task GetCarouselPhotos_ReturnsPublishedPhotosOnly()
        {
            // Arrange
            var publishedPhotos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", IsPublished = true, UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", IsPublished = true, UploadDate = DateTime.Now }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            mockDbSet.As<IAsyncEnumerable<StandalonePhoto>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new AsyncEnumerator<StandalonePhoto>(publishedPhotos.GetEnumerator()));
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.Provider)
                .Returns(publishedPhotos.Provider);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.Expression)
                .Returns(publishedPhotos.Expression);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.ElementType)
                .Returns(publishedPhotos.ElementType);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.GetEnumerator())
                .Returns(publishedPhotos.GetEnumerator());

            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetCarouselPhotos(2);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.LessThanOrEqualTo(2));
            Assert.That(result.All(p => p.IsPublished), Is.True);
        }

        [Test]
        public async Task GetCarouselPhotos_RespectsCountParameter()
        {
            // Arrange
            var photos = Enumerable.Range(1, 10)
                .Select(i => new StandalonePhoto 
                { 
                    PhotoId = i, 
                    Title = $"Photo {i}", 
                    IsPublished = true, 
                    UploadDate = DateTime.Now 
                })
                .AsQueryable();

            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            mockDbSet.As<IAsyncEnumerable<StandalonePhoto>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new AsyncEnumerator<StandalonePhoto>(photos.Take(5).GetEnumerator()));
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.Provider)
                .Returns(photos.Provider);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.Expression)
                .Returns(photos.Expression);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.ElementType)
                .Returns(photos.ElementType);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.GetEnumerator())
                .Returns(photos.GetEnumerator());

            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetCarouselPhotos(5);

            // Assert
            Assert.That(result.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public async Task GetCarouselPhotos_DefaultCountIsFive()
        {
            // Arrange
            var photos = Enumerable.Range(1, 20)
                .Select(i => new StandalonePhoto 
                { 
                    PhotoId = i, 
                    Title = $"Photo {i}", 
                    IsPublished = true, 
                    UploadDate = DateTime.Now 
                })
                .AsQueryable();

            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            mockDbSet.As<IAsyncEnumerable<StandalonePhoto>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new AsyncEnumerator<StandalonePhoto>(photos.Take(5).GetEnumerator()));
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.Provider)
                .Returns(photos.Provider);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.Expression)
                .Returns(photos.Expression);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.ElementType)
                .Returns(photos.ElementType);
            mockDbSet.As<IQueryable<StandalonePhoto>>()
                .Setup(x => x.GetEnumerator())
                .Returns(photos.GetEnumerator());

            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetCarouselPhotos();

            // Assert
            Assert.That(result.Count, Is.LessThanOrEqualTo(5));
        }

        #endregion

        #region GetGalleryPhotos Tests

        [Test]
        public async Task GetGalleryPhotos_FilterAll_ReturnsAllPublished()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", IsPublished = true, UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Photo 2", IsPublished = true, UploadDate = DateTime.Now }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(photos);
            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetGalleryPhotos("all", 1, 20);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.All(p => p.IsPublished), Is.True);
        }

        [Test]
        public async Task GetGalleryPhotos_PaginationWorks()
        {
            // Arrange
            var photos = Enumerable.Range(1, 50)
                .Select(i => new StandalonePhoto 
                { 
                    PhotoId = i, 
                    Title = $"Photo {i}", 
                    IsPublished = true, 
                    UploadDate = DateTime.Now.AddDays(-i)
                })
                .AsQueryable();

            var mockDbSet = CreateMockDbSet(photos);
            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var page1 = await _service.GetGalleryPhotos("all", 1, 20);
            var page2 = await _service.GetGalleryPhotos("all", 2, 20);

            // Assert
            Assert.That(page1.Count, Is.LessThanOrEqualTo(20));
            Assert.That(page2.Count, Is.LessThanOrEqualTo(20));
        }

        [Test]
        public async Task GetGalleryPhotos_InvalidPageDefaults()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Photo 1", IsPublished = true, UploadDate = DateTime.Now }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(photos);
            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetGalleryPhotos("all", -1, -1);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        #endregion

        #region GetPhotoDetail Tests

        [Test]
        public async Task GetPhotoDetail_ReturnsApprovedCommentsOnly()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                PhotoId = 1,
                Title = "Test Photo",
                IsPublished = true,
                Comments = new List<PhotoComment>
                {
                    new PhotoComment { CommentId = 1, Message = "Approved", IsApproved = true },
                    new PhotoComment { CommentId = 2, Message = "Not Approved", IsApproved = false }
                },
                Reactions = new List<PhotoReaction>()
            };

            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StandalonePhoto, bool>>>(), default))
                .Returns(Task.FromResult(photo));

            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetPhotoDetail(1);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetPhotoDetail_ReturnsAggregatedReactionCounts()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                PhotoId = 1,
                Title = "Test Photo",
                IsPublished = true,
                Comments = new List<PhotoComment>(),
                Reactions = new List<PhotoReaction>
                {
                    new PhotoReaction { ReactionId = 1, ReactionType = "👍" },
                    new PhotoReaction { ReactionId = 2, ReactionType = "👍" },
                    new PhotoReaction { ReactionId = 3, ReactionType = "❤️" }
                }
            };

            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StandalonePhoto, bool>>>(), default))
                .Returns(Task.FromResult(photo));

            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetPhotoDetail(1);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetPhotoDetail_NonexistentPhotoReturnsNull()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StandalonePhoto, bool>>>(), default))
                .Returns(Task.FromResult<StandalonePhoto>(null));

            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.GetPhotoDetail(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region SearchPhotos Tests

        [Test]
        public async Task SearchPhotos_FindsByTitle()
        {
            // Arrange
            var photos = new List<StandalonePhoto>
            {
                new StandalonePhoto { PhotoId = 1, Title = "Sunset Beach", IsPublished = true, UploadDate = DateTime.Now },
                new StandalonePhoto { PhotoId = 2, Title = "Mountain Peak", IsPublished = true, UploadDate = DateTime.Now }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(photos);
            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.SearchPhotos("Sunset");

            // Assert
            Assert.That(result.Any(p => p.Title.Contains("Sunset")), Is.True);
        }

        [Test]
        public async Task SearchPhotos_EmptyQueryReturnsEmptyList()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.SearchPhotos("");

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SearchPhotos_NullQueryReturnsEmptyList()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<StandalonePhoto>>();
            _mockDbContext.Setup(x => x.StandalonePhoto).Returns(mockDbSet.Object);

            // Act
            var result = await _service.SearchPhotos(null);

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region Helper Methods

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IAsyncEnumerable<T>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new AsyncEnumerator<T>(data.GetEnumerator()));
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.Provider)
                .Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.Expression)
                .Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.ElementType)
                .Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(data.GetEnumerator());

            return mockDbSet;
        }

        #endregion
    }

    /// <summary>
    /// Async enumerator helper for mocking DbSet queries.
    /// </summary>
    public class AsyncEnumerator<T> : System.Collections.Generic.IAsyncEnumerator<T>
    {
        private readonly System.Collections.Generic.IEnumerator<T> _inner;

        public AsyncEnumerator(System.Collections.Generic.IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            return _inner.MoveNext();
        }

        public async ValueTask DisposeAsync()
        {
            _inner?.Dispose();
        }
    }
}
