//namespace CMS.Mcp.Server.Monkey
//{
//    using System;
//    using System.ComponentModel;
//    using System.Text.Json;
//    using System.Threading.Tasks;
//    using ModelContextProtocol.Server;

//    [McpServerResourceType]
//    public class MonkeyResources(MonkeyService monkeyService)
//    {
//        [McpServerResource(UriTemplate = "monkeymcp://monkeys/baboon", Name = "Baboon", MimeType = "application/json")]
//        [Description("Get details of a baboon monkey")]
//        public async Task<string> Baboon()
//        {
//            var baboon = await monkeyService.GetMonkey("Baboon") ?? throw new Exception($"Baboon not found");
//            return JsonSerializer.Serialize(baboon);
//        }

//        [McpServerResource(UriTemplate = "monkeymcp://monkeys/{name}", Name = "Monkey")]
//        [Description("Get monkey details by name")]
//        public async Task<string> Monkey(string name)
//        {
//            var monkey = await monkeyService.GetMonkey(name) ?? throw new Exception($"{name} not found");
//            return JsonSerializer.Serialize(monkey);
//        }
//    }
//}