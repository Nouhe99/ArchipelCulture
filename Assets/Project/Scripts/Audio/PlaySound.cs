using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/AudioBank")]
public class PlaySound : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private AudioClip clickNormal;
    public AudioClip clickReturn;
    [SerializeField] private AudioClip buyPiece;
    public AudioClip bounce;
    public AudioClip placeItem;
    public AudioClip photoSaved;

    [Header("Aventure")]
    public AudioClip goodAnswer;
    public AudioClip badAnswer;
    public AudioClip celebration;
    public AudioClip fail;
    [SerializeField] private AudioClip eventKraken;

    [Header("Ambiant")]
    public AudioClip beach;
    public AudioClip underwater;
    public AudioClip boat;

    [Header("Music")]
    public AudioClip musicLogin;
    public AudioClip musicScenario;
    public AudioClip musicAdventure;
    public AudioClip musicKraken;

    [Header("Paper")]
    public AudioClip openPage;
    public AudioClip closePage;
    [SerializeField] private AudioClip openBook;
    [SerializeField] private AudioClip closeBook;

    [Header("Bip")]
    public AudioClip bipValid;
    public AudioClip bipUnvalid;
    [SerializeField] private AudioClip bipButton;

    #region Playing Audio Bank sound

    public void PressButtonNormal()
    {
        PlayAudio.Instance?.PlayOneShot(clickNormal);
    }
    public void PressButtonReturn()
    {
        PlayAudio.Instance?.PlayOneShot(clickReturn);
    }
    public void PressButtonBip()
    {
        PlayAudio.Instance?.PlayOneShot(bipButton);
    }
    public void BuySound()
    {
        PlayAudio.Instance?.PlayOneShot(buyPiece);
    }
    public void KrakenSound()
    {
        PlayAudio.Instance?.PlayOneShot(eventKraken);
    }
    public void Book(bool open)
    {
        if (open)
            PlayAudio.Instance?.PlayOneShot(openBook);
        else
            PlayAudio.Instance?.PlayOneShot(closeBook);
    }
    #endregion


}
