using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class TextPanelSplit
{
    public void OnClickPolicies()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(PolicyAndroid());
#else
        PolicyWindow();
    }

    void PolicyWindow()
    {
        string filePath = string.Format("{0}/OperatingPolicy/OperatingPolicy_{1}.txt", Application.streamingAssetsPath, Application.systemLanguage.ToString());
        FileInfo fileInfo = new FileInfo(filePath);
        string text = "";

        if (fileInfo.Exists)
        {
            StreamReader reader = new StreamReader(filePath);
            text = reader.ReadToEnd();
            reader.Close();
        }
        else
        {
            text = "File doesn't Exist...";
        }
        SplitPanelText(text);
    }

    IEnumerator PolicyAndroid()
    {
        string path = string.Format("{0}/OperatingPolicy/OperatingPolicy_{1}.txt", Application.streamingAssetsPath, Application.systemLanguage.ToString());
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load OperatingPolicy: " + www.error);
        }
        else
        {
            string content = www.downloadHandler.text;
            SplitPanelText(content);
        }
    }

    void SplitPanelText(string text)
    {
        if (string.IsNullOrEmpty(text) || LblPolicy1 == null || LblPolicy2 == null)
            return;

        int breakpoint = 0;
        for (int i = content.Length / 2; i < (content.Length / 2) + 1000; ++i)
        {
            if (content[i] == '\n')
            {
                breakpoint = i;
                break;
            }
            else
            {
                continue;
            }
        }
        LblPolicy1.text = content.Substring(0, breakpoint);
        LblPolicy1.height = (int)LblPolicy1.printedSize.y;
        LblPolicy1.ResizeCollider();
        LblPolicy1.GetComponentInParent<UIPanel>().baseClipRegion = new Vector4(0, 0, 1400, LblPolicy1.height);

        LblPolicy2.text = content.Substring(breakpoint + 1);
        LblPolicy2.height = (int)LblPolicy2.printedSize.y;
        LblPolicy2.ResizeCollider();
        LblPolicy2.GetComponentInParent<UIPanel>().baseClipRegion = new Vector4(0, 0, 1400, LblPolicy2.height);
        LblPolicy2.cachedTransform.localPosition = new Vector3(0, -((LblPolicy1.height + LblPolicy2.height) / 2));

        policyPanel.GetComponent<UIScrollView>()?.ResetPosition();
        policiesMsgBox?.SetActive(true);
    }
}
