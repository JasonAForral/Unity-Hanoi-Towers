using UnityEngine;
using System.Collections;

public class TowerIdentifier : MonoBehaviour
{
    int towerIndex;

    public void SetIndex (int index)
    {
        towerIndex = index;
    }

    public int GetIndex ()
    {
        return towerIndex;
    }
}