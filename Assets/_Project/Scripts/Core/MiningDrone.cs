using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceMining
{
    public class MiningDrone : MonoBehaviour
    {
        public float speed = 3f;

        private OreBlock _target;
        private List<Vector3> _waypoints;
        private int _waypointIndex;
        private GridManager _gridManager;
        private GridVisualizer _gridVisualizer;
        private Animator _animator;
        private string _attackState;
        private float _attackDuration;
        private bool _arrived;
        private float _wigglePhase;

        public void Initialize(OreBlock target, List<Vector3> waypoints, GridManager gm, GridVisualizer gv,
                               Animator anim, string attackState, float attackDuration)
        {
            _target = target;
            _waypoints = waypoints;
            _waypointIndex = 0;
            _gridManager = gm;
            _gridVisualizer = gv;
            _animator = anim;
            _attackState = attackState;
            _attackDuration = attackDuration;
            _wigglePhase = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            if (_arrived || _waypoints == null || _waypoints.Count == 0) return;

            Vector3 dest = _waypoints[_waypointIndex];
            Vector3 toWaypoint = dest - transform.position;
            float distToDest = toWaypoint.magnitude;

            // Perlin noise wander perpendicular to path, fades to zero in the last 0.5 units
            Vector3 moveTarget = dest;
            if (distToDest > 0.01f)
            {
                Vector3 perp = new Vector3(-toWaypoint.y, toWaypoint.x, 0f).normalized;
                float noise = (Mathf.PerlinNoise(Time.time * 2f + _wigglePhase, _wigglePhase) - 0.5f) * 2f;
                float fade  = Mathf.Clamp01(distToDest / 0.5f);
                moveTarget += perp * noise * 0.5f * fade;
            }

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, speed * Time.deltaTime);

            Vector3 dir = moveTarget - transform.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                Quaternion target = Quaternion.Euler(0f, 0f, angle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 360f * Time.deltaTime);
            }

            if (distToDest < 0.3f)
            {
                _waypointIndex++;
                if (_waypointIndex >= _waypoints.Count)
                {
                    _arrived = true;
                    StartCoroutine(Arrive());
                }
            }
        }

        private IEnumerator Arrive()
        {
            _animator?.Play(_attackState);
            yield return new WaitForSeconds(_attackDuration);
            _gridManager.OnBlockMined(_target);
            _gridVisualizer.VanishBlock(_target);
            Destroy(gameObject);
        }
    }
}
