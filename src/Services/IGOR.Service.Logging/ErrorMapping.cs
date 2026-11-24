using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IGOR.Service.Logging
{
    [Serializable]
    internal class ErrorMapping
    {
        [JsonInclude]
        public Dictionary<uint, string> SourceNames { get; }

        [JsonInclude]
        public Dictionary<uint, string> Descriptions { get; }

        public ErrorMapping(Dictionary<uint, string> sourceNames, Dictionary<uint,string> descriptions)
        {
            SourceNames = sourceNames;
            Descriptions = descriptions;
        }


        public static ErrorMapping GetErrorMapping()
        {
            var json = File.ReadAllText(Environment.CurrentDirectory + @"\Data\ErrorMessages.json");
            try
            {
                return JsonSerializer.Deserialize<ErrorMapping>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
