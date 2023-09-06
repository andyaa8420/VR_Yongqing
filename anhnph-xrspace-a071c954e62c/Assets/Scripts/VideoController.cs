using UnityEngine;
using UnityEngine.UI;

public class VideoController: MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer = null;
    public GameObject playBtn;
    public GameObject muteBtn;
    public GameObject volumeUpBtn;
    public GameObject volumeDownBtn;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (videoPlayer.isPlaying)
        {
            Sprite pauseImg = Resources.Load<Sprite>("Image/Pause");
            playBtn.GetComponent<Image>().sprite = pauseImg;
        }
        else
        {

            Sprite playImg = Resources.Load<Sprite>("Image/Play");
            playBtn.GetComponent<Image>().sprite = playImg;
        }


        if (videoPlayer.GetDirectAudioMute(0))
        {
            Sprite soundOffImg = Resources.Load<Sprite>("Image/SoundOff");
            muteBtn.GetComponent<Image>().sprite = soundOffImg;
        }
        else
        {
            Sprite soundOnImg = Resources.Load<Sprite>("Image/SoundOn");
            muteBtn.GetComponent<Image>().sprite = soundOnImg;
        }
    }

    public void PlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }

    public void MuteAudio()
    {
        if (videoPlayer.GetDirectAudioMute(0))
        {
            videoPlayer.SetDirectAudioMute(0, false);
        }
        else
        {
            videoPlayer.SetDirectAudioMute(0, true);
        }
    }

    public void VolumeUp()
    {
        float currentVolume = videoPlayer.GetDirectAudioVolume(0);
        if (currentVolume <= (float)0.9)
        {
            videoPlayer.SetDirectAudioVolume(0, currentVolume + (float)0.1);
        } else
        {
            videoPlayer.SetDirectAudioVolume(0, 1);
        }
    }

    public void VolumeDown()
    {
        float currentVolume = videoPlayer.GetDirectAudioVolume(0);
        if (currentVolume >= (float)0.1)
        {
            videoPlayer.SetDirectAudioVolume(0, currentVolume - (float)0.1);
        }
        else
        {
            videoPlayer.SetDirectAudioVolume(0, 0);
        }
    }

}
