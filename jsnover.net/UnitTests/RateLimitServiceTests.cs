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
    /// Unit tests for RateLimitService with mocked DbContext.
    /// Tests rate limiting enforcement, multiple IPs, authenticated bypass, and time-window reset.
    /// </summary>
    [TestFixture]
    public class RateLimitServiceTests
    {
        private Mock<jsnoverdotnetdbContext> _mockDbContext;
        private RateLimitService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<jsnoverdotnetdbContext>();
            _service = new RateLimitService(_mockDbContext.Object);
        }

        #region CheckRateLimit Tests

        [Test]
        public async Task IsRateLimited_UnderLimit_ReturnsFalse()
        {
            // Arrange
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 9, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(9));

            // Act
            var result = await _service.IsRateLimited("192.168.1.1", "images", false);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsRateLimited_AtLimit_ReturnsTrue()
        {
            // Arrange
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));

            // Act
            var result = await _service.IsRateLimited("192.168.1.1", "images", false);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsRateLimited_OverLimit_ReturnsTrue()
        {
            // Arrange
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 15, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(15));

            // Act
            var result = await _service.IsRateLimited("192.168.1.1", "images", false);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsRateLimited_ReactionEndpoint_HasDifferentLimit()
        {
            // Arrange
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "reaction", RequestCount = 5, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(5));

            // Act
            var result = await _service.IsRateLimited("192.168.1.1", "reaction", false);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsRateLimited_MultipleIPsTrackedSeparately()
        {
            // Arrange - IP A has 10 requests, IP B has 3 requests
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-30) },
                new RateLimitLog { LogId = 2, IpAddress = "192.168.1.2", Endpoint = "images", RequestCount = 3, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);

            // Act & Assert for IP A (at limit)
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));
            var resultA = await _service.IsRateLimited("192.168.1.1", "images", false);
            Assert.That(resultA, Is.True);

            // Act & Assert for IP B (under limit)
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(3));
            var resultB = await _service.IsRateLimited("192.168.1.2", "images", false);
            Assert.That(resultB, Is.False);
        }

        [Test]
        public async Task IsRateLimited_AuthenticatedUserBypassesLimit()
        {
            // Arrange
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 20, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(20));

            // Act
            var result = await _service.IsRateLimited("192.168.1.1", "images", true);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsRateLimited_EmptyIPAddress_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsRateLimited("", "images", false);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsRateLimited_NullIPAddress_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsRateLimited(null, "images", false);

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region TimeWindow Reset Tests

        [Test]
        public async Task IsRateLimited_OldRequestsNotCounted()
        {
            // Arrange - Log is 15 minutes old (outside 1-minute window)
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 15, Timestamp = DateTime.UtcNow.AddMinutes(-15) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(0));  // Old requests don't count

            // Act
            var result = await _service.IsRateLimited("192.168.1.1", "images", false);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region LogRequest Tests

        [Test]
        public async Task LogRequest_ValidInputAndNewLog_CreatesNewEntry()
        {
            // Arrange
            var logs = new List<RateLimitLog>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, bool>>>(), default))
                .Returns(Task.FromResult<RateLimitLog>(null));
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            // Act
            await _service.LogRequest("192.168.1.1", "images");

            // Assert
            _mockDbContext.Verify(x => x.RateLimitLog.Add(It.IsAny<RateLimitLog>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Test]
        public async Task LogRequest_ExistingLog_IncrementsRequestCount()
        {
            // Arrange
            var existingLog = new RateLimitLog { LogId = 1, IpAddress = "192.168.1.1", Endpoint = "images", RequestCount = 5, Timestamp = DateTime.UtcNow };
            var logs = new List<RateLimitLog> { existingLog }.AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, bool>>>(), default))
                .Returns(Task.FromResult(existingLog));
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            // Act
            await _service.LogRequest("192.168.1.1", "images");

            // Assert
            Assert.That(existingLog.RequestCount, Is.EqualTo(6));
            _mockDbContext.Verify(x => x.RateLimitLog.Update(It.IsAny<RateLimitLog>()), Times.Once);
        }

        [Test]
        public async Task LogRequest_EmptyIPAddress_DoesNothing()
        {
            // Arrange & Act
            await _service.LogRequest("", "images");

            // Assert
            _mockDbContext.Verify(x => x.RateLimitLog.Add(It.IsAny<RateLimitLog>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
        }

        [Test]
        public async Task LogRequest_NullEndpoint_DoesNothing()
        {
            // Arrange & Act
            await _service.LogRequest("192.168.1.1", null);

            // Assert
            _mockDbContext.Verify(x => x.RateLimitLog.Add(It.IsAny<RateLimitLog>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
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
}
