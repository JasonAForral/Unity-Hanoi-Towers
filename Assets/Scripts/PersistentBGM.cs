using UnityEngine;
using System.Collections;

public class PersistentBGM : MonoBehaviour
{
    public static PersistentBGM persistentBGM;
    public AudioSource audioSource;

    void Awake ()
    {
        audioSource = GetComponent<AudioSource>();

        if (null == persistentBGM)
        {
            DontDestroyOnLoad(gameObject);
            persistentBGM = this;
            audioSource.Play();
            name = "Persistent BGM";
        }
        else if (this != persistentBGM)
            {
                audioSource.Stop();
                Destroy(gameObject);
            }
    }


}