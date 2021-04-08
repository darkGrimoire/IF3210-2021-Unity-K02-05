using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeChange : MonoBehaviour
{
    // public AudioMixer mixer;
    public Slider musicSlider;

    private float musicVolume = 0.5f;
    // Start is called before the first frame update
    public void Start()
    {
        if (PlayerPrefs.HasKey("volMusic")) {
            musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("volMusic");
        }
    }

    public void Update()
    {
        PlayerPrefs.SetFloat("volMusic", GameObject.Find ("VoiceControl").GetComponent <Slider> ().value);
    }

}
