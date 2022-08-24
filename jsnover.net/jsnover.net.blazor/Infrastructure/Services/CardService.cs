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

        public CardService(HttpClient client, ISessionStorageService sessionStorageService)
        {
            _client = client;
            _sessionStorageService = sessionStorageService;
        }



        /// <summary>
        /// Gets new shuffled deck from DeckOfCardsApi
        /// </summary>
        /// <returns>New Shuffled Deck as List<Card></returns>
        public async Task<NewDeck> GetDeck()
        {
            var newDeck = new NewDeck() {};
            var response = await _client.GetAsync("https://www.deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1");
            var content = await response.Content.ReadAsStringAsync();
            newDeck = JsonSerializer.Deserialize<NewDeck>(content);
            await _sessionStorageService.SetItemAsync("newDeck", newDeck);

            return newDeck;
        }

        public async Task<CardDraw> DrawAllCards()
        {
            var newDeck = await _sessionStorageService.GetItemAsync<NewDeck>("newDeck");
            var deckResponse = await _client.GetAsync($"https://www.deckofcardsapi.com/api/deck/{newDeck.deck_id}/draw/?count=52");
            var deckContent = await deckResponse.Content.ReadAsStringAsync();
            var newCardDraw = JsonSerializer.Deserialize<CardDraw>(deckContent);

            return newCardDraw;
        }

        // draw any number of cards from one deck {List<Card>}, and retain in different deck{List<Card>}

        // shaffle deck{List<Card>}
    }
}
