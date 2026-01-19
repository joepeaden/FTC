using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The pool where all the objects like to hang out.
/// There's barbecues on Saturdays.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance => _instance;
    private static ObjectPool _instance;

    [Header("PawnPreviews")]
    public GameObject pawnPreviewPrefab;
    public int pawnPreviewPoolSize;

    [Header("AudioSources")]
    public GameObject audioSourcePrefab;
    public int audioSourcePoolSize;

    [Header("ItemUIs")]
    public GameObject itemUIPrefab;
    public int itemUIPoolSize;

    [Header("Blood Splats")]
    public GameObject bloodSplatPrefab;
    public int bloodSplatPoolSize;

    private List<GameObject> pawnPreviews;
    private List<GameObject> audioSources;
    private List<GameObject> itemUIs;
    private List<GameObject> bloodSplats;

    void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("Object Pool already exists, deleting this one");
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    void Start()
    {
        audioSources = CreatePool(audioSourcePrefab, audioSources, audioSourcePoolSize);
        pawnPreviews = CreatePool(pawnPreviewPrefab, pawnPreviews, pawnPreviewPoolSize);
        itemUIs = CreatePool(itemUIPrefab, itemUIs, itemUIPoolSize);
        bloodSplats = CreatePool(bloodSplatPrefab, bloodSplats, bloodSplatPoolSize);

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
            if (!theList[i].activeSelf)
            {
                theList[i].SetActive(true);
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

    public GameObject GetItemUI()
    {
        return GetPooledObject(itemUIs, itemUIPrefab);
    }
    
    public GameObject GetBloodSplat()
    {
        return GetPooledObject(bloodSplats, bloodSplatPrefab);
    }
    

    /// <summary>
    /// Disable and re-parent the gameobject back to the object pool
    /// </summary>
    /// <param name="g"></param>
    public void Return(GameObject g)
    {
        g.gameObject.SetActive(false);
        g.transform.SetParent(ObjectPool.Instance.transform, false);
    }

    /// <summary>
    /// Resets pooled objects to not active and reparents them to the object pool - otherwise
    /// they may be destroyed along with their parent object when changing scenes.
    /// </summary>
    public void ReparentObjects()
    {
        ResetPooledObjects(itemUIs);
        ResetPooledObjects(pawnPreviews);
        ResetPooledObjects(audioSources);
        ResetPooledObjects(bloodSplats);
    }

    /// <summary>
    /// Iterate the list and set inactive & reset parent
    /// </summary>
    /// <param name="theList"></param>
    private void ResetPooledObjects(List<GameObject> theList)
    {
        foreach (GameObject g in theList)
        {
            g.transform.parent = transform;
            g.SetActive(false);
        }
    }
}
