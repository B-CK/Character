using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class AnimCompress
{
    const string _postionCurve = "Position";
    const string _scaleCurve = "Scale";

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

