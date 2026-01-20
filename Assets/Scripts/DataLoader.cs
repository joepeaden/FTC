using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

/// <summary>
/// For loading adressable stuff and access to them.
/// </summary>
public class DataLoader
{
    public UnityEvent OnDataLoaded = new();

    // character types
    public static Dictionary<string, GameCharacterData> charTypes = new();

    // active abilities that aren't tied to weapons
    public static Dictionary<string, Ability> abilities = new();
    
    // passive abilities for characters
    public static Dictionary<string, PassiveData> passives = new();

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

    // weapons
    public static Dictionary<string, ItemData> weapons = new();
    
    // armor
    public static Dictionary<string, ItemData> armor = new();

    // shields
    public static Dictionary<string, ItemData> shields = new();

    public async void LoadData()
    {
        var tasks = new List<Task>
        {
            LoadAssetsAsync<GameCharacterData>("charTypes", OnLoadCharTypesCompleted),
            LoadAssetsAsync<MotCondData>("motConditions", OnLoadMotCondDataCompleted),
            LoadAssetsAsync<FaceDetailData>("hairDetail", OnLoadHairDetailCompleted),
            LoadAssetsAsync<FaceDetailData>("browDetail", OnLoadBrowDetailCompleted),
            LoadAssetsAsync<FaceDetailData>("facialHairDetail", OnLoadFacialHairDetailCompleted),
            LoadAssetsAsync<EffectData>("characterEffects", OnLoadCharEffectsCompleted),
            LoadAssetsAsync<ContractData>("contracts", OnLoadContractsCompleted),
            LoadAssetsAsync<Ability>("charAbilities", OnLoadCharacterAbilities),
            LoadAssetsAsync<PassiveData>("passives", OnLoadPassives),
            LoadAssetsAsync<WeaponItemData>("weapons", OnLoadWeaponsCompleted),
            LoadAssetsAsync<ArmorItemData>("armor", OnLoadArmorCompleted),
            LoadAssetsAsync<ShieldItemData>("shields", OnLoadShieldsCompleted)
        };
        
        await Task.WhenAll(tasks);
        
        OnDataLoaded.Invoke();
    }

    private async Task LoadAssetsAsync<T>(string label, System.Action<T> callback)
    {
        var handle = Addressables.LoadAssetsAsync<T>(label, callback);
        await handle.Task;
    }

    private void OnLoadPassives(PassiveData result)
    {
        passives[result.passiveID] = result;
    }

    /// <summary>
    /// Load character specific abilities - results should not contain weapon-tied abilities!
    /// </summary>
    /// <param name="result"></param>
    private void OnLoadCharacterAbilities(Ability result)
    {
        abilities[result.abilityID] = result;
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

    private void OnLoadWeaponsCompleted(ItemData result)
    {
        weapons[result.itemName] = result;
    }

    private void OnLoadArmorCompleted(ItemData result)
    {
        armor[result.itemName] = result;
    }
    
    private void OnLoadShieldsCompleted(ItemData result)
    {
        shields[result.itemName] = result;
    }
}
