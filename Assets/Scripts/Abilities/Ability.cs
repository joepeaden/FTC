using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// TIL you can use scriptable objects in such a way. I always just used them for
// data storage alone - but you can actually just use them the same way you use
// other scripts. Super cool. Cold brew baby. Let's go.

[CreateAssetMenu(fileName = "Ability", menuName = "MyScriptables/Ability")]
public class Ability : ScriptableObject
{
    // scriptable info - define the data for the ability
    [Header("Descriptive")]
    public string abilityName;
    public string description;
    public int motCost;
    public int range;
    public Sprite sprite;

    /// <summary>
    /// The current ability a pawn is about to use.
    /// </summary>
    /// <remarks>
    /// I'm not sure about this, but I think it's a better place than the BattleManager.
    /// </remarks>
    public static Ability SelectedAbility;

    // this class should probably also have access to a sound system. Can do
    // an object pool for audio components. Each pawn does not need one.

    protected string dataAddress;

    // static so that we don't load more addressable references than necessary
    // (i.e. multiple honorable characters will use the same data) 
    protected static Dictionary<string, Ability> data = new ();

    /// <summary>
    /// Used to check if the scriptable data has been loaded yet for this ability.
    /// </summary>
    public bool IsInitialized => data.ContainsKey(dataAddress) && data[dataAddress] != null;

    /// <summary>
    /// Activate the ability. Should be overridden by subclass for the
    /// actual behavior.
    /// </summary>
    /// <param name="currentPawn"></param>
    /// <param name="targetPawn"></param>
    public virtual bool Activate(Pawn currentPawn, Pawn targetPawn)
    {
        Debug.Log("Ability not implemented!");
        return false;
    }

    /// <summary>
    /// Should be called by subclass, not externals, to load the addressable data
    /// </summary>
    /// <remarks>
    /// The ability is instantiated, and then later loads the scriptable instance
    /// of itself for the data values.... a little weird here. But, it works for
    /// now. Sorry future me if this is confusing.
    /// </remarks>
    public void LoadData()
    {
        if (!data.ContainsKey(dataAddress))
        {
            // this way, when the next ability is instantiated, the data DOES contain the key. Because the
            // LoadAssetAsync method is async, so it might get called alot before it's actually in the
            // dictionary to stop another call.
            data[dataAddress] = null;
            Addressables.LoadAssetAsync<Ability>(dataAddress).Completed += OnLoadDataCompleted;
        }
    }

    private void OnLoadDataCompleted(AsyncOperationHandle<Ability> result)
    {
        if (result.Status == AsyncOperationStatus.Succeeded)
        {
            data[dataAddress] = result.Result;
        }
        else
        {
            Debug.LogError("Failed to load ScriptableObject with key: " + dataAddress);
        }
    }

    public Ability GetData()
    {
        return data[dataAddress];
    }
}
