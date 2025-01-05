using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [Header("PawnPreviews")]
    public GameObject pawnPreviewPrefab;
    public int pawnPreviewPoolSize;

    //[Header("Enemies")]
    //public GameObject enemyPrefab;
    //public int enemyPoolSize;

    [Header("AudioSources")]
    public GameObject audioSourcePrefab;
    public int audioSourcePoolSize;

    //[Header("corpses")]
    //public GameObject corpsePrefab;
    //public int corpsePoolSize;

    //[Header("TextFloatUp")]
    //public GameObject textFloatUpPrefab;
    //public int textFloatUpPoolSize;

    //[Header("ShellCasings")]
    //public GameObject shellPrefab;
    //public int shellPoolSize;

    //[Header("Smaples")]
    //public GameObject samplePrefab;
    //public int samplePoolSize;

    private Transform objectPoolParent;
    private List<GameObject> pawnPreviews;
    //private List<GameObject> enemies;
    private List<GameObject> audioSources;
    //private List<GameObject> corpses;
    //private List<GameObject> textFloatUps;
    //private List<GameObject> samples;
    //private List<GameObject> shells;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // just for organization
        objectPoolParent = Instantiate(new GameObject()).GetComponent<Transform>();
        objectPoolParent.name = "ObjectPool";

        audioSources = CreatePool(audioSourcePrefab, audioSources, audioSourcePoolSize);
        pawnPreviews = CreatePool(pawnPreviewPrefab, pawnPreviews, pawnPreviewPoolSize);
    }

    private List<GameObject> CreatePool(GameObject prefab, List<GameObject> listToAssign, int count)
    {
        listToAssign = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < count; i++)
        {
            tmp = Instantiate(prefab, objectPoolParent);
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
