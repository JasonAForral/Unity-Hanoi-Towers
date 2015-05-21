using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    protected const int towerCount = 3;
    
    public static GameManager gameManager;

    public GameObject blockTemplate;
    public GameObject towerTemplate;
    public GameObject light;

    public GameObject[] panels;
    public Text[] texts;

    protected int difficulty;
    protected int level;
    
    protected Transform table;
    protected Transform[] towers = new Transform[towerCount];
    protected Stack<GameObject>[] towersContents = new Stack<GameObject>[towerCount];
    
    protected ClickState clickState;
    protected GameObject movingBlock;
    protected Transform movingBlockTransform;
    protected int fromTower;

    protected bool pickUp;
    protected bool paused;

    [SerializeField]
    protected float blockShapeFactor = 1f;

    protected void Awake ()
    {
        if (null == gameManager)
            gameManager = this;
        else if (this != gameManager)
            Destroy(this);

        ObtainLevel();

        InitializeDisplay();
        InitializeTowers();
    }

    protected void ObtainLevel ()
    {
        level = LevelTracker.level;
        difficulty = level + 1;
        Camera.main.transform.localPosition = Vector3.back * (difficulty * 4+5);
    }

    private void InitializeDisplay ()
    {
        Instantiate(light);

        texts[0].text = "Level: " + level;
        texts[1].text = "Layers: " + difficulty;

        panels[0].SetActive(true);
        panels[1].SetActive(true);

        Time.timeScale = 1f;
    }

    protected void InitializeTowers ()
    {
        table = new GameObject("Table").transform;
        for (int i = 0; i < towerCount; i++)
        {
            towers[i] = (Instantiate(towerTemplate, Vector3.right * (i - 1f) * (difficulty + 2f), Quaternion.identity) as GameObject).transform;

            towers[i].gameObject.name = "Tower " + i;

            towers[i].GetChild(0).localScale = new Vector3(difficulty + 1f, blockShapeFactor, difficulty + 1f);
            
            Transform peg = towers[i].GetChild(1);
            peg.localPosition = Vector3.up * (float)difficulty * 0.5f;
            peg.localScale = new Vector3(0.5f, (difficulty + 1f) * blockShapeFactor, 0.5f);
            
            towers[i].parent = table;
            towersContents[i] = new Stack<GameObject>();
            
        }


        for (int i = 0; i < difficulty; i++)
        {
            GameObject newBlock = Instantiate(blockTemplate, towers[0].transform.position + Vector3.up * i, Quaternion.identity) as GameObject;
            newBlock.transform.parent = towers[0];
            float blockScale = difficulty - towersContents[0].Count;
            newBlock.transform.localScale = new Vector3(blockScale, blockShapeFactor, blockScale);
            towersContents[0].Push(newBlock);
        }
    }

    protected int clicks;

    protected void incrementClicks ()
    {
        if (ClickState.Victory != clickState)
        {
            clicks++;
            updateClickDisplay();
        }
    }

    protected void updateClickDisplay ()
    {
        texts[2].text = "Clicks: " + clicks;
    }

    protected float timer;

    protected void Update ()
    {
        if (0f < Time.timeScale)
            timer += Time.deltaTime;
        texts[3].text = "Time: " + timer.ToString("F3");
        if (Input.GetButtonDown("Fire2"))
            if (ClickState.Victory != clickState)
                Pause();

        //
        if (Input.GetButtonDown("Fire3"))
            Debug.Log("prompt restart");
    }

    public void TowerClicked (Transform parentTower)
    {
        if (paused)
            return;

        int towerIndex = 0;
        for (int i = 0; i < towerCount; i++)
        {
            if (towers[i] == parentTower)
            {
                towerIndex = i;
                break;
            }
        }
        switch (clickState)
        {
        case ClickState.Pickup:
            if (0 < towersContents[towerIndex].Count)
                movingBlock = towersContents[towerIndex].Peek();
            else break;
            movingBlockTransform = movingBlock.transform;
            movingBlock.transform.Translate(Vector3.up);
            fromTower = towerIndex;
            clickState = ClickState.Drop;
            incrementClicks();
            break;
        case ClickState.Drop:
            // check if block can be dropped
            if (0 == towersContents[towerIndex].Count || movingBlockTransform.localScale.x <= towersContents[towerIndex].Peek().transform.localScale.x)

            // successful Drop
            {
                movingBlock = towersContents[fromTower].Pop();
                movingBlockTransform = movingBlock.transform;
                movingBlock.transform.SetParent(towers[towerIndex], false);
                movingBlock.transform.localPosition = Vector3.up * towersContents[towerIndex].Count;

                towersContents[towerIndex].Push(movingBlock);
                clickState = ClickState.Pickup;

                incrementClicks();
                CheckVictory();
                
            }

            break;
        case ClickState.Victory:
            LevelTracker.Victory();
            break;
        }
    }

    protected void CheckVictory ()
    {
        if (towersContents[2].Count == difficulty)
        {
            panels[2].SetActive(true);
            Time.timeScale = 0f;
            clickState = ClickState.Victory;
        }
    }

    void Pause ()
    {
        paused = !paused;
        if (paused)
        {
            panels[3].SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            panels[3].SetActive(false);
        }
    }

    protected enum ClickState
    {
        Pickup,
        Drop,
        Victory
    }

}
