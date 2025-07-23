namespace CMS.Mcp.Server.Monkey
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CMS.Mcp.Server.Contracts.Monkey;
    using Contracts;
    using Microsoft.AspNetCore.Authorization;
    using ModelContextProtocol.Server;

    [Authorize]
    [McpServerToolType]
    public sealed class MonkeyTools(MonkeyService monkeyService, MonkeyLocationService locationService)
    {
        [McpServerTool(Title = "GetMonkeys"), Description("Get a list of monkeys.")]
        public async Task<string> GetMonkeys() {
            var monkeys = await monkeyService.GetMonkeys();
            return JsonSerializer.Serialize(monkeys);
        }

        [McpServerTool(Title = "GetMonkey"), Description("Get a monkey by name.")]
        public async Task<string> GetMonkey([Description("The name of the monkey to get details for")] string name) {
            var monkey = await monkeyService.GetMonkey(name);
            return JsonSerializer.Serialize(monkey);
        }

        [McpServerTool(Title = "GetMonkeyBusiness"), Description("Monkey Business, outputs random monkey and monkey-adjacent emoji")]
        public Task<string> GetMonkeyBusiness() {
            var monkeyEmojis = new[] { "🐵", "🐒", "🦍", "🦧", "🙈", "🙉", "🙊", "🍌", "🌴", "🥥", "🌿", "🐾" };
            var random = new Random();
            var count = random.Next(3, 7);

            var result = "";
            for (int i = 0; i < count; i++) {
                result += monkeyEmojis[random.Next(monkeyEmojis.Length)];
            }

            return Task.FromResult(result);
        }

        [McpServerTool(Title = "GetMonkeyJourney"), Description("Get a unique journey path with activities and health stats for a specific monkey.")]
        public async Task<string> GetMonkeyJourney([Description("The name of the monkey to get a journey for")] string name) {
            var monkey = await monkeyService.GetMonkey(name);
            if (monkey == null) {
                return JsonSerializer.Serialize(new { error = $"Monkey '{name}' not found" });
            }

            var journey = locationService.GenerateMonkeyJourney(monkey);
            return JsonSerializer.Serialize(journey);
        }

        [McpServerTool(Title = "GetAllMonkeyJourneys"), Description("Get journey paths for all available monkeys.")]
        public async Task<string> GetAllMonkeyJourneys() {
            var monkeys = await monkeyService.GetMonkeys();
            var journeys = new List<MonkeyJourney>();

            foreach (var monkey in monkeys) {
                var journey = locationService.GenerateMonkeyJourney(monkey);
                journeys.Add(journey);
            }

            return JsonSerializer.Serialize(journeys);
        }
    }
}