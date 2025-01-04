using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Ability
{
    // this class should probably also have access to a sound system. Can do
    // an object pool for audio components. Each pawn does not need one.

    protected string dataAddress;
    // static so that we don't load more addressable references than necessary
    // (i.e. multiple honorable characters will use the same data) 
    protected static Dictionary<string, AbilityData> data;

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
        Addressables.LoadAssetAsync<AbilityData>(dataAddress).Completed += OnLoadDataCompleted;
    }

    private void OnLoadDataCompleted(AsyncOperationHandle<AbilityData> result)
    {
        if (result.Status == AsyncOperationStatus.Succeeded)
        {
            data.Add(dataAddress, result.Result);
        }
        else
        {
            Debug.LogError("Failed to load ScriptableObject with key: " + dataAddress);
        }
    }

    /// <summary>
    /// Needs to be called to release addressable data
    /// </summary>
    public void ReleaseDataReferences()
    {
        Addressables.Release(data[dataAddress]);
    }
}
