using jsnover.net.blazor.Models;
using System.Collections.Generic;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class CardsService
    {
        /// <summary>
        /// Gets new shuffled deck from DeckOfCardsApi
        /// </summary>
        /// <returns>New Shuffled Deck as List<Card></returns>
        public List<Card> GetDeck()
        {
            var newDeck = new List<Card>() {};
            return newDeck;
        }

        // draw cards from api

        // draw any number of cards from one deck {List<Card>}, and retain in different deck{List<Card>}

        // shaffle deck{List<Card>}
    }
}
