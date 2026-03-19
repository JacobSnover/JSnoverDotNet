using System;
using System.Collections.Generic;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service for generating and verifying CAPTCHA challenges to prevent bot interactions.
    /// </summary>
    public class BotProtectionService
    {
        private static readonly Random _random = new Random();
        private static readonly List<(int, int, char)> _problems = new List<(int, int, char)>();

        /// <summary>
        /// Generates a random math CAPTCHA problem for user verification.
        /// </summary>
        /// <returns>Tuple containing the problem string and correct answer</returns>
        public (string problem, int correctAnswer) GenerateCaptchaChallenge()
        {
            try
            {
                int a = _random.Next(1, 100);
                int b = _random.Next(1, 100);
                char op = GetRandomOperator();

                int answer = 0;
                string problem = $"{a} {op} {b} = ?";

                switch (op)
                {
                    case '+':
                        answer = a + b;
                        break;
                    case '-':
                        answer = a - b;
                        break;
                    case '*':
                        answer = a * b;
                        break;
                }

                return (problem, answer);
            }
            catch (Exception)
            {
                return ("2 + 2 = ?", 4);
            }
        }

        /// <summary>
        /// Verifies if the user's answer matches the correct CAPTCHA answer.
        /// </summary>
        /// <param name="userAnswer">The answer provided by the user</param>
        /// <param name="correctAnswer">The correct answer to verify against</param>
        /// <returns>True if the answer is correct; false otherwise</returns>
        public bool VerifyCaptcha(int userAnswer, int correctAnswer)
        {
            try
            {
                return userAnswer == correctAnswer;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a random mathematical operator for CAPTCHA generation.
        /// </summary>
        /// <returns>A random operator: +, -, or *</returns>
        private char GetRandomOperator()
        {
            var operators = new[] { '+', '-', '*' };
            return operators[_random.Next(operators.Length)];
        }
    }
}
