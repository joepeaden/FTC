using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacter
{
    public enum Motivator
    {
        Avarice,
        Sanctimony,
        Vainglory
    }

    public string CharName => _charName;
    private string _charName;
    public bool IsPlayerChar => _isPlayerChar;
    private bool _isPlayerChar;

    // motivators
    public int Avarice => _avarice;
    private int _avarice;
    public int Sanctimony => _sanctimony;
    private int _sanctimony;
    public int Vainglory => _vainglory;
    private int _vainglory;

    public int Initiative => _initiative;
    private int _initiative;

    public int HitPoints => _hitPoints;
    private int _hitPoints;

    public ArmorItemData HeadItem => _headItem;
    private ArmorItemData _headItem;

    public ArmorItemData BodyItem => _bodyItem;
    private ArmorItemData _bodyItem;

    public WeaponItemData WeaponItem => _weaponItem;
    private WeaponItemData _weaponItem;

    //public Sprite FaceSprite => _faceSprite;
    //private Sprite _faceSprite;

    public GameCharacter(string newName, bool isPlayerCharacter, int avarice, int sanctimony, int vainglory)
    {
        _charName = newName;
        _isPlayerChar = isPlayerCharacter;
        _avarice = avarice;
        _vainglory = vainglory;
        _sanctimony = sanctimony;
        _initiative = Random.Range(1, 5);
        _hitPoints = Random.Range(3, 5);
    }

    public GameCharacter()
    {
        List<string> characterNameOptions = new()
        {
            "Ealdred",
            "Cynric",
            "Leofward",
            "Eadgarth",
            "Wulfstan",
            "Hrothgar",
            "Aldhelm",
            "Eadmund",
            "Leofric",
            "Osbruh",
            "Offa",
            "Godwin",
            "Beorthric",
            "Eadmer",
            "Godric",
            "Aldhelm",
            "Wigstan",
            "Beorn",
            "Eadric",
            "Alfwold",
            "Eadred",
            "Wulfrun",
            "Wulfric",
            "Arthur"
        };

        _charName = characterNameOptions[Random.Range(0, characterNameOptions.Count)];
        _isPlayerChar = false;

        if (GameManager.Instance != null)
        {
            GameCharacterData data = GameManager.Instance.GameCharData;
            _avarice = Random.Range(data.minVice, data.maxVice);
            _vainglory = Random.Range(data.minVice, data.maxVice);
            _sanctimony = Random.Range(data.minVice, data.maxVice);
        }
        else
        {
            _avarice = Random.Range(0, 10);
            _vainglory = Random.Range(0, 10);
            _sanctimony = Random.Range(0, 10);
        }

        _initiative = Random.Range(1, 5);
        _hitPoints = Random.Range(3, 5);
    }

    public ItemData EquipItem(ItemData newItem)
    {
        ItemData oldItem = null;

        switch (newItem.itemType)
        {
            case ItemType.Helmet:
                oldItem = _headItem;
                _headItem = (ArmorItemData) newItem;
                break;
            case ItemType.Armor:
                oldItem = _bodyItem;
                _bodyItem = (ArmorItemData) newItem;
                break;
            case ItemType.Weapon:
                oldItem = _weaponItem;
                _weaponItem = (WeaponItemData) newItem;
                break;
        }

        return oldItem;
    }

    public Motivator GetBiggestMotivator()
    {
        if (Avarice > Vainglory && Avarice > Sanctimony)
        {
            return Motivator.Avarice;
        }
        else if (Sanctimony > Vainglory && Sanctimony > Avarice)
        {
            return Motivator.Sanctimony;
        }
        else if (Vainglory > Sanctimony && Vainglory > Avarice)
        {
            return Motivator.Vainglory;
        }
        else
        {
            // two motivators must be tied - pick a random one between the two.
            int coinFlip = Random.Range(0, 2);
            if (Avarice == Vainglory && Avarice > Sanctimony)
            {
                if (coinFlip == 1)
                {
                    return Motivator.Avarice;
                }
                else
                {
                    return Motivator.Vainglory;
                }
            }
            else if (Sanctimony == Vainglory && Sanctimony > Avarice)
            {
                if (coinFlip == 1)
                {
                    return Motivator.Sanctimony;
                }
                else
                {
                    return Motivator.Vainglory;
                }
            }
            else if(Avarice == Sanctimony && Sanctimony > Vainglory)
            {
                if (coinFlip == 1)
                {
                    return Motivator.Avarice;
                }
                else
                {
                    return Motivator.Sanctimony;
                }
            }
            else
            {
                // all three are tied! Roll a D3 to pick.
                int roll = Random.Range(0, 3);
                return (Motivator) roll;
            }
        }
    }

}
