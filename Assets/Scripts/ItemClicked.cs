using UnityEngine;
using System.Collections;

public class ItemClicked : MonoBehaviour
{
    void OnMouseDown ()
    {
        GameManager.gameManager.TowerClicked(transform.parent.GetComponent<TowerIdentifier>().GetIndex());
    }
}