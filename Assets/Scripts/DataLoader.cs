using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// For loading adressable stuff and access to them.
/// </summary>
public class DataLoader
{
    // character types
    public static Dictionary<string, GameCharacterData> charTypes = new();

    // contract types
    public static Dictionary<string, ContractData> contracts = new();

    // motivation condition scriptables
    public static Dictionary<string, MotCondData> motConds = new ();

    // sprites
    public static Dictionary<string, FaceDetailData> hairDetail = new();
    public static Dictionary<string, FaceDetailData> browDetail = new();
    public static Dictionary<string, FaceDetailData> facialHairDetail = new();

    // effects a character could endure
    public static Dictionary<string, EffectData> effects = new();

    public void LoadData()
    {
        Addressables.LoadAssetsAsync<GameCharacterData>("charTypes", OnLoadCharTypesCompleted);
        Addressables.LoadAssetsAsync<MotCondData>("motConditions", OnLoadMotCondDataCompleted);
        Addressables.LoadAssetsAsync<FaceDetailData>("hairDetail", OnLoadHairDetailCompleted);
        Addressables.LoadAssetsAsync<FaceDetailData>("browDetail", OnLoadBrowDetailCompleted);
        Addressables.LoadAssetsAsync<FaceDetailData>("facialHairDetail", OnLoadFacialHairDetailCompleted);
        Addressables.LoadAssetsAsync<EffectData>("characterEffects", OnLoadCharEffectsCompleted);
        Addressables.LoadAssetsAsync<ContractData>("contracts", OnLoadContractsCompleted);
    }

    private void OnLoadContractsCompleted(ContractData result)
    {
        contracts[result.contractID] = result;
    }

    private void OnLoadCharTypesCompleted(GameCharacterData result)
    {
        charTypes[result.charTypeID] = result;
    }

    private void OnLoadCharEffectsCompleted(EffectData result)
    {
        effects[result.effectID] = result;
    }

    private void OnLoadMotCondDataCompleted(MotCondData result)
    {
        motConds[result.effectID] = result;
    }

    private void OnLoadHairDetailCompleted(FaceDetailData result)
    {
        hairDetail[result.name] = result;
    }

    private void OnLoadBrowDetailCompleted(FaceDetailData result)
    {
        facialHairDetail[result.name] = result;
    }

    private void OnLoadFacialHairDetailCompleted(FaceDetailData result)
    {
        browDetail[result.name] = result;
    }
}
