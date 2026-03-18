using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string baseUrl = "http://localhost:3000/api/player";
    public string localPlayerId;

    [Header("UI - Registration")]
    public TMP_InputField regUsername;
    public TMP_InputField regEmail;
    public TMP_InputField regPassword;

    [Header("UI - Login")]
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;

    [Header("UI - Dashboard & Settings")]
    public TMP_InputField scoreKillsInput;
    public TMP_InputField scoreDeathsInput;
    public TMP_InputField newPasswordInput;
    public TMP_Text scoreDisplay; 
    public void OnRegisterClick() => Register(regUsername.text, regEmail.text, regPassword.text);

    public void OnLoginClick() => Login(loginUsername.text, loginPassword.text);

    public void OnViewScoreClick() => GetScore();

    public void OnUpdateScoreClick()
    {
        int k = int.Parse(scoreKillsInput.text);
        int d = int.Parse(scoreDeathsInput.text);
        UpdateScore(k, d);
    }

    public void OnUpdatePasswordClick() => UpdatePassword(newPasswordInput.text);

    public void OnDeleteAccountClick() => DeleteAccount();



    public void Register(string user, string email, string pass)
    {
        string json = "{\"username\":\"" + user + "\", \"email\":\"" + email + "\", \"password\":\"" + pass + "\"}";
        StartCoroutine(SendRequest(baseUrl + "/register", "POST", json));
    }

    public void Login(string user, string pass)
    {
        string json = "{\"username\":\"" + user + "\", \"password\":\"" + pass + "\"}";
        StartCoroutine(SendRequest(baseUrl + "/login", "POST", json, (response) => {
            if (response.success)
            {
                localPlayerId = response.data._id;
                Debug.Log("Logged in! ID: " + localPlayerId);
            }
        }));
    }

    public void GetScore()
    {
        StartCoroutine(SendRequest(baseUrl + "/" + localPlayerId, "GET", "", (res) => {
            if (scoreDisplay != null)
                scoreDisplay.text = $"Kills: {res.data.kills} | Deaths: {res.data.deaths}";
        }));
    }

    public void UpdateScore(int k, int d)
    {
        string json = "{\"id\":\"" + localPlayerId + "\", \"kills\":" + k + ", \"deaths\":" + d + "}";
        StartCoroutine(SendRequest(baseUrl + "/score", "PUT", json));
    }

    public void UpdatePassword(string newPass)
    {
        string json = "{\"id\":\"" + localPlayerId + "\", \"newPassword\":\"" + newPass + "\"}";
        StartCoroutine(SendRequest(baseUrl + "/updatePassword", "PUT", json));
    }

    public void DeleteAccount() => StartCoroutine(SendRequest(baseUrl + "/" + localPlayerId, "DELETE", ""));

    [System.Serializable] public class PlayerResponse { public bool success; public PlayerData data; }
    [System.Serializable] public class PlayerData { public string _id; public int kills; public int deaths; }

    IEnumerator SendRequest(string url, string method, string json, System.Action<PlayerResponse> callback = null)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.SetRequestHeader("Content-Type", "application/json");
            }
            req.downloadHandler = new DownloadHandlerBuffer();
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                PlayerResponse res = JsonUtility.FromJson<PlayerResponse>(req.downloadHandler.text);
                callback?.Invoke(res);
                Debug.Log(method + " Success");
            }
            else
            {
                Debug.LogError("Error: " + req.error);
            }
        }
    }
}