using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using IGOR.Interface.Communication.Actor;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace IGOR.Service.Communication.TwinCat
{
    internal class Actor : IActor
    {
        public string Name { get; }
        public IEnumerable<ActorAction> Actions { get; }

        [JsonIgnore]
        IEnumerable<IActorAction> IActor.Actions => Actions;

        public Actor(string name, IEnumerable<ActorAction> actions)
        {
            Name = name;
            Actions = actions;
        }       

        public static void GenerateSampleJson()
        {
            var actions = new List<ActorAction>
            {
                new ActorAction("Öffen", "Hmi.bopen", "Hmi.bIsOpen"),
                new ActorAction("Schließen", "Hmi.bopen", "Hmi.bIsOpen")
            };

            var actors = new List<Actor>
            {
                new Actor("Greifer", actions),
                new Actor("Greifer2", actions)
            };
            var jsonText = JsonSerializer.Serialize(actors, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic) });
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\Data\Actors.json", jsonText, Encoding.UTF8);
        }

        public static IReadOnlyCollection<Actor> LoadActors()
        {
            var jsonText = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Data\Actors.json", Encoding.UTF8);
            return JsonSerializer.Deserialize<List<Actor>>(jsonText);
        }
    }
}
