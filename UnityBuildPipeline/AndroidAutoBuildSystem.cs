using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// 안드로이드 빌드 파이프라인 자동화 시스템
/// 서버 환경별 빌드 세팅 자동 변경
/// 빌드 파일 타겟 설정 및 에셋번들, AndroidManifest, 아이콘, 이름 등을 동적 제어
/// </summary>
public class AndroidAutoBuildSystem : MonoBehaviour
{
    public enum ServerType
    {
        Dev,
        Stage,
        Live
    }

    static string TargetDir;
    static string StreammingAssetsDir;

    //static string[] BuildScenes = new string[2] { EditorBuildSettings.scenes[1].path, EditorBuildSettings.scenes[2].path };

    static string apk = "apk";
    static string aab = "aab";

    #region Editor Menu
    [MenuItem("Android Build/Dev/withAssets")]
    static void DevBuildwAssets()
    {
        AndroidBuild(apk, true, ServerType.Dev);
    }
    [MenuItem("Android Build/Stage/withAssets")]
    static void StageBuildwAssets()
    {
        AndroidBuild(apk, true, ServerType.Stage);
    }
    [MenuItem("Android Build/Live/withAssets")]
    static void LiveBuildwAssets()
    {
        AndroidBuild(apk, true, ServerType.Live);
    }
    [MenuItem("Android Build/Dev/withoutAssets/apk")]
    static void DevBuildwoAssetsAPK()
    {
        AndroidBuild(apk, false, ServerType.Dev);
    }
    [MenuItem("Android Build/Dev/withoutAssets/aab")]
    static void DevBuildwoAssetsABB()
    {
        AndroidBuild(aab, false, ServerType.Dev);
    }
    [MenuItem("Android Build/Stage/withoutAssets/apk")]
    static void StageBuildwoAssetsAPK()
    {
        AndroidBuild(apk, false, ServerType.Stage);
    }
    [MenuItem("Android Build/Stage/withoutAssets/aab")]
    static void StageBuildwoAssetsABB()
    {
        AndroidBuild(aab, false, ServerType.Stage);
    }
    [MenuItem("Android Build/Live/withoutAssets/apk")]
    static void LiveBuildwoAssetsAPK()
    {
        AndroidBuild(apk, false, ServerType.Live);
    }
    [MenuItem("Android Build/Live/withoutAssets/aab")]
    static void LiveBuildwoAssetsABB()
    {
        AndroidBuild(aab, false, ServerType.Live);
    }
    #endregion

    /// <summary>
    /// 안드로이드 빌드 실행
    /// </summary>
    /// <param name="fileType">파일 확장자 apk 또는 aab</param>
    /// <param name="iswAsset">에셋 빌드 하는가</param>
    /// <param name="sType">연결할 서버 타입</param>
    private static void AndroidBuild(string fileType, bool iswAsset, ServerType sType)
    {
        try {
            BuildTargetDirSetting(fileType);
            if (iswAsset) {
                SetStreamingAssets();
            }
            KeyStoreSetting();
            IconSettings(sType);
            AppNameSettings(sType);

            // Live빌드의 경우에만 광고 설정합니다.
            if(sType == ServerType.Live)
            {
                SetLiveAdConfigs();
            }

            PerformBuild();
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            CleanUp();
            //빌드 이후(오류 포함) 무조건 테스트광고로 전환
            SetTestAdConfigIds();
        }
    }

    /// <summary>
    /// 빌드 위치, 이름 지정
    /// </summary>
    /// <param name="fileType">안드로이드 빌드 확장자 apk 또는 aab</param>
    static void BuildTargetDirSetting(string fileType)
    {
        //apk, abb 선택
        targetDir = EditorUtility.SaveFilePanel("Build Android", "", "FortressW", fileType);

        EditorUserBuildSettings.buildAppBundle = fileType.Equals("aab");
    }

