using System;
using System.Collections.Generic;
using System.Linq;
using jsnover.net.blazor.Models;

namespace jsnover.net.blazor.GameLogic
{
    public class SolitaireGameState
    {
        public List<Card> Stock { get; set; } = new(); // Undrawn cards
        public List<Card> Waste { get; set; } = new(); // Discard pile
        public List<Card>[] Tableau { get; set; } = new List<Card>[7]; // 7 columns
        public List<Card>[] Foundations { get; set; } = new List<Card>[4]; // 4 suit piles

        private List<Card> originalDeck;

        public SolitaireGameState(List<Card> deck)
        {
            originalDeck = deck.ToList(); // keep a copy for restart
            for (int i = 0; i < 7; i++) Tableau[i] = new List<Card>();
            for (int i = 0; i < 4; i++) Foundations[i] = new List<Card>();
            InitializeBoard(deck);
        }

        private void Shuffle(List<Card> deck)
        {
            var rng = new Random();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (deck[n], deck[k]) = (deck[k], deck[n]);
            }
        }

        private void InitializeBoard(List<Card> deck)
        {
            int deckIndex = 0;
            // Defensive: ensure we have at least 28 cards for tableau
            if (deck.Count < 52) throw new System.Exception($"Deck must have 52 cards, got {deck.Count}");
            // Deal cards to tableau
            for (int col = 0; col < 7; col++)
            {
                for (int row = 0; row <= col; row++)
                {
                    if (deckIndex >= deck.Count) throw new System.Exception("Not enough cards to deal tableau");
                    var card = deck[deckIndex++];
                    // Only the top card in each column starts face up
                    card.IsFaceUp = row == col;
                    Tableau[col].Add(card);
                }
            }
            // Remaining cards go to stock
            Stock = deck.Skip(deckIndex).ToList();
        }

        // Move a card from one tableau column to another
        public bool MoveTableauToTableau(int fromCol, int cardIndex, int toCol)
        {
            if (fromCol == toCol) return false; // Can't move to same column
            
            if (cardIndex >= Tableau[fromCol].Count) return false;
            
            var movingCard = Tableau[fromCol][cardIndex];
            var targetCard = Tableau[toCol].LastOrDefault();
            
            // Check if the move is valid
            if (!CanMoveToTableau(movingCard, targetCard))
            {
                return false;
            }

            // Calculate how many cards to move (all cards from cardIndex to end)
            int cardsToMove = Tableau[fromCol].Count - cardIndex;
            
            // Move the cards
            var movedCards = Tableau[fromCol].GetRange(cardIndex, cardsToMove);
            Tableau[toCol].AddRange(movedCards);
            Tableau[fromCol].RemoveRange(cardIndex, cardsToMove);
            
            // Reveal the new top card in the source column if there are any cards left
            if (Tableau[fromCol].Count > 0)
            {
                Tableau[fromCol].Last().IsFaceUp = true;
            }
            
            return true;
        }

        // Move a card from tableau to foundation
        public bool MoveTableauToFoundation(int fromCol, int foundationIndex)
        {
            var card = Tableau[fromCol].LastOrDefault();
            if (card == null || !CanMoveToFoundation(card, foundationIndex))
                return false;
            
            Foundations[foundationIndex].Add(card);
            Tableau[fromCol].RemoveAt(Tableau[fromCol].Count - 1);
            
            // Reveal the new top card in the source column if there are any cards left
            if (Tableau[fromCol].Count > 0)
            {
                Tableau[fromCol].Last().IsFaceUp = true;
            }
            
            return true;
        }

        // Move a card from waste to tableau
        public bool MoveWasteToTableau(int toCol)
        {
            var card = Waste.LastOrDefault();
            if (card == null || !CanMoveToTableau(card, Tableau[toCol].LastOrDefault()))
                return false;
            card.IsFaceUp = true; // Ensure card stays face up when moved
            Tableau[toCol].Add(card);
            Waste.RemoveAt(Waste.Count - 1);
            return true;
        }

        // Move a card from waste to foundation
        public bool MoveWasteToFoundation(int foundationIndex)
        {
            var card = Waste.LastOrDefault();
            if (card == null || !CanMoveToFoundation(card, foundationIndex))
                return false;
            card.IsFaceUp = true; // Ensure card stays face up in foundation
            Foundations[foundationIndex].Add(card);
            Waste.RemoveAt(Waste.Count - 1);
            return true;
        }

        // Draw a card from stock to waste
        public bool DrawFromStock()
        {
            if (Stock.Count == 0) return false;
            var card = Stock[0];
            card.IsFaceUp = true; // Cards in waste pile should always be face up
            Waste.Add(card);
            Stock.RemoveAt(0);
            return true;
        }

        // Move validation helpers
        private bool CanMoveToTableau(Card moving, Card target)
        {
            // Rule 1: Kings can be moved to empty columns
            if (target == null)
            {
                bool isKing = moving.value == "K" || moving.value == "KING";
                Console.WriteLine($"Moving to empty column. Card is {moving.value}, isKing: {isKing}");
                return isKing;
            }

            // Rule 2: Cards must be of opposite colors
            bool oppositeColors = IsOppositeColor(moving.suit, target.suit);
            
            // Rule 3: Cards must be in descending order (moving card must be 1 less than target)
            int movingValue = GetCardValue(moving.value);
            int targetValue = GetCardValue(target.value);
            bool correctSequence = movingValue == targetValue - 1;

            Console.WriteLine($"Moving {moving.value} ({movingValue}) of {moving.suit} to {target.value} ({targetValue}) of {target.suit}");
            Console.WriteLine($"Opposite colors: {oppositeColors}, Correct sequence: {correctSequence}");

            return oppositeColors && correctSequence;
        }

        private bool CanMoveToFoundation(Card card, int foundationIndex)
        {
            var foundation = Foundations[foundationIndex];
            
            if (foundation.Count == 0)
            {
                return card.value == "ACE" || card.value == "A";
            }
            
            var top = foundation.Last();
            return card.suit == top.suit && GetCardValue(card.value) == GetCardValue(top.value) + 1;
        }

        private bool IsOppositeColor(string suit1, string suit2)
        {
            return (IsRed(suit1) && !IsRed(suit2)) || (!IsRed(suit1) && IsRed(suit2));
        }

        private bool IsRed(string suit)
        {
            return suit == "HEARTS" || suit == "DIAMONDS";
        }

        private int GetCardValue(string value)
        {
            switch (value.ToUpper())
            {
                case "A":
                case "ACE":
                    return 1;
                case "2":
                    return 2;
                case "3":
                    return 3;
                case "4":
                    return 4;
                case "5":
                    return 5;
                case "6": 
                    return 6;
                case "7":
                    return 7;
                case "8":
                    return 8;
                case "9":
                    return 9;
                case "10":
                    return 10;
                case "JACK":
                case "J":
                    return 11;
                case "QUEEN":
                case "Q":
                    return 12;
                case "KING":
                case "K":
                    return 13;
                default:
                    return 0;
            }
        }
        public void Restart()
        {
            for (int i = 0; i < 7; i++) Tableau[i].Clear();
            for (int i = 0; i < 4; i++) Foundations[i].Clear();
            Waste.Clear();
            Stock.Clear();
            var deck = originalDeck.ToList();
            Shuffle(deck);
            InitializeBoard(deck);
        }

        public bool IsWin()
        {
            // Win if all foundations have 13 cards
            return Foundations.All(f => f.Count == 13);
        }
    }
}
