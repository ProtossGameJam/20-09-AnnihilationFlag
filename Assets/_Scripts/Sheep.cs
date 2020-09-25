using System.Collections;
using UnityEngine;

namespace ProjectAF.Crowd
{
    public class Sheep : MonoBehaviour
    {
        [SerializeField]
        private float _movementSpeed = 1.0f;

        [SerializeField]
        private float _jumpForce = 1.0f;

        [SerializeField]
        private Rigidbody2D _rigidbody2D = null;

        private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

        private Coroutine _moveCoroutine;
        
        public void StartMoving(Vector3 destination)
        {
            if (!(_moveCoroutine is null))
                StopMoving();

            if (!CheckUsable(destination))
                return;

            _moveCoroutine = StartCoroutine(CoStartMove(destination));
        }

        public void StopMoving()
        {
            if (_moveCoroutine is null)
                return;

            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        public void Jump(Vector3 destination)
        {
            if (!CheckUsable(destination))
                return;

            var direction = destination - transform.position;
            var magnitude = direction.sqrMagnitude;
            var force = _jumpForce / magnitude;
            force = Mathf.Clamp(force, 0f, 12.5f);
            var vec = direction.normalized * force;
            _rigidbody2D.AddForce(vec, ForceMode2D.Impulse);
        }

        private IEnumerator CoStartMove(Vector3 destination)
        {
            while (gameObject.activeSelf)
            {
                var direction = destination - transform.position;
                var angle = Vector3.Angle(direction, Vector2.right);
                var magnitude = direction.magnitude;
                magnitude = Mathf.Max(magnitude, 4.0f);
                direction = angle <= 90.0f ? Vector3.right : Vector3.left;
                var delta = direction * _movementSpeed * Time.fixedDeltaTime / magnitude;

                transform.Translate(delta);
                yield return _waitForFixedUpdate;
            }
        }


        protected bool CheckUsable(Vector3 targetPosition)
        {
            var vec = transform.position - targetPosition;
            // 10^10
            return vec.magnitude <= 7f;
        }
    }
}