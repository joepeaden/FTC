using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The pool where all the objects like to hang out.
/// There's barbecues on Saturdays.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [Header("PawnPreviews")]
    public GameObject pawnPreviewPrefab;
    public int pawnPreviewPoolSize;

    [Header("AudioSources")]
    public GameObject audioSourcePrefab;
    public int audioSourcePoolSize;

    private List<GameObject> pawnPreviews;
    private List<GameObject> audioSources;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        audioSources = CreatePool(audioSourcePrefab, audioSources, audioSourcePoolSize);
        pawnPreviews = CreatePool(pawnPreviewPrefab, pawnPreviews, pawnPreviewPoolSize);

        // some of these are re-parented (the pawn previews) so they need to
        // be individually set like this.
        for (int i = 0; i < transform.childCount; i++)
        {
            DontDestroyOnLoad(transform.GetChild(i).gameObject);
        }
    }

    private List<GameObject> CreatePool(GameObject prefab, List<GameObject> listToAssign, int count)
    {
        listToAssign = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < count; i++)
        {
            tmp = Instantiate(prefab, transform);
            tmp.SetActive(false);
            listToAssign.Add(tmp);
        }

        return listToAssign;
    }

    private GameObject GetPooledObject(List<GameObject> theList, GameObject prefab)
    {
        for (int i = 0; i < theList.Count; i++)
        {
            if (!theList[i].activeInHierarchy)
            {
                return theList[i];
            }
        }

        GameObject newObject = Instantiate(prefab);
        theList.Add(newObject);
        return newObject;
    }

    public GameObject GetPawnPreview()
    {
        return GetPooledObject(pawnPreviews, pawnPreviewPrefab);
    }

    public GameObject GetAudioSource()
    {
        return GetPooledObject(audioSources, audioSourcePrefab);
    }
}
