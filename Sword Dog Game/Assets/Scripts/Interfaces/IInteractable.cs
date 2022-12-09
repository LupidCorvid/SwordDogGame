using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void interact(GameObject user);

    public GameObject gameObject
    {
        get;
    }

    public bool inRange
    {
        get;
        set;
    }

    public void showPrompt(GameObject prompt);

    public void hidePrompt(GameObject prompt);
}