    /// <summary>
    /// 빌드된 Asset들 StreammingAsset의 AssetBundles로 복사하기
    /// </summary>
    static void SetStreamingAssets()
    {
        //Create Asset Dir
        streamingAssetsDir = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        if(Directory.Exists(streamingAssetsDir))
        {
            Directory.Delete(streamingAssetsDir, true);
        }
        Directory.CreateDirectory(streamingAssetsDir);

        //Assets Move
        string sourceAssetDir = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), "AssetBundles/Android");
        if (Directory.Exists(sourceAssetDir))
        {
            DirectoryInfo assetDirInfo = new DirectoryInfo(sourceAssetDir);
            FileInfo[] assetFiles = assetDirInfo.GetFiles();
            foreach (FileInfo file in assetFiles)
            {
                file.CopyTo(Path.Combine(streamingAssetsDir, file.Name), true);
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 안드로이드 빌드시 서명 keystore 설정 자동화
    /// </summary>
    static void KeyStoreSetting()
    {
        string keystorePath = Path.Combine(Application.dataPath, "../Keystore/ProjectKeyStore.keystore");
        string[] lines = File.ReadAllLines(Path.Combine(Application.dataPath, "../Keystore/Keystore.txt"));

        if (!File.Exists(infoPath)) return;

        if (lines.length == 3)
        {
            string keypass = lines[0];
            string alias = lines[1];
            string aliaspass = lines[2];

            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keypass;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliaspass;
        }
    }

    /// <summary>
    /// 연결할 서버에 따라 앱 icon을 설정
    /// </summary>
    static void IconSettings(ServerType type)
    {
        //icon 경로
        string path = string.Concat("Assets/Build/Icons/Project_", type.ToString(), ".png");
        Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

        if (icon == null) return;

        //현재 안드로이드 빌드용 아이콘 정보 불러와 덮어씌위기
        UnityEditor.Build.NamedBuildTarget platform = UnityEditor.Build.NamedBuildTarget.Android;
        PlatformIcon[] iconsLegacy = PlayerSettings.GetPlatformIcons(platform, AndroidPlatformIconKind.Legacy);
        for (int i = 0; i < iconsLegacy.Length; i++)
        {
            iconsLegacy[i].SetTexture(icon);
        }

        //PlayerSettings 의 icon 변경
        PlayerSettings.SetPlatformIcons(platform, AndroidPlatformIconKind.Legacy, iconsLegacy);
        PlayerSettings.SetIcons(platform, new Texture2D[] { icon }, IconKind.Notification);
    }

    /// <summary>
    /// 연결할 서버에 따라 패키지 이름 설정
    /// </summary>
    /// <param name="type">서버 타입</param>
    static void AppNameSettings(ServerType type)
    {
        //정리되어 있는 json 파일 불러옴
        var serverInfo = null;
        //패키지, 앱 이름 설정
        string packageName = string.Concat("com.company.samplegame", serverInfo.scheme);
        string productName = serverInfo.productName;
        //Debug.Log($"packageName : {packageName} / productName : {productName}");

        //패키지, 앱 이름 변경
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, packageName);
        PlayerSettings.productName = productName;

        //AndroidManifest 설정
        SetAndroidManifestXML(serverInfo.scheme);
    }

    /// <summary>
    /// 각 서버에 맞게 AndroidManifest.xml 수정 (scheme 수정)
    /// </summary>
    /// <param name="type">서버 타입</param>
    static void SetAndroidManifestXML(string scheme)
    {
        string androidManifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

        if (!File.Exists(androidManifestPath))
        {
            Debug.LogError("AndroidManifest Not Found!!!!!!!!!");
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(androidManifestPath);

        XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        XmlNode node = xmlDoc.SelectSingleNode("//data[@android:scheme]", nsMgr);

        if (node != null)
        {
            XmlElement element = (XmlElement)node;
            element.SetAttribute("scheme", "http://schemas.android.com/apk/res/android", scheme);
            xmlDoc.Save(androidManifestPath);
        }
        else
        {
            Debug.LogError("android:scheme NOT FOUND !!!!!!!!!!!!!!!!");
        }
    }

    /// <summary>
    /// AdConfig ScriptableObject설정
    /// Live 빌드시에만 실제광고 ID를 설정해야합니다.
    /// </summary>
    static void SetLiveAdConfigs()
    {
        // Live 환경 라이브 광고 ID 및 테스트 모드 비활성화 설정 로직
    }

    /// <summary>
    /// AdConfig ScriptableObject설정
    /// 빌드이후 ADConfig 광고ID를 테스트 광고로 설정
    /// </summary>
    static void SetTestAdConfigIds()
    {
        // 안전 장치: 빌드 종료 후 기본 테스트 광고 ID로 자동 복원
    }

    /// <summary>
    /// 최종 빌드 실행
    /// </summary>
    static void PerformBuild()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        var report = BuildPipeline.BuildPlayer(BuildScenes, targetDir, BuildTarget.Android, BuildOptions.None);
        if (report != null)
        {
            Debug.Log("Build Result - " + report.summary.result + "\n" +
                      "Build Start - " + report.summary.buildStartedAt + "\n" +
                      "Build End - " + report.summary.buildEndedAt + "\n" +
                      "Build Size - " + report.summary.totalSize + "\n" +
                      "Build Error Count - " + report.summary.totalErrors);
        }
    }

    /// <summary>
    /// 다시 에디터에서 실행해도 문제없게 원래대로 되돌리기
    /// </summary>
    static void CleanUp()
    {
        if (Directory.Exists(streamingAssetsDir))
        {
            Directory.Delete(streamingAssetsDir, true);
        }

        EditorUserBuildSettings.buildAppBundle = false;
        IconSettings(ServerType.Live);
        AppNameSettings(ServerType.Live);
        SetAndroidManifestXML("sampleGame");
        ServerSelector.ChangeServer(ServerType.Dev);
    }

}
