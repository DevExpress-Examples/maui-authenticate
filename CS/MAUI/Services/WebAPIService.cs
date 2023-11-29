using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ON = DevExpress.Maui.Core.Internal.On;
namespace MAUI.Services;

public class WebAPIService : IDataStore {
    private readonly HttpClient HttpClient = new() { Timeout = new TimeSpan(0, 0, 10) };
#if DEBUG
    public static readonly string ApiUrl = ON.Platform("http://10.0.2.2:5000/api/", "http://localhost:5000/api/");
#else
    public static readonly string ApiUrl = ON.Platform("https://10.0.2.2:5001/api/", "https://localhost:5001/api/");
#endif
    private const string ApplicationJson = "application/json";



    public async Task<string> Authenticate(string userName, string password) {
        HttpResponseMessage tokenResponse;
        try {
            tokenResponse = await RequestTokenAsync(userName, password);
        }
        catch (Exception) {
#if DEBUG
            await Application.Current.MainPage.DisplayAlert("Couldn't reach the WebAPI service", "Potential reasons: \n\n- The WebAPI project is not started. Please right-click the WebAPI project and choose Debug -> Start New Instance \n\n- You debug the project using a physical device. Please try using an emulator \n\n- IIS Express on Windows blocks the access. Please follow the recommendations in the example description", "OK");
#endif
            return "An error occurred when processing the request";
        }
        if (tokenResponse.IsSuccessStatusCode) {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await tokenResponse.Content.ReadAsStringAsync());
            return null;
        }
        else {
            return await tokenResponse.Content.ReadAsStringAsync();
        }
    }
    private async Task<HttpResponseMessage> RequestTokenAsync(string userName, string password) {
        return await HttpClient.PostAsync($"{ApiUrl}Authentication/Authenticate",
                                            new StringContent(JsonSerializer.Serialize(new { userName, password = $"{password}" }), Encoding.UTF8,
                                            ApplicationJson));
    }
}

