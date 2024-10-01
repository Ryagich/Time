using System;
using System.Net.Http;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public static class TimeFetcher
{
    private const string url = "https://yandex.com/time/sync.json";

    public static async UniTask<DateTime> FetchTimeFromServer()
    {
        using var webRequest = UnityWebRequest.Get(url);
        var operation = webRequest.SendWebRequest();
        await operation.ToUniTask();
        if (webRequest.result != UnityWebRequest.Result.Success)
            throw new Exception($"Ошибка при запросе времени: {webRequest.error}");
        var serverTimeResponse = JsonConvert.DeserializeObject<ServerTimeResponse>(webRequest.downloadHandler.text);
        return DateTimeOffset.FromUnixTimeMilliseconds(serverTimeResponse.time).DateTime;
    }
}