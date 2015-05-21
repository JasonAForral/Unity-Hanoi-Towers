using UnityEngine;
using System.Collections;

public class PersistentLight : MonoBehaviour
{
    public static PersistentLight persistentlight;

        void Awake(){
            if (null == persistentlight)
                persistentlight = this;
            else if (this != persistentlight)
            Destroy(this);
            DontDestroyOnLoad  (this);
        }
}