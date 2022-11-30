using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JaimesUtilities.AStarManual
{
    // TODO: Clean up this code!
    public class PathfindingAgent : MonoBehaviour 
    {
        public enum Status
        {
            Active,
            Sleep
        }

        public PathfindingManager manager;
        [Range(0, 2f)]
        public float refreshRate = 0.5f;
        public float minNodeDistance = 0.5f;
        [Range(0.1f, 60f)]
        public float pathingSnappiness = 5f;

        private float currentRefreshTime;
        private Vector3 targetPosition;
        private bool targetPositionSet;
        protected Pathfinder pathfinder => manager.pathfinder;
        public Path path;

        private Vector3 targetDirection;
        private Vector3 savedTargetDirection;
        private Status status;
        public Status CurrentStatus => status;

        public float LengthInPath => path != null ? path.Length : 0f;

        private void Awake() {
            currentRefreshTime = Time.time + Random.Range(0, refreshRate);
            targetPosition = transform.position;
        }

        private void FixedUpdate() {
            if (status == Status.Sleep) {
                if (targetDirection != Vector3.zero) savedTargetDirection = targetDirection;
                targetDirection = Vector3.zero;
                return;
            }

            if (savedTargetDirection != Vector3.zero) {
                targetDirection = savedTargetDirection;
                savedTargetDirection = Vector3.zero;
            }

            if (status != Status.Sleep && currentRefreshTime < Time.time) {
                currentRefreshTime = Time.time + refreshRate;

                if (targetPositionSet) {
                    path = pathfinder.FindPath(transform.position, targetPosition);
                    if (path.status == Path.Status.Invalid) path = null;
                }

                targetPositionSet = false;
            }

            if (path != null) {
                if (path.corners.Count == 0) path = null;
                else {
                    float sqrCurDist = (transform.position - path.corners[0]).Horizontal().sqrMagnitude;
                    float sqrMinDist = Mathf.Sqrt(minNodeDistance);

                    float sqrCurDistNext = path.corners.Count > 1 ? (transform.position - path.corners[1]).Horizontal().sqrMagnitude : float.MaxValue;

                    if (sqrCurDist < sqrMinDist || sqrCurDistNext < sqrCurDist) {
                        path.corners.RemoveAt(0);
                    }
                    else {
                        targetDirection = Vector3.Lerp(targetDirection, (path.corners[0] - transform.position).Horizontal().normalized, pathingSnappiness * Time.deltaTime);
                    }
                }
            }
            else targetDirection = Vector3.zero;
        }

        private void OnValidate() {
            minNodeDistance = Mathf.Max(0.01f, minNodeDistance);
        }

        public void SetTargetPosition(Vector3 position) {
            targetPositionSet = true;
            targetPosition = position;
        }
        public Vector3 GetTargetDirection() {
            return targetDirection;
        }
        public void SetStatus(Status status) {
            this.status = status;
        }
    }
}