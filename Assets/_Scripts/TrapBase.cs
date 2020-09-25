using UnityEngine;

namespace ProjectAF
{
    public class TrapBase : MonoBehaviour
    {
        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                collision.gameObject.SetActive(false);
        }
    }
}
