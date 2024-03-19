using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*---------------------------------*
 * Code from plugin SpeechGenerationForNPC : https://assetstore.unity.com/packages/tools/audio/speech-generation-for-npc-212550
 *---------------------------------*/

public class Tts
{
    public string Phrase;

    public double Octave;

    public Dictionary<string, string[]> Options = new Dictionary<string, string[]>
    {
        { "Arabic",new string[] { "Farah", } },
        { "Chinese (Mandarin)",new string[] { "Daiyu", } },
        { "Danish",new string[] { "Emma", "Oscar", } },
        { "Dutch",new string[] { "Anke","Adriaan", } },
        { "English (Australian)",new string[] { "Mia","Grace","Jack", } },
        { "English (British)",new string[] { "Charlotte","Sophia","Elijah", } },
        { "English (Indian)",new string[] { "Advika","Onkar", } },
        { "English (New Zealand)",new string[] { "Ruby", } },
        { "English (South African)",new string[] { "Elna", } },
        { "English (US)",new string[] { "Mary","Linda","Patricia","Barbara","Susan","Paul","Michael","William","Thomas", } },
        { "English (Welsh)",new string[] { "Aeron", } },
        { "French",new string[] { "Capucine","Alix","Arnaud", } },
        { "French (Canadian)",new string[] { "Stephanie","Celine", } },
        { "German",new string[] { "Maria","Theresa","Felix", } },
        { "Hindi",new string[] { "Chhaya", } },
        { "Icelandic",new string[] { "Anna","Sigriour", } },
        { "Italian",new string[] { "Gabriella","Bella","Lorenzo", } },
        { "Japanese",new string[] { "Rika","Tanaka", } },
        { "Korean",new string[] { "Ji-Ho", } },
        { "Norwegian",new string[] { "Camilla", } },
        { "Polish",new string[] { "Katarzyna","Malgorzata","Piotr","Jan", } },
        { "Portuguese (Brazilian)",new string[] { "Tabata","Juliana","Pedro", } },
        { "Portuguese (European)",new string[] { "Pati","Adriano", } },
        { "Romanian",new string[] { "Alexandra", } },
        { "Russian",new string[] { "Inessa","Viktor", } },
        { "Spanish (European)",new string[] { "Francisca","Margarita","Mateo", } },
        { "Spanish (Mexican)",new string[] { "Leticia", } },
        { "Spanish (US)",new string[] { "Josefina","Rosa","Miguel", } },
        { "Swedish",new string[] { "Eva", } },
        { "Turkish",new string[] { "Mesut", } },
        { "Welsh",new string[] { "Angharad", } },
    };

    public string[] Langues = new string[]
        {
            "Arabic", "Chinese (Mandarin)", "Danish", "Dutch", "English (Australian)", "English (British)", "English (Indian)", "English (New Zealand)", "English (South African)", "English (US)", "English (Welsh)", "French", "French (Canadian)", "German", "Hindi", "Icelandic", "Italian", "Japanese", "Korean", "Norwegian", "Polish", "Portuguese (Brazilian)", "Portuguese (European)", "Romanian", "Russian", "Spanish (European)", "Spanish (Mexican)", "Spanish (US)", "Swedish", "Turkish", "Welsh",
        };

    public int Selected_O = 0;
    public int Selected_L = 0;
}
public class Synthese
{
    public string sentence;
    public string audio;

}
public class Authorization
{
    public string api_key = "kGOdv5yl.oqUd2gMibwhBVp5C6ed57Wpxvs2LUKOW";

}

public class TextToSpeech : MonoBehaviour
{
    private Tts _tts = new Tts();

    UnityWebRequest www;
    private Synthese info = null;
    string s_octave;

    private void Awake()
    {
        _tts.Octave = 0;//normal pitch
        _tts.Selected_L = 11;//french
        _tts.Selected_O = 1;//voice (Alix)

    }

    [SerializeField] private AudioSource speaker;
    [SerializeField] private GameObject spinner;
    private AudioClip son_pilou;
    private IEnumerator TextToAudio(string option, string phrase, double octave, string apikey)
    {
        if (apikey == null)
        {
            UnityEngine.Debug.LogError("Please contact the support");
            yield break;
        }

        if (phrase == null || phrase == "")
        {
            UnityEngine.Debug.LogError("Text is empty");
            yield break;
        }

        spinner.SetActive(true);
        yield return null;

        www = null;
        WWWForm form = new();
        form.AddField("sentence", phrase);
        s_octave = octave.ToString();
        form.AddField("octave", s_octave.Replace(',', '.'));
        //form.AddField("speed", "1");
        string lien = $"https://ariel-api.xandimmersion.com/tts/{option}";
        www = UnityWebRequest.Post(lien, form);
        www.SetRequestHeader("Authorization", "Api-Key " + apikey);

        www.SendWebRequest();
        while (!www.isDone)
        {
            continue;
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError("Error While Sending: " + www.error);
        }
        else
        {
            info = JsonUtility.FromJson<Synthese>(www.downloadHandler.text);
        }
        www.Dispose();

        string url = "https://rocky-taiga-14840.herokuapp.com/" + info.audio;
        using (UnityWebRequest www_audio = UnityWebRequestMultimedia.GetAudioClip(info.audio, AudioType.WAV))
        {
            www_audio.SetRequestHeader("x-requested-with", "http://127.0.0.1:8080");

            www_audio.SendWebRequest();


            while (!www_audio.isDone)
            {
                continue; //ajouter une barre de chargement
            }

            if (www_audio.result == UnityWebRequest.Result.ConnectionError)
            {
                UnityEngine.Debug.LogError(www_audio.error);
            }
            else
            {
                son_pilou = DownloadHandlerAudioClip.GetContent(www_audio);
                speaker.Stop();
                speaker.clip = son_pilou;
                speaker.Play();
            }
            www_audio.Dispose();
        }
        info = null;
        yield return null;
        spinner.SetActive(false);
    }

    private void Speak(string speech)
    {
        if (_tts.Phrase == speech && son_pilou != null)
        {
            speaker.Stop();
            speaker.clip = son_pilou;
            speaker.Play();
            return;
        }
        _tts.Phrase = speech;
        StartCoroutine(TextToAudio(_tts.Options[_tts.Langues[_tts.Selected_L].ToString()][_tts.Selected_O], _tts.Phrase, _tts.Octave, "kGOdv5yl.oqUd2gMibwhBVp5C6ed57Wpxvs2LUKOW"));
    }

    [SerializeField] private StagePanel stagePanel;
    public void ListenQuestion()
    {
        Speak(stagePanel.questionPanel.GetInfos());
    }
    public void ListenRemediation()
    {
        Speak(stagePanel.answerPanel.GetInfos());
    }
    public void ListenInfos()
    {
        Speak(stagePanel.informationPanel.GetInfos());
    }

}

