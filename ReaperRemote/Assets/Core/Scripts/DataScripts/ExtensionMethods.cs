using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core{

public static class ExtensionMethods
{
    #region List<> methods
    public static void ContainElseAdd<T> ( this List<T> list, T obj ) where T : UnityEngine.Object {
		if ( !list.Contains ( obj ) ) {
			list.Add ( obj );
		}
	}

	public static T Last<T> ( this List<T> list ) where T : UnityEngine.Object {
		return list [list.Count - 1];
	}    
    #endregion List<> methods

    #region Gameobjects
    public static bool HasComponent<T> ( this GameObject gameObject ) where T : Component {
		return gameObject.GetComponent<T> () != null;
	}

    #endregion Gameobjects

    #region Vectors
    public static Vector3 GetClosest ( this Vector3 position, IEnumerable<Vector3> otherPositions ) {
		Vector3 closest = Vector3.zero;
		float shortestDistance = Mathf.Infinity;

		foreach ( Vector3 otherPos in otherPositions ) {
			float distance = ( position - otherPos ).magnitude;
			if ( distance < shortestDistance ) {
				closest = otherPos;
				shortestDistance = distance;
			}
		}
		return closest;
	}

    public static Vector3 DirectionTo ( this Transform transform, Transform other ) {
        return other.position - transform.position;
    }
    #endregion Vectors

    #region Transform
    /// <summary>
    /// Resets the local coordinate.
    /// </summary>
    /// <param name="transform">Transform.</param>
    public static void ResetLocal ( this Transform transform ) {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    
    public static void ResetChildScales ( this Transform transform ) {
		for ( int i = 0 ; i < transform.childCount ; i++ ) {
			transform.GetChild ( i ).localScale = Vector3.one;
		}
	}

	public static void ResetChildRotations ( this Transform transform ) {
		for ( int i = 0 ; i < transform.childCount ; i++ ) {
			transform.GetChild ( i ).localRotation = Quaternion.identity;
		}
	}

    public static void ResetChildTransforms ( this Transform transform ) {
		for ( int i = 0 ; i < transform.childCount ; i++ ) {
			transform.GetChild ( i ).localPosition = Vector3.zero;
			transform.GetChild ( i ).localScale = Vector3.one;
			transform.GetChild ( i ).localRotation = Quaternion.identity;
		}
	}

	public static void ResetChildPositions ( this Transform transform ) {
		for ( int i = 0 ; i < transform.childCount ; i++ ) {
			transform.GetChild ( i ).localPosition = Vector3.zero;
		}
	}
    #endregion Transform

    #region String
    public static string Colorize ( this string text, Color desiredColor ) {
        return string.Format ( "<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", 
        ( byte ) ( desiredColor.r * 255f ), 
        ( byte ) ( desiredColor.g * 255f ), 
        ( byte ) ( desiredColor.b * 255f ), text );
    }
    #endregion String


}

}
#region UI
namespace  Core.UI{
	using UnityEngine.Events;
	using UnityEngine.UI;
public static class UIExtensions{
	public static void RemoveAllAndAdd(this Button button, UnityAction action)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(action);
	}
}
}
#endregion UI