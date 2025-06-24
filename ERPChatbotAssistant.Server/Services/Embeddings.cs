using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class EmbeddingsHelper
{
    public static async Task<float[]> GetBgeEmbeddingAsync(string text, string apiToken)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var payload = new
        {
            inputs = text,
            options = new { wait_for_model = true }
        };
 
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api-inference.huggingface.co/models/BAAI/bge-small-en-v1.5", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var embedding = JsonSerializer.Deserialize<List<float>>(json);
        return embedding.ToArray();
    }

    public static double CosineSimilarity(float[] v1, float[] v2)
    {
        double dot = 0.0, mag1 = 0.0, mag2 = 0.0;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += System.Math.Pow(v1[i], 2);
            mag2 += System.Math.Pow(v2[i], 2);
        }
        return dot / (System.Math.Sqrt(mag1) * System.Math.Sqrt(mag2));
    }
}