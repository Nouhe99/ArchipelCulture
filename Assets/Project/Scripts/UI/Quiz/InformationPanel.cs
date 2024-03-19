using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InformationPanel : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image image;
    [Header("Explanation")]
    [SerializeField] private TMP_Text informationTitle;
    [SerializeField] private TMP_Text informationText;

    private InformationStage activeInformation;

    public void FillPanel(InformationStage info)
    {
        activeInformation = info;
        image.sprite = info.ResourcePicture;
        informationTitle.text = info.Title;
        informationText.text = info.Description;
    }

    public void DownloadResource()
    {
        Debug.Log("Download Resource");
        activeInformation.ResourceDownloaded = true;
        StartCoroutine(DownloadResourceCoroutine());
    }

    private IEnumerator DownloadResourceCoroutine()
    {
        UnityWebRequest request = new UnityWebRequest(activeInformation.ResourceURL, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath + Path.DirectorySeparatorChar + Path.GetFileName(activeInformation.ResourceURL));
        request.downloadHandler = new DownloadHandlerFile(path);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                request.Dispose();
                bool result = false;
                yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
                if (result)
                {
                    yield return StartCoroutine(DownloadResourceCoroutine());
                    yield break;
                }
            }
        }
        else
        {
            Debug.Log("File successfully downloaded and saved to : " + path);
        }
    }

    public void Close()
    {
        //if (activeInformation.ResourceDownloaded)
        //{
        //    activeInformation.State = StageState.CompletedSuccessfully;
        //}
        //else
        //{
        //    activeInformation.State = StageState.Completed;
        //}
        activeInformation.State = StageState.CompletedSuccessfully;
        ScenarioManager.Instance?.CloseStage();
    }


    /// <returns>String with all the infos for the TTS.</returns>
    public string GetInfos()
    {
        return informationText.text;
    }
}
