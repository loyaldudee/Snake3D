using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    // UPDATE: Added "/api" to match your Django urls.py
    [Header("Backend Settings")]
    public string baseUrl = "http://127.0.0.1:8000/api"; 

    private void Awake()
    {
        // Singleton pattern to keep this alive across scenes
        if (Instance == null) { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else { 
            Destroy(gameObject); 
        }
    }

    // --- 1. REGISTER PLAYER ---
    public void RegisterPlayer(string playerName, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(RegisterRoutine(playerName, onSuccess, onError));
    }

    IEnumerator RegisterRoutine(string name, Action<string> onSuccess, Action<string> onError)
    {
        // 1. Create the data object
        RegisterData data = new RegisterData { name = name };
        string json = JsonUtility.ToJson(data);

        // 2. Send Request
        using (UnityWebRequest req = CreateRequest(baseUrl + "/register/", "POST", json))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                // 3. Parse Response
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(req.downloadHandler.text);
                onSuccess?.Invoke(response.player_code);
            }
            else
            {
                onError?.Invoke(req.error);
            }
        }
    }

    // --- 2. SUBMIT SCORE ---
    public void SubmitScore(string playerCode, int score)
    {
        StartCoroutine(SubmitScoreRoutine(playerCode, score));
    }

    IEnumerator SubmitScoreRoutine(string playerCode, int score)
    {
        string json = JsonUtility.ToJson(new ScoreSubmission { player_code = playerCode, score = score });
        
        using (UnityWebRequest req = CreateRequest(baseUrl + "/submit-score/", "POST", json))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) 
                Debug.LogError("API Error: " + req.error + " | " + req.downloadHandler.text);
            else 
                Debug.Log("Score Submitted to Django Successfully!");
        }
    }

    // --- 3. GET LEADERBOARD ---
    public void GetLeaderboard(Action<List<ScoreData>> onSuccess)
    {
        StartCoroutine(GetLeaderboardRoutine(onSuccess));
    }

    IEnumerator GetLeaderboardRoutine(Action<List<ScoreData>> onSuccess)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/leaderboard/"))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                // Wrapper trick for top-level JSON arrays
                string wrappedJson = "{ \"scores\": " + req.downloadHandler.text + " }";
                ScoreList list = JsonUtility.FromJson<ScoreList>(wrappedJson);
                onSuccess?.Invoke(list.scores);
            }
        }
    }

    // --- 4. GET PLAYER HISTORY ---
    public void GetHistory(string playerCode, Action<List<ScoreData>> onSuccess)
    {
        StartCoroutine(GetHistoryRoutine(playerCode, onSuccess));
    }

    IEnumerator GetHistoryRoutine(string playerCode, Action<List<ScoreData>> onSuccess)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/history/" + playerCode + "/"))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string wrappedJson = "{ \"scores\": " + req.downloadHandler.text + " }";
                ScoreList list = JsonUtility.FromJson<ScoreList>(wrappedJson);
                onSuccess?.Invoke(list.scores);
            }
        }
    }

    // --- HELPER ---
    UnityWebRequest CreateRequest(string url, string method, string json)
    {
        UnityWebRequest req = new UnityWebRequest(url, method);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }
}

// --- DATA CLASSES ---
[Serializable] public class RegisterData { public string name; }
[Serializable] public class RegisterResponse { public string player_code; }
[Serializable] public class ScoreSubmission { public string player_code; public int score; }
[Serializable] public class ScoreData { public string name; public int score; public string date; }
[Serializable] public class ScoreList { public List<ScoreData> scores; }