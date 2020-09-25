using ProjectAF.Crowd;
using UnityEngine;

namespace ProjectAF
{
    public abstract class FlagBase : MonoBehaviour
    {
        protected abstract void OnMouseDown();

        protected abstract void OnMouseUp();
    }
}