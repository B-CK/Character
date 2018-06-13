using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMS_Part : MonoBehaviour
{
    /// <summary>
    /// 骨骼结构
    /// </summary>
    public GameObject m_bone;
    public GameObject m_eye;
    public GameObject[] m_faces;
    public GameObject[] m_hairs;
    public GameObject[] m_tops;
    public GameObject[] m_pants;
    public GameObject[] m_shoes;

    GameObject m_maleGo;
    GameObject m_eyeGo;
    GameObject m_faceGo;
    GameObject m_hairGo;
    GameObject m_topGo;
    GameObject m_pantGo;
    GameObject m_shoeGo;

    Animation m_animation;

    void Awake()
    {
        m_maleGo = Instantiate(m_bone) as GameObject;
        m_maleGo.transform.eulerAngles = Vector3.up * 180;

        m_eyeGo = Instantiate(m_eye) as GameObject;
        m_faceGo = Instantiate(m_faces[0]) as GameObject;
        m_hairGo = Instantiate(m_hairs[0]) as GameObject;
        m_topGo = Instantiate(m_tops[0]) as GameObject;
        m_pantGo = Instantiate(m_pants[0]) as GameObject;
        m_shoeGo = Instantiate(m_shoes[0]) as GameObject;

        Combine();
    }

    bool m_applySingleMesh = false;
    GUILayoutOption m_bigWidth = GUILayout.Width(180);
    GUILayoutOption m_smallWidth = GUILayout.Width(90);
    void OnGUI()
    {
        m_applySingleMesh = GUILayout.Toggle(m_applySingleMesh, "是否合并网格到多个子网格?");

        bool isChange = false;
        isChange |= DrawButton(ref m_faceGo, m_faces);
        isChange |= DrawButton(ref m_hairGo, m_hairs);
        isChange |= DrawButton(ref m_topGo, m_tops);
        isChange |= DrawButton(ref m_pantGo, m_pants);
        isChange |= DrawButton(ref m_shoeGo, m_shoes);

        if (isChange)
            Combine();
    }
    bool DrawButton(ref GameObject obj, GameObject[] objects)
    {
        bool isChange = false;
        GUILayout.BeginHorizontal(m_bigWidth);
        if (GUILayout.Button(objects[0].name, m_smallWidth))
        {
            Destroy(obj);
            obj = Instantiate(objects[0]) as GameObject;
            isChange = true;
        }
        if (GUILayout.Button(objects[1].name, m_smallWidth))
        {
            Destroy(obj);
            obj = Instantiate(objects[1]) as GameObject;
            isChange = true;
        }
        GUILayout.EndHorizontal();
        return isChange;
    }

    void Combine()
    {
        List<SkinnedMeshRenderer> skinneds = new List<SkinnedMeshRenderer>();
        skinneds.Add(m_eyeGo.GetComponentInChildren<SkinnedMeshRenderer>());
        skinneds.Add(m_faceGo.GetComponentInChildren<SkinnedMeshRenderer>());
        skinneds.Add(m_hairGo.GetComponentInChildren<SkinnedMeshRenderer>());
        skinneds.Add(m_pantGo.GetComponentInChildren<SkinnedMeshRenderer>());
        skinneds.Add(m_shoeGo.GetComponentInChildren<SkinnedMeshRenderer>());
        skinneds.Add(m_topGo.GetComponentInChildren<SkinnedMeshRenderer>());

        //需要合并的网格
        List<CombineInstance> combines = new List<CombineInstance>();
        //需要绑定的骨骼
        List<Transform> bones = new List<Transform>();
        //所有网格贴图
        List<Texture2D> texture2Ds = new List<Texture2D>();
        //所有材质球
        List<Material> materials = new List<Material>();
        Dictionary<string, Transform> boneDict = new Dictionary<string, Transform>();
        Transform oldSkin = m_maleGo.transform.Find("SMS");
        if (oldSkin != null)
            Destroy(oldSkin.gameObject);
        Transform hips = m_maleGo.transform.Find("Male_Hips");
        Transform[] transforms = hips.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
            boneDict.Add(transforms[i].name, transforms[i]);

        if (m_applySingleMesh)
        {
            List<Vector2[]> uvs = new List<Vector2[]>();
            int texWidth = 0;
            int texHeight = 0;
            int uvLength = 0;

            foreach (var skin in skinneds)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = skin.sharedMesh;
                combines.Add(ci);

                uvs.Add(skin.sharedMesh.uv);
                uvLength += skin.sharedMesh.uv.Length;

                for (int i = 0; i < skin.bones.Length; i++)
                {
                    string boneName = skin.bones[i].name;
                    if (boneDict.ContainsKey(boneName))
                        bones.Add(boneDict[boneName]);
                }

                if (skin.material.mainTexture != null)
                {
                    texture2Ds.Add(skin.material.mainTexture as Texture2D);
                    texWidth += skin.material.mainTexture.width;
                    texHeight += skin.material.mainTexture.height;
                }
            }

            //被合并图片需要是可读写状态
            Texture2D texture = new Texture2D(get2Pow(texWidth), get2Pow(texHeight));
            Rect[] rects = texture.PackTextures(texture2Ds.ToArray(), 0, 1024);
            Vector2[] fullUVs = new Vector2[uvLength];
            int index = 0;
            for (int i = 0; i < uvs.Count; i++)
            {
                Rect rect = rects[i];
                for (int j = 0; j < uvs[i].Length; j++)
                {
                    Vector2 uv = uvs[i][j];
                    fullUVs[index].x = Mathf.Lerp(rect.xMin, rect.xMax, uv.x);
                    fullUVs[index].y = Mathf.Lerp(rect.yMin, rect.yMax, uv.y);
                    index++;
                }
            }

            GameObject skinGo = new GameObject("SMS");
            SkinnedMeshRenderer skinned = skinGo.AddComponent<SkinnedMeshRenderer>();
            skinned.bones = bones.ToArray();

            skinned.sharedMesh = new Mesh();
            skinned.sharedMesh.CombineMeshes(combines.ToArray(), true, false);
            skinned.sharedMesh.uv = fullUVs;

            skinned.sharedMaterial = new Material(skinneds[0].sharedMaterial);
            skinned.sharedMaterial.mainTexture = texture;

            skinGo.transform.parent = m_maleGo.transform;
            skinGo.transform.localPosition = Vector3.zero;
            skinGo.transform.localEulerAngles = Vector3.right * -90;

        }
        else
        {
            foreach (var skin in skinneds)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = skin.sharedMesh;
                combines.Add(ci);

                for (int i = 0; i < skin.bones.Length; i++)
                {
                    string boneName = skin.bones[i].name;
                    if (boneDict.ContainsKey(boneName))
                        bones.Add(boneDict[boneName]);
                }

                materials.AddRange(skin.materials);
            }


            GameObject skinGo = new GameObject("SMS");
            SkinnedMeshRenderer skinned = skinGo.AddComponent<SkinnedMeshRenderer>();
            skinned.bones = bones.ToArray();

            skinned.sharedMesh = new Mesh();
            skinned.sharedMesh.CombineMeshes(combines.ToArray(), false, false);

            skinned.materials = materials.ToArray();
            skinGo.transform.parent = m_maleGo.transform;
            skinGo.transform.localPosition = Vector3.zero;
            skinGo.transform.localEulerAngles = Vector3.right * -90;
        }

        m_eyeGo.SetActive(false);
        m_faceGo.SetActive(false);
        m_hairGo.SetActive(false);
        m_pantGo.SetActive(false);
        m_shoeGo.SetActive(false);
        m_topGo.SetActive(false);
    }

    /// <summary>
    /// 获取最接近输入值的2的N次方的数，最大不会超过1024，例如输入320会得到512
    /// </summary>
    public int get2Pow(int into)
    {
        int outo = 1;
        for (int i = 0; i < 10; i++)
        {
            outo *= 2;
            if (outo > into)
            {
                break;
            }
        }

        return outo;
    }
}
