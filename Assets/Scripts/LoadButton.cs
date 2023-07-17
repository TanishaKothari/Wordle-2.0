using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public void LoadLetter(int length)
    {
        GameManager.Instance.LoadLetter(length);
    }
}
