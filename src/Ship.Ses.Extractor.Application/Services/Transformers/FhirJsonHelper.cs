using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Services.Transformers
{
    public static class FhirJsonHelper
    {
        public static void ApplyConstants(JsonNode root, Dictionary<string, JsonNode> constants, ILogger logger = null)
        {
            foreach (var kvp in constants)
            {
                SetFhirValue(root, kvp.Key, kvp.Value, logger);
            }
        }
        public static void SetFhirValue(JsonNode root, string fhirPath, JsonNode value, ILogger? logger = null)
        {
            var parts = fhirPath.Replace("]", "").Split(new[] { '[', '.' }, StringSplitOptions.RemoveEmptyEntries);
            JsonNode current = root;
            var pathTrace = new List<string>();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool isLast = i == parts.Length - 1;
                string? nextPart = i + 1 < parts.Length ? parts[i + 1] : null;
                pathTrace.Add(part);

                if (int.TryParse(part, out int index)) // array index
                {
                    if (current is not JsonArray array)
                    {
                        logger?.LogError("❌ Failed at '{Path}': Expected JsonArray, but found {Type}.", string.Join(".", pathTrace), current.GetType().Name);
                        throw new InvalidOperationException($"Expected array at '{string.Join(".", pathTrace)}' but found {current?.GetType().Name ?? "null"}");
                    }

                    while (array.Count <= index)
                        array.Add(new JsonObject());

                    if (isLast)
                    {
                        array[index] = value;
                        logger?.LogInformation("📌 Applied constant to {FhirPath}", fhirPath);
                        return;
                    }

                    current = array[index] ??= new JsonObject();
                }
                else // object key
                {
                    if (isLast)
                    {
                        var clonedValue = value.DeepClone();
                        current[part] = clonedValue;
                        logger?.LogInformation("📌 Applied constant to {FhirPath}", fhirPath);
                        return;
                    }

                    // Determine what structure to create
                    if (current[part] == null)
                    {
                        current[part] = int.TryParse(nextPart, out _) ? new JsonArray() : new JsonObject();
                    }
                    else if (int.TryParse(nextPart, out _) && current[part] is not JsonArray)
                    {
                        logger?.LogError("❌ Expected JsonArray at '{Path}', but got {Type}.", string.Join(".", pathTrace), current[part]?.GetType().Name ?? "null");
                        throw new InvalidOperationException($"Expected JsonArray at '{string.Join(".", pathTrace)}', but got {current[part]?.GetType().Name}");
                    }

                    current = current[part]!;
                }
            }
        }
    }

}
