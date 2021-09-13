using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEngine.EventSystems;


public static class CoroutineExtension
{
    public static IEnumerator InvokeAfter(Action action, float seconds)
    {
        var wait = new WaitForEndOfFrame();

        while ((seconds -= Time.deltaTime) > 0f) yield return wait;

        action.Invoke();
    }
}

public static class TimespanExtension
{
    public static string ToStringFormatted(this TimeSpan ts)
    {
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}

public static class FloatExt
{
    static public string ToStringFormatted(this float n)
    {
        return
            (n <= 9_999_999f)
            ? n.ToString("#,###,##0")
            : n.ToString("0.00e##0");
    }

    static public string BeautifulFormat(float number)
    {
        float abs = Mathf.Abs(number);

        string str =
            (abs > 9_999_999f)
            ? abs.ToString("e2")
            : abs.ToString("N0", CultureInfo.InvariantCulture);


        return str ;
    }

    static public string BeautifulFormatSigned(float number)
    {
        return ((number < 0)? '-' : '+') + BeautifulFormat(number);
    }
}

public static class FieldInfoExt
{
    static public bool HasAttribute<T>(this FieldInfo fieldInfo, out T attribute)
        where T : Attribute
    {
        attribute = (T) fieldInfo.GetCustomAttribute(typeof(T), true);
        
        return attribute != null;
    }
}

public static class IEnumerableExt
{
    public static string Print(this IEnumerable items)
    {
        return string.Join(", ", items);
    }
}

static public class EventTriggerExtensions
{
    static public void AddTrigger(this EventTrigger eventTrigger , EventTriggerType triggerType, Action<BaseEventData> action)
    {

        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = triggerType;

        entry.callback.AddListener((args)=>{ action(args); });

        eventTrigger.triggers.Add(entry);
    }
}

static public class GameObjectExtensions
{
    static public T AddGetComponent<T>(this GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();

        if (component == null) component = go.AddComponent<T>();

        return component;
    }
}


static public class ListExtensions
{
    static public T GetRandom<T>(this List<T> list)
    {
        int id = UnityEngine.Random.Range(0, list.Count);
        return list[id];
    }

    static public T ExtractRandom<T>(this List<T> list)
    {
        int id = UnityEngine.Random.Range(0, list.Count);
        T r = list[id];
        list.RemoveAt(id);
        return r;
    }

    static public T ExtractFirst<T>(this List<T> list)
    {
        T r = list[0];
        list.RemoveAt(0);
        return r;
    }

    static public T ExtractLast<T>(this List<T> list)
    {
        int id = list.Count-1;
        T r = list[id];
        list.RemoveAt(id);
        return r;
    }
}

static public class PhysicMethods
{
    static public bool CastRayThroughRandomViewportPoint(LayerMask layerMask, Predicate<RaycastHit> filterHitInfo, out RaycastHit hitInfo)
    {
        Vector3 location = Vector3.zero;

        Vector3 viewportPoint = new Vector3(UnityEngine.Random.Range(.2f, .8f),
                                            UnityEngine.Random.Range(.2f, .8f),
                                            0);

        Ray ray = Camera.main.ViewportPointToRay(viewportPoint);


        return Physics.Raycast(ray, out hitInfo, 500, layerMask) && filterHitInfo.Invoke(hitInfo);
    }

}

static public class Utils
{
    static public Vector3 GetProgScale(float val, float max)
    {
        float v = val/max;
        return new Vector3(v, v, v);
    }


    static public Camera camera = Camera.main;

    static public Vector3 GetMouseWorldPosition(float z=0f)
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, camera);
        vec.z = z;
        return vec;
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera) {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    static public float AngleToFaceTarget(Vector3 targetPos, Transform transform, out Quaternion lookRotation)
    {
        Vector3 direction = (targetPos - transform.position).normalized;

        lookRotation = Quaternion.LookRotation(direction, Vector3.up);

        float angle = Utils.AngleSigned(transform.rotation, lookRotation);

        return Mathf.Abs(angle);
    }

    public static float AngleSigned(Quaternion A, Quaternion B)
    {
        Vector3 axis = Vector3.forward;
        Vector3 vecA = A * axis;
        Vector3 vecB = B * axis;

        // now we need to compute the actual 2D rotation projections on the base plane
        float angleA = Mathf.Atan2(vecA.x, vecA.z) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(vecB.x, vecB.z) * Mathf.Rad2Deg;


        // get the signed difference in these angles
        var angleDiff = Mathf.DeltaAngle( angleA, angleB );

        return angleDiff;
    }
}

