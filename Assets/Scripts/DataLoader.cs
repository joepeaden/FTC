using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// For loading scriptables and access to them.
/// </summary>
public class DataLoader
{ 
    public static Dictionary<string, MotCondData> motConds = new ();

    public void LoadData()
    {
        Addressables.LoadAssetsAsync<MotCondData>("mot_conditions", OnLoadDataCompleted);
    }

    private void OnLoadDataCompleted(MotCondData result)
    {
        motConds[result.condId] = result;
    }
}
