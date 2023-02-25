using jsnover.net.blazor.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.SessionStorage;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class CardService
    {
        private HttpClient _client;
        private ISessionStorageService _sessionStorageService;
        private Dictionary<string, List<Card>> GameDictionary;

        public CardService(HttpClient client, ISessionStorageService sessionStorageService)
        {
            _client = client;
            _sessionStorageService = sessionStorageService;
        }

        // shaffle deck{List<Card>}

        /// <summary>
        /// Creates Dictionary in Solitaire Configuration
        /// </summary>
        /// <returns>Dictionary configured for Solitaire GameBoard</returns>
        public async Task<Dictionary<string, List<Card>>> CreateSolitaireDictionary()
        {
            await GetDeck();
            await DrawAllCards();

            GameDictionary = new Dictionary<string, List<Card>>();
            GameDictionary.Add("Column1", await DrawCards(1));
            GameDictionary.Add("Column2", await DrawCards(2));
            GameDictionary.Add("Column3", await DrawCards(3));
            GameDictionary.Add("Column4", await DrawCards(4));
            GameDictionary.Add("Column5", await DrawCards(5));
            GameDictionary.Add("Column6", await DrawCards(6));
            GameDictionary.Add("Column7", await DrawCards(7));
            GameDictionary.Add("HomeDeck", await DrawCards(21));
            GameDictionary.Add("Deal", await DrawCards(3));
            GameDictionary.Add("ClubPile", new List<Card>());
            GameDictionary.Add("SpadePile", new List<Card>());
            GameDictionary.Add("DiamondPile", new List<Card>());
            GameDictionary.Add("HeartPile", new List<Card>());
            GameDictionary.Add("DiscardPile", new List<Card>());

            return GameDictionary;
        }

        /// <summary>
        /// Creates Dictionary in Black Jack Configuration
        /// </summary>
        /// <returns>Dictionary configured for the Black Jack GameBoard </returns>
        public async Task<Dictionary<string, List<Card>>> CreateBlackJackDictionary()
        {
            await GetDeck();
            await DrawAllCards();            

            GameDictionary = new Dictionary<string, List<Card>>();
            GameDictionary.Add("Player", new List<Card>());
            GameDictionary.Add("HomeDeck", await DrawCards(52));
            GameDictionary.Add("Deal", new());
            GameDictionary.Add("DiscardPile", new());

            return GameDictionary;
        }

        /// <summary>
        /// Gets new shuffled deck from DeckOfCardsApi
        /// </summary>
        /// <returns>New Shuffled Deck as List of Cards</returns>
        public async Task<NewDeck> GetDeck()
        {
            var newDeck = await _sessionStorageService.GetItemAsync<NewDeck>("newDeck");
            if(newDeck is null)
            {
                var response = await _client.GetAsync("https://www.deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1");
                var content = await response.Content.ReadAsStringAsync();
                newDeck = JsonSerializer.Deserialize<NewDeck>(content);
                await _sessionStorageService.SetItemAsync("newDeck", newDeck);
            }            

            return newDeck;
        }

        /// <summary>
        /// Draws Full Deck of Cards from DeckofCardsApi
        /// </summary>
        /// <returns>Full Deck</returns>
        public async Task<CardDraw> DrawAllCards()
        {
            var newDeck = await _sessionStorageService.GetItemAsync<NewDeck>("newDeck");
            var deckResponse = await _client.GetAsync($"https://www.deckofcardsapi.com/api/deck/{newDeck.deck_id}/draw/?count=52");
            var deckContent = await deckResponse.Content.ReadAsStringAsync();
            var newCardDraw = JsonSerializer.Deserialize<CardDraw>(deckContent);
            await _sessionStorageService.SetItemAsync("Deck", newCardDraw);

            return newCardDraw;
        }

        /// <summary>
        /// Draw any number of cards from the remaining cards in the deck
        /// </summary>
        /// <param name="count"></param>
        /// <returns><see cref="List{T}"/> of <see cref="Card"/> based of the count passed in.</returns>
        public async Task<List<Card>> DrawCards(int count)
        {
            var cards = new List<Card>();
            var deck = await _sessionStorageService.GetItemAsync<CardDraw>("Deck");

            for (int i = count - 1; i >= 0; i--)
            {
                cards.Add(deck.cards[i]);
                deck.cards.RemoveAt(i);
            }

            return cards;
        }
    }
}
