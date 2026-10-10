using System.Text.Json.Nodes;

namespace ThePhotographersForecast.Data
{
    public static class JsonCleaner
    {
        public static string RemoveNullProperties(string json)
        {
            JsonNode? root = JsonNode.Parse(json);
            if (root is JsonObject obj) RemoveNulls(obj);
            else if (root is JsonArray arr) RemoveNulls(arr);
            return root?.ToJsonString() ?? "{}";
        }

        static void RemoveNulls(JsonObject obj)
        {
            var keys = obj.Select(kvp => kvp.Key).ToList(); // avoid modifying while enumerating
            foreach (var key in keys)
            {
                JsonNode? child = obj[key];
                if (child == null || (child is JsonValue v && v.ToJsonString() == "null"))
                {
                    obj.Remove(key);
                    continue;
                }

                if (child is JsonObject childObj) RemoveNulls(childObj);
                else if (child is JsonArray childArr) RemoveNulls(childArr);
            }
        }

        static void RemoveNulls(JsonArray arr)
        {
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                JsonNode? child = arr[i];
                if (child == null || (child is JsonValue v && v.ToJsonString() == "null"))
                {
                    arr.RemoveAt(i);
                    continue;
                }

                if (child is JsonObject childObj) RemoveNulls(childObj);
                else if (child is JsonArray childArr) RemoveNulls(childArr);
            }
        }
    }
}
