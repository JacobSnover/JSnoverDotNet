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
    /// Security tests for rate limiting enforcement in RateLimitService.
    /// Validates request blocking, threshold behavior, header inclusion, and edge cases.
    /// </summary>
    [TestFixture]
    public class RateLimitSecurityTests
    {
        private Mock<jsnoverdotnetdbContext> _mockDbContext;
        private RateLimitService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<jsnoverdotnetdbContext>();
            _service = new RateLimitService(_mockDbContext.Object);
        }

        #region Request Blocking Tests

        [Test]
        public async Task IsRateLimited_Threshold_BlocksRequestsAfterLimit()
        {
            // Arrange - 11 requests is over the 10/min limit for images
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.1", Endpoint = "/api/photos", RequestCount = 11, Timestamp = DateTime.UtcNow.AddSeconds(-15) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(11));

            // Act
            var result = await _service.IsRateLimited("203.0.113.1", "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Should block 11th request on image endpoint");
        }

        [Test]
        public async Task IsRateLimited_AllowsRequestsBelowThreshold()
        {
            // Arrange - 9 requests is under the 10/min limit
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.2", Endpoint = "/api/photos", RequestCount = 9, Timestamp = DateTime.UtcNow.AddSeconds(-20) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(9));

            // Act
            var result = await _service.IsRateLimited("203.0.113.2", "/api/photos", false);

            // Assert
            Assert.That(result, Is.False, "Should allow 9th request on image endpoint");
        }

        [Test]
        public async Task IsRateLimited_BlocksAtExactThreshold()
        {
            // Arrange - Exactly 10 requests (at limit)
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.3", Endpoint = "/api/photos", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));

            // Act
            var result = await _service.IsRateLimited("203.0.113.3", "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Should block at exact threshold (10 requests)");
        }

        #endregion

        #region Per-IP Tracking Tests

        [Test]
        public async Task IsRateLimited_AppliesPerIPIndependently()
        {
            // Arrange - Two IPs with different request counts
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.4", Endpoint = "/api/photos", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-30) },
                new RateLimitLog { LogId = 2, IpAddress = "203.0.113.5", Endpoint = "/api/photos", RequestCount = 3, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);

            // Act & Assert for IP 1 (at limit, should be blocked)
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));
            var resultIP1 = await _service.IsRateLimited("203.0.113.4", "/api/photos", false);
            Assert.That(resultIP1, Is.True, "IP 1 at limit should be blocked");

            // Act & Assert for IP 2 (under limit, should be allowed)
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(3));
            var resultIP2 = await _service.IsRateLimited("203.0.113.5", "/api/photos", false);
            Assert.That(resultIP2, Is.False, "IP 2 under limit should be allowed");
        }

        #endregion

        #region Different Endpoint Limits Tests

        [Test]
        public async Task IsRateLimited_ImageEndpointLimit_10PerMinute()
        {
            // Arrange - /api/photos endpoint should have 10/min limit
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.6", Endpoint = "/api/photos", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));

            // Act
            var result = await _service.IsRateLimited("203.0.113.6", "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Image endpoint limit is 10/min");
        }

        [Test]
        public async Task IsRateLimited_ReactionEndpointLimit_5PerMinute()
        {
            // Arrange - /api/reactions endpoint should have 5/min limit
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.7", Endpoint = "/api/reactions", RequestCount = 5, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(5));

            // Act
            var result = await _service.IsRateLimited("203.0.113.7", "/api/reactions", false);

            // Assert
            Assert.That(result, Is.True, "Reaction endpoint limit is 5/min");
        }

        [Test]
        public async Task IsRateLimited_ReactionEndpoint_AllowsUnder5Requests()
        {
            // Arrange - 4 requests under reaction limit
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.8", Endpoint = "/api/reactions", RequestCount = 4, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(4));

            // Act
            var result = await _service.IsRateLimited("203.0.113.8", "/api/reactions", false);

            // Assert
            Assert.That(result, Is.False, "Should allow 4 requests on reaction endpoint");
        }

        #endregion

        #region Authenticated User Bypass Tests

        [Test]
        public async Task IsRateLimited_AuthenticatedUserBypassesLimit()
        {
            // Arrange - Authenticated user with 20 requests (way over limit)
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.9", Endpoint = "/api/photos", RequestCount = 20, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(20));

            // Act
            var result = await _service.IsRateLimited("203.0.113.9", "/api/photos", isAuthenticated: true);

            // Assert
            Assert.That(result, Is.False, "Authenticated users should bypass rate limiting");
        }

        [Test]
        public async Task IsRateLimited_UnauthenticatedUserRespectLimit()
        {
            // Arrange - Unauthenticated user with 10 requests (at limit)
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.10", Endpoint = "/api/photos", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-30) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));

            // Act
            var result = await _service.IsRateLimited("203.0.113.10", "/api/photos", isAuthenticated: false);

            // Assert
            Assert.That(result, Is.True, "Unauthenticated users should respect rate limit");
        }

        #endregion

        #region Time Window Reset Tests

        [Test]
        public async Task IsRateLimited_WindowReset_After10Minutes()
        {
            // Arrange - Request from 11 minutes ago should not count
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.11", Endpoint = "/api/photos", RequestCount = 15, Timestamp = DateTime.UtcNow.AddMinutes(-11) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);

            // The service should only count requests from the last 1 minute
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(0));

            // Act
            var result = await _service.IsRateLimited("203.0.113.11", "/api/photos", false);

            // Assert
            Assert.That(result, Is.False, "Old requests outside 1-minute window should not count");
        }

        [Test]
        public async Task IsRateLimited_WindowReset_RecentRequestsCount()
        {
            // Arrange - Request within 1 minute should count
            var logs = new List<RateLimitLog>
            {
                new RateLimitLog { LogId = 1, IpAddress = "203.0.113.12", Endpoint = "/api/photos", RequestCount = 10, Timestamp = DateTime.UtcNow.AddSeconds(-45) }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(10));

            // Act
            var result = await _service.IsRateLimited("203.0.113.12", "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Recent requests within 1-minute window should count");
        }

        #endregion

        #region Edge Cases & Error Handling

        [Test]
        public async Task IsRateLimited_NullIPAddress_ReturnsTrue()
        {
            // Arrange - Null IP should not be allowed
            // Act
            var result = await _service.IsRateLimited(null, "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Null IP address should be treated as violation");
        }

        [Test]
        public async Task IsRateLimited_EmptyIPAddress_ReturnsTrue()
        {
            // Arrange - Empty IP should not be allowed
            // Act
            var result = await _service.IsRateLimited("", "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Empty IP address should be treated as violation");
        }

        [Test]
        public async Task IsRateLimited_WhitespaceIPAddress_ReturnsTrue()
        {
            // Arrange - Whitespace-only IP should not be allowed
            // Act
            var result = await _service.IsRateLimited("   ", "/api/photos", false);

            // Assert
            Assert.That(result, Is.True, "Whitespace-only IP should be treated as violation");
        }

        [Test]
        public async Task IsRateLimited_DatabaseException_FailOpen()
        {
            // Arrange - Simulate database error
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.IsRateLimited("203.0.113.13", "/api/photos", false);

            // Assert
            Assert.That(result, Is.False, "Database errors should fail open (allow request)");
        }

        [Test]
        public async Task IsRateLimited_UnknownIPFormat_HandlesGracefully()
        {
            // Arrange
            var logs = new List<RateLimitLog>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.RateLimitLog).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.RateLimitLog.SumAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RateLimitLog, int>>>(), default))
                .Returns(Task.FromResult(0));

            // Act
            var result = await _service.IsRateLimited("INVALID-IP-FORMAT", "/api/photos", false);

            // Assert
            Assert.That(result, Is.False, "Invalid IP format should be handled gracefully");
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

        #endregion
    }
}
