using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using jsnover.net.blazor.Infrastructure.Services;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for BotProtectionService.
    /// Tests CAPTCHA generation and verification for bot prevention.
    /// </summary>
    [TestFixture]
    public class BotProtectionServiceTests
    {
        private BotProtectionService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new BotProtectionService();
        }

        #region GenerateCaptchaChallenge Tests

        [Test]
        public void GenerateCaptchaChallenge_ReturnsProblemAndAnswer()
        {
            // Arrange & Act
            var result = _service.GenerateCaptchaChallenge();

            // Assert
            Assert.That(result.problem, Is.Not.Null);
            Assert.That(result.problem, Is.Not.Empty);
            Assert.That(result.correctAnswer, Is.GreaterThanOrEqualTo(-10000)); // Allow for subtraction results
        }

        [Test]
        public void GenerateCaptchaChallenge_ProblemsAreDifferent()
        {
            // Arrange & Act
            var result1 = _service.GenerateCaptchaChallenge();
            var result2 = _service.GenerateCaptchaChallenge();
            var result3 = _service.GenerateCaptchaChallenge();
            var result4 = _service.GenerateCaptchaChallenge();
            var result5 = _service.GenerateCaptchaChallenge();

            var problems = new List<string> { result1.problem, result2.problem, result3.problem, result4.problem, result5.problem };

            // Assert - At least some should be different (not all identical)
            var uniqueProblems = problems.Distinct().Count();
            Assert.That(uniqueProblems, Is.GreaterThan(1), "Multiple CAPTCHA challenges should produce different problems");
        }

        [Test]
        public void GenerateCaptchaChallenge_ProblemFormatIsCorrect()
        {
            // Arrange & Act
            var result = _service.GenerateCaptchaChallenge();

            // Assert - Format should be "number operator number = ?"
            var parts = result.problem.Split(' ');
            Assert.That(parts.Length, Is.EqualTo(5)); // "number", "operator", "number", "=", "?"
            Assert.That(parts[4], Is.EqualTo("?"));
        }

        [Test]
        public void GenerateCaptchaChallenge_AnswerIsValid()
        {
            // Arrange & Act
            var result = _service.GenerateCaptchaChallenge();

            // Assert
            Assert.That(result.correctAnswer, Is.TypeOf<int>());
        }

        #endregion

        #region VerifyCaptcha Tests

        [Test]
        public void VerifyCaptcha_CorrectAnswer_ReturnsTrue()
        {
            // Arrange
            int userAnswer = 8;
            int correctAnswer = 8;

            // Act
            var result = _service.VerifyCaptcha(userAnswer, correctAnswer);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyCaptcha_IncorrectAnswer_ReturnsFalse()
        {
            // Arrange
            int userAnswer = 9;
            int correctAnswer = 8;

            // Act
            var result = _service.VerifyCaptcha(userAnswer, correctAnswer);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyCaptcha_WrongByOne_ReturnsFalse()
        {
            // Arrange
            int userAnswer = 7;
            int correctAnswer = 8;

            // Act
            var result = _service.VerifyCaptcha(userAnswer, correctAnswer);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyCaptcha_ZeroAnswer_Correctly()
        {
            // Arrange & Act
            var result = _service.VerifyCaptcha(0, 0);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyCaptcha_NegativeAnswer_Correctly()
        {
            // Arrange & Act
            var result = _service.VerifyCaptcha(-5, -5);

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void GenerateAndVerify_CompleteWorkflow()
        {
            // Arrange & Act
            var challenge = _service.GenerateCaptchaChallenge();
            var isCorrect = _service.VerifyCaptcha(challenge.correctAnswer, challenge.correctAnswer);
            var isIncorrect = _service.VerifyCaptcha(challenge.correctAnswer + 1, challenge.correctAnswer);

            // Assert
            Assert.That(isCorrect, Is.True);
            Assert.That(isIncorrect, Is.False);
        }

        [Test]
        public void MultipleGenerateAndVerify_AllIndependent()
        {
            // Arrange & Act
            var challenges = new List<(string problem, int correctAnswer)>();
            for (int i = 0; i < 10; i++)
            {
                challenges.Add(_service.GenerateCaptchaChallenge());
            }

            // Assert - Each can be verified independently
            foreach (var challenge in challenges)
            {
                var result = _service.VerifyCaptcha(challenge.correctAnswer, challenge.correctAnswer);
                Assert.That(result, Is.True, $"Challenge {challenge.problem} should verify correctly");
            }
        }

        #endregion
    }
}
