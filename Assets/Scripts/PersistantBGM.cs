using UnityEngine;
using System.Collections;

public class PersistantBGM : MonoBehaviour
{
    public static PersistantBGM instance;
    public AudioSource audioSource;

    void Awake ()
    {

        if (null == instance)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
            DontDestroyOnLoad(gameObject);
        }
        else if (this != instance)
            Destroy(gameObject);
    }
}