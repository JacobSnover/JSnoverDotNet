using jsnover.net.blazor.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace jsnover.net.blazor.Infrastructure.Utilities
{
    public class CardComparer : IComparer<Card>
    {
        public int Compare(Card x, Card y)
        {
            var regex = new Regex(@"^(\d+)");

            // run the regex on both strings
            var xRegexResult = regex.Match(x.value);
            var yRegexResult = regex.Match(y.value);

            // check if they are both numbers
            if (xRegexResult.Success && yRegexResult.Success)
            {
                return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
            }

            // otherwise return as string comparison
            return x.value.CompareTo(y.value);
        }

        public void MoveAcesToEnd(List<Card> cards)
        {
            var aces = new List<Card>();

            foreach (var card in cards)
            {
                if (card.value.ToLower() == "ace")
                {
                    aces.Add(card);
                }
            }

            foreach (var ace in aces)
            {
                cards.Remove(ace);
                cards.Add(ace);
            }
        }
    }
}
