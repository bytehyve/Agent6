using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JaimesUtilities
{
    public static class JaimesUtilities_Main
    {
        public static void SetSingleton<T>(this T source, ref T instance, GameObject gameObject = null) {
            if (gameObject != null && instance != null && !instance.Equals(source)) {
                MonoBehaviour.Destroy(gameObject);
                return;
            }
        
            instance = source;
        }
        public static T Clone<T>(this T source) {
            var serialized = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<T>(serialized);
        }
        public static void Clone<T>(this T source, ref T instance) {
            var serialized = JsonUtility.ToJson(source);
            instance = JsonUtility.FromJson<T>(serialized);
        }
        public static List<T> Shuffle<T>(this List<T> source, System.Random prng) {
            List<T> newList = new List<T>();
            while (source.Count > 0) {
                int randomIndex = prng.Next(0, source.Count);
                newList.Add(source[randomIndex]);
                source.RemoveAt(randomIndex);
            }

            return newList;
        }
        public static List<T> Shuffle<T>(this List<T> source) {
            List<T> newList = new List<T>();
            while (source.Count > 0) {
                int randomIndex = Random.Range(0, source.Count);
                newList.Add(source[randomIndex]);
                source.RemoveAt(randomIndex);
            }

            return newList;
        }


        public static void SwapObjects<T>(ref T objectA, ref T objectB) {
            T savedObjectA = objectA;
            objectA = objectB;
            objectB = savedObjectA;
        }

        public static bool MouseToPointRay(Camera camera, out RaycastHit rayHit, float maxDistance = float.MaxValue, LayerMask hitMask = default) {
            return (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition.Clamp(new Vector3(Screen.currentResolution.width, Screen.currentResolution.height))), out rayHit, maxDistance, hitMask));
        }
        public static bool MouseToPointRay(Camera camera, float maxDistance = float.MaxValue, LayerMask hitMask = default) {
            return (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition.Clamp(new Vector3(Screen.currentResolution.width, Screen.currentResolution.height))), maxDistance, hitMask));
        }
        public static bool MouseToPointRay(Camera camera, LayerMask hitMask = default) {
            return (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition.Clamp(new Vector3(Screen.currentResolution.width, Screen.currentResolution.height))), float.MaxValue, hitMask));
        }
        public static bool MouseToPointRay(Camera camera, out RaycastHit rayHit, LayerMask hitMask = default) {
            return (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition.Clamp(new Vector3(Screen.currentResolution.width, Screen.currentResolution.height))), out rayHit, float.MaxValue, hitMask));
        }

        public static Vector2 PointOnEdgeOfSquare(Vector2 point, Vector2 direction, Vector2 startEdge, Vector2 endEdge, float precision = 1f) {
            while (point.x > startEdge.x && point.x < endEdge.x && point.y > startEdge.y && point.y < endEdge.y) {
                point += direction * precision; 
            }

            return point;
        }

        // RigidBody Extensions
        public static float TopSpeed(this Rigidbody rb, float force, bool countMass = false) {
            if (force <= 0) return 0;
            return ((force / rb.drag) - Time.fixedDeltaTime * force) / (countMass ? rb.mass : 1f);
        }
    }
    public static class MathFExtensions
    {
        public static float Clamp(this float source, float minValue = 0, float maxValue = 1f) {
            return Mathf.Clamp(source, minValue, maxValue);
        }
        public static int Clamp(this int source, int minValue = 0, int maxValue = 1) {
            return Mathf.Clamp(source, minValue, maxValue);
        }
        public static float Max(this float source, float maxValue = 1f) {
            return Mathf.Max(source, maxValue);
        }
        public static int Max(this int source, int maxValue = 1) {
            return Mathf.Max(source, maxValue);
        }
        public static float Min(this float source, float minValue = 1f) {
            return Mathf.Min(source, minValue);
        }
        public static int Min(this int source, int minValue = 1) {
            return Mathf.Max(source, minValue);
        }
        public static float Sign(this float source) {
            if (source < 0) source = -1;
            else if (source > 0) source = 1;
            return source;
        }
        public static float Abs(this float source) {
            return Mathf.Abs(source);
        }
    }

    public static class VectorExtensions
    {
        // Clamp
        public static Vector3 Clamp(this Vector3 source, Vector3 maxValue, Vector3 minValue = default) {
            return new Vector3(source.x.Clamp(minValue.x, maxValue.x), source.y.Clamp(minValue.y, maxValue.y), source.z.Clamp(minValue.z, maxValue.z));
        }
        public static Vector3Int Clamp(this Vector3Int source, Vector3Int maxValue, Vector3Int minValue = default) {
            return new Vector3Int(source.x.Clamp(minValue.x, maxValue.x), source.y.Clamp(minValue.y, maxValue.y), source.z.Clamp(minValue.z, maxValue.z));
        }
        public static Vector2 Clamp(this Vector2 source, Vector2 maxValue, Vector2 minValue = default) {
            return new Vector2(source.x.Clamp(minValue.x, maxValue.x), source.y.Clamp(minValue.y, maxValue.y));
        }
        public static Vector2Int Clamp(this Vector2Int source, Vector2Int maxValue, Vector2Int minValue = default) {
            return new Vector2Int(source.x.Clamp(minValue.x, maxValue.x), source.y.Clamp(minValue.y, maxValue.y));
        }
        
        // Max
        public static Vector3 Max(this Vector3 source, Vector3 maxValue) {
            return new Vector3(source.x.Max(maxValue.x), source.y.Max(maxValue.y), source.z.Max(maxValue.z));
        }
        public static Vector3Int Max(this Vector3Int source, Vector3Int maxValue) {
            return new Vector3Int(source.x.Max(maxValue.x), source.y.Max(maxValue.y), source.z.Max(maxValue.z));
        }
        public static Vector2 Max(this Vector2 source, Vector2 maxValue) {
            return new Vector2(source.x.Max(maxValue.x), source.y.Max(maxValue.y));
        }
        public static Vector2Int Max(this Vector2Int source, Vector2Int maxValue) {
            return new Vector2Int(source.x.Max(maxValue.x), source.y.Max(maxValue.y));
        }

        // Min
        public static Vector3 Min(this Vector3 source, Vector3 minValue) {
            return new Vector3(source.x.Min(minValue.x), source.y.Min(minValue.y), source.z.Min(minValue.z));
        }
        public static Vector3Int Min(this Vector3Int source, Vector3Int minValue) {
            return new Vector3Int(source.x.Min(minValue.x), source.y.Min(minValue.y), source.z.Min(minValue.z));
        }
        public static Vector2 Min(this Vector2 source, Vector2 minValue) {
            return new Vector2(source.x.Min(minValue.x), source.y.Min(minValue.y));
        }
        public static Vector2Int Min(this Vector2Int source, Vector2Int minValue) {
            return new Vector2Int(source.x.Min(minValue.x), source.y.Min(minValue.y));
        }

        // Abs
        public static Vector3 Abs(this Vector3 source) {
            return new Vector3(Mathf.Abs(source.x), Mathf.Abs(source.y), Mathf.Abs(source.z));
        }
        public static Vector3Int Abs(this Vector3Int source) {
            return ((Vector3)source).Abs().ToInt();
        }
        public static Vector2 Abs(this Vector2 source) {
            return ((Vector3)source).Abs();
        }
        public static Vector2Int Abs(this Vector2Int source) {
            return ((Vector2)source).Abs().ToInt();
        }

        // ToInt
        public static Vector3Int ToInt(this Vector3 source) {
            return new Vector3Int(Mathf.RoundToInt(source.x), Mathf.RoundToInt(source.y), Mathf.RoundToInt(source.z));
        }
        public static Vector2Int ToInt(this Vector2 source) {
            return new Vector2Int(Mathf.RoundToInt(source.x), Mathf.RoundToInt(source.y));
        }

        // SignedNormalize
        public static Vector2 SignedNormalize(this Vector2 source) {
            Vector2 absSource = source.Abs();

            if (absSource.x > absSource.y) {
                source.y = 0;
                source.x = source.x.Sign();
            }
            else {
                source.x = 0;
                source.y = source.y.Sign();
            }

            return source;
        }
        public static Vector2Int SignedNormalize(this Vector2Int source) {
            return ((Vector2)source).SignedNormalize().ToInt();
        }

        // Horizontal
        public static Vector3 Horizontal(this Vector3 source) {
            source.y = 0;
            return source;
        }

        // Rotation
        public static Vector3 FixEuler(this Vector3 source) {
            if (source.x > 180) source.x -= 360;
            if (source.y > 180) source.y -= 360;
            if (source.z > 180) source.z -= 360;
            return source;
        }
        public static Vector2 Rotate(this Vector2 source, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = source.x;
            float ty = source.y;
            source.x = (cos * tx) - (sin * ty);
            source.y = (sin * tx) + (cos * ty);
            return source;
        }
        public static Vector2Int Rotate(this Vector2Int source, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = source.x;
            float ty = source.y;
            source.x = Mathf.RoundToInt((cos * tx) - (sin * ty));
            source.y = Mathf.RoundToInt((sin * tx) + (cos * ty));
            return source;
        }

        // Angle
        public static float AngleAxis(this Vector3 source, Vector3 forward, Vector3 up) {
            Vector3 right = Vector3.Cross(up, forward).normalized;
            forward = Vector3.Cross(right, up).normalized;
            return Mathf.Atan2(Vector3.Dot(source, right), Vector3.Dot(source, forward)) * Mathf.Rad2Deg;
        }
    }
}
