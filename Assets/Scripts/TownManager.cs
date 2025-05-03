using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TownManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _goldText; 

    private void Awake()
    {
        Tile.OnTileHoverStart.AddListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.AddListener(HandleTileHoverEnd);
    }

    void Start()
    {
        Spawner.Instance.SpawnTown();

        int i = 0;
        // add inventory
        foreach (ItemData item in GameManager.Instance.PlayerInventory)
        {
            Spawner.Instance.SpawnTownItem(item, GridGenerator.Instance.TownInventorySpawns[i], true);
            i++;
        }

        // fill out shop
        int shopItemsCount = Random.Range(3, 6);
        for (i = 0; i < shopItemsCount; i++)
        {
            ItemData item = DataLoader.items.Values.ToList()[Random.Range(0, DataLoader.items.Count)];
            Spawner.Instance.SpawnTownItem(item, GridGenerator.Instance.TownShopSpawns[i], false);
        }
    }
    
    private void OnEnable() 
    {
        UpdateGoldText(); 
    }

    private void OnDestroy()
    {
        Tile.OnTileHoverStart.RemoveListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.RemoveListener(HandleTileHoverEnd);   
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    Tile clickedTile = hit.transform.GetComponent<Tile>();
                    if (clickedTile != null)
                    {
                        Pawn hoveredPawn = clickedTile.GetPawn();
                        ItemUIImmersive item = clickedTile.GetItem();

                        if (hoveredPawn != null)
                        {
                            Debug.Log("started hovering " + hoveredPawn.GameChar.CharName);
                        }

                        if (item != null)
                        {
                            HandleItemClicked(item, clickedTile);
                        }
                    }
                }
            }
        }
    }

    private void HandleItemClicked(ItemUIImmersive item, Tile tile)
    {
        TransactItem(item, tile, !item.PlayerOwns);
    }

    public void TransactItem(ItemUIImmersive item, Tile tile, bool isBuying)
    {
        bool success = true;
        if (isBuying)
        {
            // add item to player inventory data structure
            success =  GameManager.Instance.TryBuyItem(item.Item);
            AddInventoryItem(item.Item);
        }
        else
        {
            // remove item from player inventory data structure
            GameManager.Instance.RemoveItem(item.Item);
            // give the player money back
            GameManager.Instance.AddGold(item.Item.itemPrice);
            AddShopItem(item.Item);
            // CheckForGameVictory();
        }

        // !success can mean we didn't have enough money
        if (!success)
        {
            return;
        }

        tile.SetItem(null);
        Destroy(item.gameObject);

        // PlaySound(goldSound);

        UpdateGoldText();
    }

    private void AddInventoryItem(ItemData item)
    {
        foreach (Tile tile in  GridGenerator.Instance.TownInventorySpawns)
        {
            if (tile.GetItem() == null)
            {
                Spawner.Instance.SpawnTownItem(item, tile, true);
                break;
            }
        }
    }

    private void AddShopItem(ItemData item)
    {
        foreach (Tile tile in  GridGenerator.Instance.TownShopSpawns)
        {
            if (tile.GetItem() == null)
            {
                Spawner.Instance.SpawnTownItem(item, tile, false);
                break;
            }
        }
    }

    private void UpdateGoldText()
    {
        _goldText.text = "Gold: " + GameManager.Instance.PlayerGold;
    }

    public void HandleTileHoverStart(Tile targetTile)
    {
        Pawn hoveredPawn = targetTile.GetPawn();
        ItemUIImmersive item = targetTile.GetItem();

        // if (hoveredPawn != null)
        // {
        //     Debug.Log("started hovering " + hoveredPawn.GameChar.CharName);
        // }

        // if (item != null)
        // {
        //     Debug.Log("started hovering " + item.gameObject.name);
        // }
    }

    public void HandleTileHoverEnd(Tile t)
    {
        Pawn hoveredPawn = t.GetPawn();
        ItemUIImmersive item = t.GetItem();

        // if (hoveredPawn != null)
        // {
        //     Debug.Log("stopped hovering " + hoveredPawn.GameChar.CharName);
        // }

        // if (item != null)
        // {
        //     Debug.Log("stopped hovering " + item.gameObject.name);
        // }
    }

}
