using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisNet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("Tennis ball hit the net!");
        }
    }
}
