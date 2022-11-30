using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace JaimesUtilities.AStarManual
{
    public class PathfindingManager : MonoBehaviour
    {
        public PathfindingSettings settings;
        public Pathfinder pathfinder;
        protected Transform nodesContainer => transform.Find("Nodes Container");

        public Path path;

        private void Awake() {
            OnValueUpdated();
        }
        private void OnDrawGizmos() {
            if (settings.realtimeUpdate) OnValueUpdated();

            if (settings.viewAreaBounds) {
                Vector3 areaSize = settings.areaSize.Max(Vector3.one * settings.avrNodeDistance);
                Vector3 centerPos = transform.position + (settings.centerArea ? Vector3.zero : settings.areaSize / 2f);

                Gizmos.color = new Color(1f, 1f, 1f, 0.05f);
                Gizmos.DrawCube(centerPos, areaSize);
                Gizmos.color = new Color(1f, 1f, 1f, 0.9f);
                Gizmos.DrawWireCube(centerPos, areaSize);
            }
        }

        public void OnValueUpdated() {
            if (pathfinder == null) pathfinder = new Pathfinder(settings);
            if (nodesContainer == null) CreateNodeContainer();

            PathfindingNode[] nodes = nodesContainer.GetComponentsInChildren<PathfindingNode>().ToArray();
            foreach (PathfindingNode node in nodes) {
                node.manager = this;
                node.UpdatePosition();
            }

            pathfinder.Setup(nodes);
        }

        public bool GenerateNodesInArea(Vector3 areaSize, float avrNodeDistance, bool centerArea = true, bool bakeAfterFinish = false) {
            if (avrNodeDistance <= 0) {
                Debug.Log($"Error: Can't generate a grid with a node distance of < 0.");
                return false;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && !EditorUtility.DisplayDialog("Generate nodes", "This will overwrite and delete all existing nodes.", "Proceed", "Cancel")) return false;
#endif
            ClearNodes(false);

            areaSize = areaSize.Max(Vector3.one);

            Vector3 increment = new Vector3(
                areaSize.x / Mathf.Ceil(areaSize.x / avrNodeDistance),
                areaSize.y / Mathf.Ceil(areaSize.y / avrNodeDistance),
                areaSize.z / Mathf.Ceil(areaSize.z / avrNodeDistance)
            );

            Vector3 posOffset = transform.position + (centerArea ? -areaSize / 2f : Vector3.zero);
            LayerMask collideMask = settings.collideMask;
            float maxAngle = settings.maxNodeAngle;

            HashSet<Vector3> closedSet = new HashSet<Vector3>();

            for (float x = 0; x <= areaSize.x; x += increment.x) {
                for (float y = 0; y <= areaSize.y; y += increment.y) {
                    for (float z = 0; z <= areaSize.z; z += increment.z) {
                        Vector3 pos = new Vector3(x, y, z) + posOffset; 

                        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hitInfo, increment.y * 1.5f, collideMask)) {
                            if (Vector3.Angle(hitInfo.normal, Vector3.up) > maxAngle) continue;

                            if (Physics.CheckBox(pos, increment / 3f, Quaternion.identity, collideMask)) continue;
                            
                            pos = hitInfo.point + Vector3.up * increment.y;
                            if (closedSet.Contains(pos)) continue;

                            GameObject newNode = CreateNode();
                            newNode.transform.position = pos;
                            closedSet.Add(pos);
                        }
                    }
                }
            }

            Debug.Log($"Generated a total of {nodesContainer.childCount} nodes.");

            OnValueUpdated();
            if (bakeAfterFinish) pathfinder.Bake(); 

            return true;
        }
        public void ClearNodes(bool displayMessage = true) {
            if (nodesContainer == null) {
                if (displayMessage) Debug.Log("Error: No nodes container found.");
                return;
            }
            if (nodesContainer.childCount == 0) {
                if (displayMessage) Debug.Log("Error: No nodes found!");
                return;
            }

#if UNITY_EDITOR
            if (displayMessage && !Application.isPlaying && !EditorUtility.DisplayDialog("Clear nodes", "This will delete all existing nodes.", "Proceed", "Cancel")) return;
#endif

            for (int i = nodesContainer.childCount - 1; i >= 0; i--) {
                DestroyImmediate(nodesContainer.GetChild(0).gameObject);
            }
        }
        public GameObject CreateNode() {
            if (nodesContainer == null) CreateNodeContainer();
            
            GameObject newNode = new GameObject("Node");
            newNode.transform.parent = nodesContainer;
            newNode.transform.position = Vector3.zero;
            newNode.AddComponent<PathfindingNode>();
            newNode.GetComponent<PathfindingNode>().manager = this;
            return newNode;
        }
        public void CreateNodeContainer() {
            if (this.nodesContainer != null) return;

            GameObject nodesContainer = new GameObject("Nodes Container");
            nodesContainer.transform.parent = transform;
            nodesContainer.transform.position = Vector3.zero;
        }
    }
}