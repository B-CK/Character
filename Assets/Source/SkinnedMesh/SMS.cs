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

    void Awake()
    {
        string path = "character/player/qingyun/prefab/qingyun";
        var temp = Resources.Load(path);
        m_Bone = Instantiate(temp) as GameObject;
        path = "avatar/qingyun/qingyun_10001/qingyun_10001";
        temp = Resources.Load(path);
        m_Skin = Instantiate(temp) as GameObject;
        path = "character/player/qingyun/qingyun";
        temp = Resources.Load(path);
        m_Ctrl = Instantiate(temp) as RuntimeAnimatorController;

        SkinnedMeshRenderer skinnedMesh = m_Skin.GetComponentInChildren<SkinnedMeshRenderer>();
        Transform rootBone = m_Bone.transform.Find("Bip001");

        List<Transform> boneNodes = new List<Transform>(skinnedMesh.bones);
        HashSet<string> bs = new HashSet<string>();
        boneNodes.ForEach(b => bs.Add(b.name));
        List<Transform> nodes = new List<Transform>(rootBone.GetComponentsInChildren<Transform>(true));
        List<Transform> bones = nodes.FindAll(t => bs.Contains(t.name));
        skinnedMesh.rootBone = nodes.Find(b => b.name == skinnedMesh.rootBone.name);
        skinnedMesh.bones = bones.ToArray();

        skinnedMesh.transform.parent = m_Bone.transform;
        skinnedMesh.transform.position = Vector3.zero;

        var animator = m_Bone.GetComponent<Animator>();
        animator.runtimeAnimatorController = m_Ctrl;

    }

    void SwapSkin(string skinName)
    {

    }

    void OnGUI()
    {

    }
}