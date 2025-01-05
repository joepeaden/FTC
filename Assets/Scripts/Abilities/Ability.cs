using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Ability
{
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
    protected static Dictionary<string, AbilityData> data = new ();

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
    public void LoadData()
    {
        if (!data.ContainsKey(dataAddress))
        {
            // this way, when the next ability is instantiated, the data DOES contain the key. Because the
            // LoadAssetAsync method is async, so it might get called alot before it's actually in the
            // dictionary to stop another call.
            data[dataAddress] = null;
            Addressables.LoadAssetAsync<AbilityData>(dataAddress).Completed += OnLoadDataCompleted;
        }
    }

    private void OnLoadDataCompleted(AsyncOperationHandle<AbilityData> result)
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

    public AbilityData GetData()
    {
        return data[dataAddress];
    }

    public void PlaySound()
    {
        GameObject audioGO = ObjectPool.instance.GetAudioSource();
        audioGO.SetActive(true);
        AudioSource aSource = audioGO.GetComponent<AudioSource>();
        aSource.clip = GetData().soundEffect;
        aSource.Play();
    }

    /// <summary>
    /// Needs to be called to release addressable data
    /// Although I'm not sure when this actually is necessary...
    /// only loading one instance of each scriptable...
    /// </summary>
    public void ReleaseDataReferences()
    {
        Addressables.Release(data[dataAddress]);
    }
}
