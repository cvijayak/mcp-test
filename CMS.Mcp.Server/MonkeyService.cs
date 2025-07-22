namespace CMS.Mcp.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Contracts;

    public class MonkeyService(IHttpClientFactory httpClientFactory)
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private List<Monkey> _monkeyList = [];
    
        public async Task<List<Monkey>> GetMonkeys() {
            if (_monkeyList.Count > 0)
                return _monkeyList;

            // Online
            var response = await _httpClient.GetAsync("https://www.montemagno.com/monkeys.json");
            if (response.IsSuccessStatusCode) {
                _monkeyList = await response.Content.ReadFromJsonAsync<List<Monkey>>() ?? [];
            }

            _monkeyList ??= [];

            return _monkeyList;
        }

        public async Task<Monkey> GetMonkey(string name) {
            var monkeys = await GetMonkeys();
            return monkeys.FirstOrDefault(m => m.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}