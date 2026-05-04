// Example: Simple GET request in Unity C#
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleGetRequest : MonoBehaviour
{
    async void Start()
    {
        string url = "http://192.168.1.217:8000/api/unity_resources/debug"; // Replace with your actual URL
        await MakeGetRequest(url);
    }
    private async Task MakeGetRequest(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    Debug.Log("Response data: " + responseData);
                }
                else
                {
                    Debug.LogError("Error: " + response.StatusCode);
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError("Request error: " + e.Message);
            }
        }
    }
}