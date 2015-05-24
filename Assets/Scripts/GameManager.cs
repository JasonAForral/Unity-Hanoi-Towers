using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    protected const int towerCount = 3;

    public static GameManager gameManager;
    static int level =1;

    public GameObject blockTemplate;
    public GameObject towerTemplate;
    public GameObject lightPrefab;

    public GameObject[] panels;
    public Text[] texts;

    protected int difficulty;

    protected Transform table;
    protected Transform[] towers = new Transform[towerCount];
    protected Stack<GameObject>[] towersContents = new Stack<GameObject>[towerCount];

    protected AudioSource audioSource;
    protected Transform mainCamera;
    [SerializeField]
    protected AudioClip[] sounds;

    protected InputState inputState;
    protected GameObject movingBlock;
    protected Transform movingBlockTransform;
    protected int fromTower;

    protected bool pickUp;
    protected bool paused;

    protected int moves;

    protected int min;
    protected int sec;
    protected float mili;

    protected bool toggleMusic = true;
    protected bool toggleSounds = true;

    //Resolution screenRes;
    //Vector2 screenSize;

    //public Text debug;

    //ScreenOrientation orientation;
    protected enum InputState
    {
        Pickup,
        Drop,
        Victory
    }

    protected void Awake ()
    {
        if (null == gameManager)
            gameManager = this;
        else if (this != gameManager)
            Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main.transform;

        ObtainLevel();

        InitializeDisplay();
        InitializeTowers();
    }

    protected void ObtainLevel ()
    {
        difficulty = level + 1;
        MoveCamera();

    }

    void MoveCamera ()
    {
        //float aspecter = (float)Screen.height / (float)Screen.width * 2f;
        //if (aspecter < 1f)
        //    aspecter = 1f;
        mainCamera.localPosition = Vector3.back * (level * 2f + 4f);
        //orientation = Screen.orientation;

        //debug.text = "Resolution: " + Screen.width + " x " + Screen.height;
        //screenSize.Set(Screen.width, Screen.height);
        //Resolution screenRes = Screen.resolutions[0];
    }

    

    public void ToggleSounds ()
    {
        toggleSounds = !toggleSounds;
        audioSource.mute = !toggleSounds;
    }

    private void InitializeDisplay ()
    {
        //Instantiate(lightPrefab);

        texts[0].text = "Level: " + level;
        texts[1].text = "Layers: " + difficulty;

        panels[0].SetActive(true);
        panels[1].SetActive(true);

        Time.timeScale = 1f;
    }

    protected void InitializeTowers ()
    {
        table = new GameObject("Table").transform;
        for (int i = 0; i < towerCount; i++) {
            towers[i] = (Instantiate(towerTemplate, Vector3.right * (i - 1f) * (difficulty + 2f), Quaternion.identity) as GameObject).transform;

            towers[i].gameObject.name = "Tower " + i;

            towers[i].GetChild(0).localScale = new Vector3(difficulty + 1f, 1f, difficulty + 1f);

            Transform peg = towers[i].GetChild(1);
            peg.localPosition = Vector3.up * (float)difficulty*0.5f;
            peg.localScale = new Vector3(0.5f, difficulty+1f , 0.5f);

            Transform hitBox = towers[i].GetChild(2);
            hitBox.localScale = new Vector3(difficulty, (difficulty + 1f) * 2, difficulty);

            towers[i].parent = table;
            towersContents[i] = new Stack<GameObject>();

        }


        for (int i = 0; i < difficulty; i++) {
            GameObject newBlock = Instantiate(blockTemplate, towers[0].transform.position + Vector3.up * i, Quaternion.identity) as GameObject;
            newBlock.transform.parent = towers[0];
            float blockScale = difficulty - towersContents[0].Count;
            newBlock.transform.localScale = new Vector3(blockScale, 1f, blockScale);
            towersContents[0].Push(newBlock);
        }
    }

    protected void incrementMoves ()
    {
        if (InputState.Victory != inputState) {
            moves++;
            updateClickDisplay();
        }
    }

    protected void updateClickDisplay ()
    {
        texts[2].text = "Moves: " + moves;
    }

    protected void Update ()
    {

            MoveCamera();

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    panels[2].SetActive(true);
        //    Time.timeScale = 0f;
        //    inputState = InputState.Victory;
        //    audioSource.PlayOneShot(sounds[3]);
        //}


        if (0f < Time.timeScale) {
            min = (int)Time.timeSinceLevelLoad / 60;
            sec = (int)Time.timeSinceLevelLoad % 60 / 10;
            mili = Time.timeSinceLevelLoad % 10;
        }
        texts[3].text = min.ToString("D2") + ":" + sec.ToString("D1") + mili.ToString("F3");

        if (Input.GetButtonDown("Fire2") || Input.GetButtonDown("Cancel"))
            if (InputState.Victory != inputState)
                Pause();

        if (Input.GetButtonDown("Fire3"))
            Debug.Log("prompt restart");

        //if (Input.GetButtonDown("Fire1"))
        //    if (InputState.Victory == inputState)
        //        LevelTracker.Victory();
    }

    public void TowerClicked (Transform tower)
    {
        if (paused)
            return;
        int towerIndex = 4;
        for (int i = 0; i < towerCount; i++) {
            if (tower == towers[i]) {
                towerIndex = i;
                break;
            }
        }

        if (towerIndex >= towerCount) {
            Debug.Log("need new handle");
            return;
        }


        switch (inputState) {
            case InputState.Pickup:

                if (0 < towersContents[towerIndex].Count)
                    movingBlock = towersContents[towerIndex].Peek();
                else
                    break;
                audioSource.PlayOneShot(sounds[0]);
                movingBlockTransform = movingBlock.transform;
                movingBlock.transform.Translate(Vector3.up);
                fromTower = towerIndex;
                inputState = InputState.Drop;
                incrementMoves();
                break;
            case InputState.Drop:


                // check if block can be dropped
                if (0 == towersContents[towerIndex].Count || movingBlockTransform.localScale.x <= towersContents[towerIndex].Peek().transform.localScale.x)

                // successful Drop
            {
                    audioSource.PlayOneShot(sounds[1]);

                    movingBlock = towersContents[fromTower].Pop();
                    movingBlockTransform = movingBlock.transform;
                    movingBlock.transform.SetParent(towers[towerIndex], false);
                    movingBlock.transform.localPosition = Vector3.up * towersContents[towerIndex].Count;

                    towersContents[towerIndex].Push(movingBlock);
                    inputState = InputState.Pickup;

                    incrementMoves();
                    CheckVictory();

                } else {
                    audioSource.PlayOneShot(sounds[2]);
                }

                break;
            case InputState.Victory:
                //LevelTracker.Victory();
                NextLevel();
                break;
        }
    }

    public void NextLevel ()
    {
        level++;
        Application.LoadLevel(Application.loadedLevel);
    }

    

    protected void CheckVictory ()
    {
        if (towersContents[2].Count == difficulty) {
            panels[2].SetActive(true);
            Time.timeScale = 0f;
            inputState = InputState.Victory;
            audioSource.PlayOneShot(sounds[3]);

        }
    }

    void Pause ()
    {
        paused = !paused;
        if (paused) {
            panels[3].SetActive(true);
            Time.timeScale = 0f;
            PersistentBGM.persistentBGM.audioSource.Pause();
        } else {
            Time.timeScale = 1f;
            panels[3].SetActive(false);
            PersistentBGM.persistentBGM.audioSource.UnPause();
        }
    }

    public void ToggleMusic ()
    {
        toggleMusic = !toggleMusic;

        if (toggleMusic) {
            PersistentBGM.persistentBGM.audioSource.Play();
        } else {
            PersistentBGM.persistentBGM.audioSource.Stop();
        }
    }


}
