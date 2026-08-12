using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance { get; private set; }
    [HideInInspector] public string deeplinkURL;

    private string targetPackageName = "com.partner.authapp";
    private string targetScheme = "partner-auth";
    private string targetHost = "auth.partner.com";
    private string targetAppId = "sample-app-id-12345";
    private string myScheme = "samplegame";

    // 딥링크를 통해 받은 데이터를 다른 곳에 전달하기 위한 이벤트
    public static Action OnResponseReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!String.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        CretaUserData.CreateCretaUserData();

        StartCoroutine(SetPackageName());
    }

    /// <summary>
    /// 환경 설정 파일 동적 로드
    /// </summary>
    /// <returns></returns>
    IEnumerator SetPackageName()
    {

        string json = string.Empty;

#if UNITY_EDITOR
        string filePath = Path.Combine(Application.streamingAssetsPath, "env.json");
        if (File.Exists(filePath))
        {
            json = File.ReadAllText(filePath);
        }
#elif UNITY_ANDROID
        yield return LoadEnvJson(result => json = result);
#endif
        if (!string.IsNullOrEmpty(json))
        {
            var config = JsonUtility.FromJson<ServerEnvConfig>(json);
            if (config != null)
            {
                targetPackageName = config.targetPackageName;
                targetScheme = config.targetScheme;
                targetHost = config.targetHost;
                targetAppId = config.targetAppId;
                myScheme = config.scheme;
            }
        }
        yield return null;
    }

    /// <summary>
    /// 딥링크 받으면 호출되는 함수
    /// </summary>
    /// <param name="url">수신 정보</param>
    private void onDeepLinkActivated(string url)
    {
        deeplinkURL = url;
        Uri uri = new Uri(deeplinkURL);

        if (uri.Host == "action1")
        {
            // action 1에 따른 정보 처리 코드
            var queryParams = ParseQueryString(uri.Query);
            if (queryParams.TryGetValue("name01", out string name))
            {
                // string name 처리
            }
        }
        else if(uri.Host == "action2")
        {
            // action 2에 따른 정보 처리 코드
            var queryParams = ParseQueryString(uri.Query);
            if (queryParams.TryGetValue("name01", out string name))
            {
                // string name 처리
            }
        }
        else
        {
            Debug.LogError($"uri Host: {uri.Host} 뭔가 이상함");
        }

        if(bool networkOnline)
        {
            OnResponseReceived?.Invoke();
        }
    }

    /// <summary>
    /// 서버와 소통이 가능할때 액션 invoke
    /// </summary>
    public void InvokeResponseAction()
    {
        OnResponseReceived?.Invoke();
    }

    /// <summary>
    /// URL의 쿼리 스트링(?a=1&b=2)을 Dictionary 형태로 변환하는 헬퍼 함수
    /// </summary>
    private Dictionary<string, string> ParseQueryString(string query)
    {
        var dictionary = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(query)) return dictionary;

        string cleanQuery = query.TrimStart('?');
        string[] pairs = cleanQuery.Split('&');

        foreach (var pair in pairs)
        {
            // '=' 문자를 기준으로 key와 value를 분리
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                var key = Uri.UnescapeDataString(parts[0]);
                var value = Uri.UnescapeDataString(parts[1]);
                dictionary[key] = value;
            }
        }
        return dictionary;
    }

    /// <summary>
    /// 딥링크로 앱 호출
    /// </summary>
    public void OpenTargetApp()
    {
        if (IsAppInstalled_Android(targetPackageName))
        {
            string finalUrl = $"{targetScheme}://{targetAppId}";
            OpenExternalAppasNewTask(finalUrl);
        }
        else
        {
            Application.OpenURL($"market://details?id={targetPackageName}");
        }
    }

    /// <summary>
    /// 앱에게 딥링크 호출 (action1)
    /// </summary>
    public void AskTargetAction1()
    {
        if (IsAppInstalled_Android(targetScheme))
        {
            string encodedUrl = System.Uri.EscapeDataString($"{myScheme}://action-1");
            string finalUrl = $"{targetScheme}://action1?callbackUrl={encodedUrl}";
            OpenExternalAppasNewTask(finalUrl);
        }
        else
        {
            Application.OpenURL($"market://details?id={targetScheme}");
        }
    }

    /// <summary>
    /// 앱에게 딥링크 호출 (action2)
    /// </summary>
    public void AskTargetAction2()
    {
        if (IsAppInstalled_Android(targetScheme))
        {
            string encodedUrl = System.Uri.EscapeDataString($"{myScheme}://action-2");
            string finalUrl = $"{targetScheme}://action2?callbackUrl={encodedUrl}";
            OpenExternalAppasNewTask(finalUrl);
        }
        else
        {
            Application.OpenURL($"market://details?id={targetScheme}");
        }
    }

    public void OpenTargetAction3()
    {
        if (IsAppInstalled_Android(targetScheme))
        {
            string finalUrl = $"{targetScheme}://action-3?action333";
            OpenExternalAppasNewTask(finalUrl);
        }
        else
        {
            Application.OpenURL($"market://details?id={targetScheme}");
        }
    }

    /// <summary>
    /// action2 이후 행동
    /// </summary>
    public void HandleAction1Response()
    {
        //...정보 처리 함수

        //이벤트 초기화
        DeepLinkManager.OnResponseReceived -= HandleAction1Response;
    }

    /// <summary>
    /// action2 이후 행동
    /// </summary>
    public void HandleAction2Response()
    {
        //...정보 처리 함수

        //이벤트 초기화
        DeepLinkManager.OnResponseReceived -= HandleAction2Response;
    }

    /// <summary>
    /// 찾는 앱이 현재 안드로이드 기기에 설치되어 있는가 확인
    /// </summary>
    /// <param name="packageName">찾는 앱의 packageName</param>
    /// <returns>bool 설치 유무</returns>
    bool IsAppInstalled_Android(string packageName)
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            {
                packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.Log($"앱 못찾음 {e.ToString()}");
            return false;
        }
    }

    /// <summary>
    /// 안드로이드에서 다른 앱으로 실행되게
    /// </summary>
    /// <param name="url">실행할 url</param>
    void OpenExternalAppasNewTask(string url)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intentClass = new AndroidJavaClass("android.content.Intent"))
            {
                string actionView = intentClass.GetStatic<string>("ACTION_VIEW");
                using (var intentObject = new AndroidJavaObject("android.content.Intent", actionView))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", url))
                { 
                    intentObject.Call<AndroidJavaObject>("setData", uriObject);
                    int flagNewTask = intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
                    intentObject.Call<AndroidJavaObject>("addFlags", flagNewTask);
                    currentActivity.Call("startActivity", intentObject);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Android] Error opening external app: {e.Message}");
            // 네이티브 호출 실패 시 기존 방식으로 다시 시도
            Application.OpenURL(url);
        }
#else
        // 안드로이드가 아닌 플랫폼(iOS, 에디터 등)에서는 기존 방식 사용
        Application.OpenURL(url);
#endif
    }

    /// <summary>
    /// 안드로이드에서 UnityWebRequest를 통해 StreamingAssets폴더에서 env.json을 읽어옴
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadEnvJsonFromStreamingAssets(Action<string> callback)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "env.json");

        // Android: StreamingAssets는 jar 내부 → 반드시 UnityWebRequest 사용
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                callback?.Invoke(www.downloadHandler.text);
            else
            {
                callback?.Invoke(null);
            }
        }
        else
        {
            try
            {
                string json = File.ReadAllText(filePath);
                callback?.Invoke(json);
            }
            catch (Exception e)
            {
                callback?.Invoke(null);
            }
        }
    }
}

public class DeepLinkRecieveExample : MonoBehaviour
{
    public void OnBTNAction1()
    {
        DeepLinkManager.OnResponseReceived += HandleAction1Responce;
        DeepLinkManager.Instance.AskTargetAction1();
    }

    void HandleAction1Response()
    {
        //..정보 받은 후 행동들 함수
        DeepLinkManager.OnResponseReceived -= HandleAction1Responce;
    }
}