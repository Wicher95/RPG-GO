using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFunctions {

    private static List<GameObject> gameObjects = new List<GameObject>();

    public static GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
    {
        Transform t = parent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).gameObject.tag == tag)
            {
                return t.GetChild(i).gameObject;
            }

        }

        return null;
    }

    public static List<GameObject> GetChildsWithTag(Transform parent, string _tag)
    {
        gameObjects.Clear();
        GetChildObject(parent, _tag);
        return gameObjects;
    }

    public static void GetChildObject(Transform parent, string _tag)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag)
            {
                gameObjects.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                GetChildObject(child, _tag);
            }
        }
    }
}
