using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimCompressTool
{
    const string _postionCurve = "Position";
    const string _scaleCurve = "Scale";

    class ClipInfo
    {
        public string path;
        public AnimationClip clip;
    }

    static List<ClipInfo> GetAnimationClips()
    {
        List<ClipInfo> clips = new List<ClipInfo>();
        string[] guids = Selection.assetGUIDs;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                ClipInfo info = new ClipInfo()
                {
                    path = path,
                    clip = Object.Instantiate(clip),
                };
                clips.Add(info);
            }
        }
        return clips;
    }
    static void SaveClip(ClipInfo info)
    {
        string dirPath = string.Format("{0}/clips", Path.GetDirectoryName(info.path));
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        string path = string.Format("{0}/{1}.anim", dirPath, info.clip.name.Replace("(Clone)", ""));
        AssetDatabase.CreateAsset(info.clip, path);
        AssetDatabase.Refresh();

    }

    [MenuItem("Assets/Compress Anim Key", false, 500)]
    static void OnlyCompressKey()
    {
        List<ClipInfo> clipInfos = GetAnimationClips();
        for (int i = 0; i < clipInfos.Count; i++)
        {
            var info = clipInfos[i];
            CompressKey(info.clip, _scaleCurve);
            CompressKey(info.clip, _postionCurve);
            SaveClip(info);
            if (EditorUtility.DisplayCancelableProgressBar("Only Compress Key", info.clip.name, (i + 1f) / clipInfos.Count)) break;
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Compress Anim Float", false, 501)]
    static void OnlyCompressFloatPrecision()
    {
        List<ClipInfo> clipInfos = GetAnimationClips();
        for (int i = 0; i < clipInfos.Count; i++)
        {
            var info = clipInfos[i];
            CompressFloatPrecision(info.clip, "f3");
            SaveClip(info);
            if (EditorUtility.DisplayCancelableProgressBar("Only Compress Float", info.clip.name, (i + 1f) / clipInfos.Count)) break;
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Compress Anim Key-Float", false, 502)]
    static void CompressKeyAndFloatPrecision()
    {
        List<ClipInfo> clipInfos = GetAnimationClips();
        for (int i = 0; i < clipInfos.Count; i++)
        {
            var info = clipInfos[i];
            CompressKey(info.clip, _scaleCurve);
            CompressFloatPrecision(info.clip, "f3");
            SaveClip(info);
            if (EditorUtility.DisplayCancelableProgressBar("Only Compress Key&Float", info.clip.name, (i + 1f) / clipInfos.Count)) break;
        }
        EditorUtility.ClearProgressBar();
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


    /// <summary>
    /// 删除指定帧
    /// </summary>
    /// <param name="clip">动画剪辑</param>
    /// <param name="keyName">帧名</param>
    static void CompressKey(AnimationClip clip, string keyName = _scaleCurve)
    {
        EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clip);

        for (int j = 0; j < curves.Length; j++)
        {
            EditorCurveBinding curveBinding = curves[j];

            if (curveBinding.propertyName.Contains(keyName))
            {
                AnimationUtility.SetEditorCurve(clip, curveBinding, null);
            }
        }
    }

    /// <summary>
    /// 降低浮点数精度
    /// </summary>
    /// <param name="clip">动画剪辑</param>
    /// <param name="precision">精度格式f3,默认保留3位小数</param>
    static void CompressFloatPrecision(AnimationClip clip, string precision = "f3")
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        for (int j = 0; j < bindings.Length; j++)
        {
            EditorCurveBinding curveBinding = bindings[j];
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

            if (curve == null || curve.keys == null) continue;

            Keyframe[] keys = curve.keys;
            for (int k = 0; k < keys.Length; k++)
            {
                Keyframe key = keys[k];
                key.value = float.Parse(key.value.ToString(precision));
                key.inTangent = float.Parse(key.inTangent.ToString(precision));
                key.outTangent = float.Parse(key.outTangent.ToString(precision));
                keys[k] = key;
            }
            curve.keys = keys;

            AnimationUtility.SetEditorCurve(clip, curveBinding, curve);
        }
    }
}