static public class Vector2IntExt
{
    static public bool InBounds<T>(this Vector2Int v, T[,] array)
    {
        return
            (v.x >= 0 && v.x < array.GetLength(0)) &&
            (v.y >= 0 && v.y < array.GetLength(1));
    }


    static public Vector2 ToVector2(this Vector2Int v)
    {
        return new Vector2(v.x, v.y);
    }
    static public Vector3 OntoXZPlane(this Vector2Int v)
    {
        return new Vector3(v.x, 0, v.y);
    }

}

static public class ColorExt
{
    static public Color AddR(this Color col, float r) => new Color(col.r + r, col.b, col.g, col.a);
    static public Color AddB(this Color col, float b) => new Color(col.r, col.b + b, col.g, col.a);
    static public Color AddG(this Color col, float g) => new Color(col.r, col.b, col.g + g, col.a);
    static public Color AddA(this Color col, float a) => new Color(col.r, col.b, col.g, col.a + a);


    static public Color SetR(this Color col, float r) => new Color(r, col.g, col.b, col.a);
    static public Color SetB(this Color col, float g) => new Color(col.r, g, col.b, col.a);
    static public Color SetG(this Color col, float b) => new Color(col.r, col.g, b, col.a);
    static public Color SetA(this Color col, float a) => new Color(col.r, col.g, col.b, a);
}
static public class Vector2Ext
{
    static public Vector3 ProjectToXZPlane(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
}

static public class Vector3Ext
{
    static public Vector3 Mult(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x,
                           a.y * b.y,
                           a.z * b.z );
    }

    static public void ClampMinMaxParameters(Vector3 a, Vector3 b, out Vector3 clampMin, out Vector3 clampMax)
    {
        clampMin = default; clampMax = default;

        // x component
        if(a.x < b.x) {
            clampMin.x = a.x;
            clampMax.x = b.x;
        }
        else {
            clampMin.x = b.x;
            clampMax.x = a.x;
        }


        // y component
        if(a.y < b.y) {
            clampMin.y = a.y;
            clampMax.y = b.y;
        }
        else {
            clampMin.y = b.y;
            clampMax.y = a.y;
        }


        // z component
        if(a.z < b.z) {
            clampMin.z = a.z;
            clampMax.z = b.z;
        }
        else {
            clampMin.z = b.z;
            clampMax.z = a.z;
        }
    }

    static public Vector3 CirclePoint(float angle, float radius)
    {
        angle *= Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
    }

    static public Vector3 WithoutY(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    static public Vector3 AddX(this Vector3 v, float x) => new Vector3(v.x + x, v.y, v.z);
    static public Vector3 AddY(this Vector3 v, float y) => new Vector3(v.x, v.y + y, v.z);
    static public Vector3 AddZ(this Vector3 v, float z) => new Vector3(v.x, v.y, v.z + z);


    static public Vector3 SetX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
    static public Vector3 SetY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    static public Vector3 SetZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

}


#if UNITY_EDITOR
static public class HandlesExt
{
    static public float DrawRadiusHandle(Transform transform, float targetValue, string labelText, Color color,  int fontSize=20)
    {
        Handles.color = color;

        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = fontSize;

        Handles.Label(transform.position + Vector3.right * targetValue,
                      labelText,
                      textStyle);

        return Handles.RadiusHandle(Quaternion.identity,
                                    transform.position,
                                    targetValue);
    }
}
#endif

static public class ReflectionExtension
{
    static public IEnumerable<T> GetFieldsValuesOfType<T>(object callee, BindingFlags bindingFlags = BindingFlags.Instance)
    {
        return callee.GetType()
            .GetFields(bindingFlags)
            .Where(f=>f.GetType() == typeof(T))
            .Select(f=>(T)f.GetValue(callee));
    }

    static public IEnumerable<FieldInfo> GetFieldsOfType<T>(object callee, BindingFlags bindingFlags = BindingFlags.Instance)
    {
        return callee.GetType()
            .GetFields(bindingFlags)
            .Where(f=>f.GetType() == typeof(T));
    }
}

static public class StringExtension
{
    static public string HideString(this string str)
    {
        return new string(str.Select(c => (char.IsLetterOrDigit(c) ? '?' : c)).ToArray());
    }

    static public string FilterNonNumbers(this string str)
    {
        return new string(str.Where(c => char.IsDigit(c)).ToArray());
    }
}
