using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JaimesUtilities.AStarManual
{
    [ExecuteInEditMode]
    public class PathfindingNode : MonoBehaviour
    {
        public PathfindingManager manager;

        // Pathfinding
        [HideInInspector] public List<PathfindingNode> links = new List<PathfindingNode>();
        [HideInInspector] public float gCost, hCost;
        [HideInInspector] public PathfindingNode parent;

        public bool isEnabled = true;
        
        public void ResetValues() {
            parent = null;
            gCost = 0;
            hCost = 0;
        }

        // Gizmos
        private float gizmosRadius = 0.25f;
        private Color[] gizmosColors = new Color[] {
            Color.yellow, Color.red, (Color.red + Color.blue) / 2f, Color.blue, Color.green,
        };
        [Range(0, 5)] public byte gizmosColorIndex;

        private Vector3 previousPosition;

        public void UpdatePosition() {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 10f, manager.settings.collideMask)) {
                transform.position = hitInfo.point + Vector3.up * manager.settings.nodeHeight;
            }
        }

        private void OnDrawGizmos() {
            if (previousPosition != transform.position) {
                UpdatePosition();
                previousPosition = transform.position;
            }

            if (manager.settings.showType == PathfindingSettings.ShowType.Nothing) return;
            Gizmos.color = isEnabled ? gizmosColors[gizmosColorIndex] : Color.black;
            Gizmos.DrawSphere(transform.position, gizmosRadius);

            if (manager.settings.showType == PathfindingSettings.ShowType.OnlyNodes) return;
            Gizmos.color = isEnabled ? Color.white : Color.black;
            links.ForEach(x => Gizmos.DrawLine(transform.position, x.transform.position));
        }

        private void OnDestroy() {
            links.ForEach(x => x.links.Remove(this));
        }
    }
}