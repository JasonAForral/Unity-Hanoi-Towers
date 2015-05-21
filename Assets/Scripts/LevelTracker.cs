using UnityEngine;
using System.Collections;

public class LevelTracker : MonoBehaviour
{
    protected static int _level = 1;
    public static int level
    {
        get { return _level; }
    }

    static LevelTracker levelTracker;
    void Awake ()
    {
        if (null == levelTracker)
            levelTracker = this;
        else if (this != levelTracker)
            Destroy(this);

        DontDestroyOnLoad(this);
    }

    public static void Victory ()
    {
        _level++;
        Application.LoadLevel(0);
    }
}