using ProjectAF.Crowd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAF
{
    public class Destination : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                CrowdController.Instance.OnEnterGoal(collision.gameObject);
            }
        }
    }
}
