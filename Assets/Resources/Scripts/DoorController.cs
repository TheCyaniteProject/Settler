using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator animator;
    public bool isOpen = false;
    public bool isLocked = false;

    // Start is called before the first frame update
    void Update()
    {
        animator.SetBool("Open", isOpen);
    }

    public void Toggle()
    {
        if (!isLocked)
            isOpen = !isOpen;
    }
}
