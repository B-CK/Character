using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMS : MonoBehaviour
{
    string m_Dir = "";

    [ReadOnly]
    public GameObject m_Role;
    [ReadOnly]
    public GameObject m_Bone;
    [ReadOnly]
    public GameObject m_Skin;
    [ReadOnly]
    public RuntimeAnimatorController m_Ctrl;

    const string skin1 = "qingyun_10001";
    const string skin2 = "qingyun_10002";
    const string skin3 = "qingyun_10003";

    void Awake()
    {
        string path = "character/player/qingyun/prefab/qingyun";
        var temp = Resources.Load(path);
        m_Bone = Instantiate(temp) as GameObject;
        path = "character/player/qingyun/qingyun";
        temp = Resources.Load(path);
        m_Ctrl = Instantiate(temp) as RuntimeAnimatorController;

        SwapSkin(skin1);

        m_Bone.transform.Rotate(Vector3.up * 180);
        var animator = m_Bone.GetComponent<Animator>();
        animator.runtimeAnimatorController = m_Ctrl;
    }
 

    /// <summary>
    /// 蒙皮重新绑定骨骼
    /// </summary>
    void SwapSkin(string skinName)
    {
        string path = string.Format("avatar/qingyun/{0}/{0}", skinName);
        Object temp = Resources.Load(path);
        m_Skin = Instantiate(temp) as GameObject;

        //销毁旧的蒙皮
        m_Bone.SetActive(false);
        var old = m_Bone.GetComponentInChildren<SkinnedMeshRenderer>();
        if (old != null) Destroy(old.gameObject);

        //网格中的骨骼顺序要一致.否则蒙皮会绑定到错误的骨骼.树状结构被线性化.
        SkinnedMeshRenderer skinnedMesh = m_Skin.GetComponentInChildren<SkinnedMeshRenderer>();
        Transform rootBone = m_Bone.transform.Find("Bip001");

        Transform[] boneNodes = skinnedMesh.bones;
        List<Transform> nodes = new List<Transform>(rootBone.GetComponentsInChildren<Transform>(true));
        Dictionary<string, Transform> dict = new Dictionary<string, Transform>();
        for (int i = 0; i < nodes.Count; i++)
        {
            if (!dict.ContainsKey(nodes[i].name))
            {
                dict.Add(nodes[i].name, nodes[i]);
            }
        }
        List<Transform> bones = new List<Transform>();
        for (int i = 0; i < boneNodes.Length; i++)
        {
            if (dict.ContainsKey(boneNodes[i].name))
                bones.Add(dict[boneNodes[i].name]);
        }
        //skinnedMesh.rootBone = dict[skinnedMesh.rootBone.name];
        skinnedMesh.bones = bones.ToArray();

        skinnedMesh.transform.parent = m_Bone.transform;
        skinnedMesh.transform.position = Vector3.zero;
        Destroy(m_Skin);

        Resources.UnloadUnusedAssets();
        m_Bone.SetActive(true);
    }

    void OnGUI()
    {
        if (GUILayout.Button("Skin 1", GUILayout.Width(200)))
            SwapSkin(skin1);
        if (GUILayout.Button("Skin 2", GUILayout.Width(200)))
            SwapSkin(skin2);
        if (GUILayout.Button("Skin 3", GUILayout.Width(200)))
            SwapSkin(skin3);
    }
}