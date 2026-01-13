using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class AssetLoader
{
    public static IEnumerator LoadSprite(string path, System.Action<Sprite> onCompleted)
    {
        if (string.IsNullOrEmpty(path))
        {
            onCompleted?.Invoke(null);
            yield break;
        }

        string url = "file://" + path;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
                onCompleted?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                onCompleted?.Invoke(sprite);
            }
        }
    }

    public static IEnumerator LoadAudio(string path, System.Action<AudioClip> onCompleted)
    {
        string url = "file://" + path;
        
        AudioType audioType = AudioType.UNKNOWN;
        if (path.EndsWith(".mp3")) audioType = AudioType.MPEG;
        else if (path.EndsWith(".wav")) audioType = AudioType.WAV;
        else if (path.EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;

        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
                onCompleted?.Invoke(null);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                onCompleted?.Invoke(clip);
            }
        }
    }

    public static IEnumerator LoadTexture(string path, System.Action<Texture2D> onCompleted)
    {
        if (string.IsNullOrEmpty(path))
        {
            onCompleted?.Invoke(null);
            yield break;
        }
        string url = "file://" + path;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
                onCompleted?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                onCompleted?.Invoke(texture);
            }
        }
    }
}