using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class AnimCompress
{
    [MenuItem("Assets/Compress Anim Key", false, 500)]
    static void OnlyCompressKey()
    {

    }

    [MenuItem("Assets/Compress Anim Float", false, 501)]
    static void OnlyCompressFloatPrecision()
    {

    }

    [MenuItem("Assets/Compress Anim Key-Float", false, 502)]
    static void CompressKeyAndFloatPrecision()
    {

    }

    [MenuItem("Assets/Compress Anim Key", true, 500)]
    [MenuItem("Assets/Compress Anim Float", true, 501)]
    [MenuItem("Assets/Compress Anim Key-Float", true, 502)]
    static bool RequireAnimation()
    {
        bool isOk = false;
        string[] guids = Selection.assetGUIDs;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                isOk = true;
                break;
            }
        }
        return isOk;
    }
}

