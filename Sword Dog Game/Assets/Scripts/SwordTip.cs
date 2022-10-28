using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTip : MonoBehaviour
{
    public SwordFollow sword;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            // do damage here
        }
        else if (other.gameObject.tag == "Ground")
        {
            Debug.Log("ground");
            sword.Freeze();
        }
    }
}
