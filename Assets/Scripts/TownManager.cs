using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TownManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private CharDetailPanel _followerDetails;
    [SerializeField] private CharDetailPanel _recruitDetails;
    [SerializeField] private ItemDetailsImmersive _shopItemDetails;
    [SerializeField] private ItemDetailsImmersive _inventoryItemDetails;

    private ItemUIImmersive _selectedItem;
    private Pawn _selectedPawn;
    private Tile _selectedTile;

    private float leftClickBeginTime;
    private float timeToTriggerDrag = .25f;
    private bool _isDragging;

    private void Awake()
    {
        Tile.OnTileHoverStart.AddListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.AddListener(HandleTileHoverEnd);
        GameManager.Instance.OnPlayerInventoryUpdated.AddListener(RefreshInventory);
    }

    void Start()
    {
        Spawner.Instance.SpawnTown();

        RefreshInventory();

        // fill out shop
        int shopItemsCount = Random.Range(3, 6);
        for (int i = 0; i < shopItemsCount; i++)
        {
            ItemData item = DataLoader.items.Values.ToList()[Random.Range(0, DataLoader.items.Count)];
            Spawner.Instance.SpawnTownItem(item, GridGenerator.Instance.TownShopSpawns[i], false);
        }

        _followerDetails.gameObject.SetActive(false);
        _recruitDetails.gameObject.SetActive(false);
        _shopItemDetails.gameObject.SetActive(false);
        _inventoryItemDetails.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdateGoldText();
    }

    private void OnDestroy()
    {
        Tile.OnTileHoverStart.RemoveListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.RemoveListener(HandleTileHoverEnd);
        GameManager.Instance.OnPlayerInventoryUpdated.RemoveListener(RefreshInventory);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ContextMenu.Instance.Hide();

            _selectedItem = null;
            _selectedPawn = null;

            _followerDetails.gameObject.SetActive(false);
            _recruitDetails.gameObject.SetActive(false);
            _inventoryItemDetails.gameObject.SetActive(false);
            _shopItemDetails.gameObject.SetActive(false);

            // optimizaion idea: I'm doing like 3 raycasts that all do the same thing in here. I can
            // just do that once I'm sure and reuse the result. Whatever for now. 
            Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    Tile hoveredTile = hit.transform.GetComponent<Tile>();
                    if (hoveredTile != null)
                    {

                        // this is also copy and pasted like 10 times. Should combine.
                        Pawn tilePawn = hoveredTile.GetPawn();
                        ItemUIImmersive tileItem = hoveredTile.GetItem();

                        if (tilePawn != null)
                        {
                            if (GameManager.Instance.PlayerFollowers.Contains(tilePawn.GameChar))
                            {
                                _followerDetails.gameObject.SetActive(true);
                                _followerDetails.SetPawn(tilePawn);
                            }
                            else 
                            {
                                _recruitDetails.gameObject.SetActive(true);
                                _recruitDetails.SetPawn(tilePawn);
                            }
                            
                        }

                        if (tileItem != null)
                        {
                            if (tileItem.PlayerOwns)
                            {
                                _inventoryItemDetails.gameObject.SetActive(true);
                                _inventoryItemDetails.SetItem(tileItem.Item);
                            }
                            else 
                            {
                                _shopItemDetails.gameObject.SetActive(true);
                                _shopItemDetails.SetItem(tileItem.Item);
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            leftClickBeginTime = Time.time;

            Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    Tile clickedTile = hit.transform.GetComponent<Tile>();
                    if (clickedTile != null)
                    {
                        HandleTileClicked(clickedTile);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && _selectedItem != null && Time.time - leftClickBeginTime >= timeToTriggerDrag)
        {
            _isDragging = true;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            _selectedItem.transform.position = mousePos;
        }
        else if (_isDragging)
        {
            bool equipped = false;
            Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    Tile dropTile = hit.transform.GetComponent<Tile>();
                    if (dropTile != null)
                    {
                        Pawn dropPawn = dropTile.GetPawn();
                        if (dropPawn != null && GameManager.Instance.PlayerFollowers.Contains(dropPawn.GameChar))
                        {
                            // if (dropPawn != _selectedPawn)
                            // {
                            //     dropPawn.EquipItem(_selectedItem.Item);
                            // }
                            // else
                            // {
                                // the details script calls EquipItem on the pawn
                                _followerDetails.EquipItem(_selectedItem.Item);
                            // }
                            
                            equipped = true;

                            if (!_selectedItem.PlayerOwns)
                            {
                                TransactItem(_selectedItem, true, true);
                                Destroy(_selectedItem.gameObject);
                            }
                            else
                            {
                                GameManager.Instance.RemoveItem(_selectedItem.Item);
                            }
                        }
                    }
                }
            }

            if (!equipped)
            {
                _selectedItem.transform.position = _selectedItem.CurrentTile.transform.position;
            }

            _isDragging = false;
        }
    }

    private void RefreshInventory()
    {
        Debug.Log("Reloading inventory.");
        // clear all items
        foreach (Tile tile in GridGenerator.Instance.TownInventorySpawns)
        {
            ItemUIImmersive tileItem = tile.GetItem();
            if (tileItem  != null)
            {
                tile.SetItem(null);
                Destroy(tileItem.gameObject);
            }
        }

        // add inventory
        foreach (ItemData item in GameManager.Instance.PlayerInventory)
        {
            SpawnInventoryItem(item);
        }
    }

    private void HandleTileClicked(Tile tile)
    {
        SetSelectedTile(tile);

        Pawn tilePawn = tile.GetPawn();
        ItemUIImmersive tileItem = tile.GetItem();

        if (tilePawn != null)
        {
            _selectedPawn = tilePawn;
            
            if (GameManager.Instance.PlayerFollowers.Contains(tilePawn.GameChar))
            {
                _followerDetails.gameObject.SetActive(true);
                _followerDetails.SetPawn(tilePawn);
            }
            else 
            {
                _recruitDetails.gameObject.SetActive(true);
                _recruitDetails.SetPawn(tilePawn);

                Dictionary<string, UnityAction> options = new();
                options.Add("Recruit", () => { Recruit(tilePawn); });
                ContextMenu.Instance.SetOptionsAndShow(options);
            }
        }

        if (tileItem != null)
        {
            _selectedItem = tileItem;

            if (tileItem.PlayerOwns)
            {
                _inventoryItemDetails.gameObject.SetActive(true);
                _inventoryItemDetails.SetItem(tileItem.Item);
            }
            else 
            {
                _shopItemDetails.gameObject.SetActive(true);
                _shopItemDetails.SetItem(tileItem.Item);
            }
        }
    }

    private void Recruit(Pawn p)
    {
        Debug.Log(p.name);
    }

    public void TransactItem(ItemUIImmersive item, bool isBuying, bool directEquip = false)
    {
        bool success = true;
        if (isBuying)
        {
            // add item to player inventory data structure
            success = GameManager.Instance.TryBuyItem(item.Item, !directEquip);
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

        // PlaySound(goldSound);

        UpdateGoldText();
    }

    private void SpawnInventoryItem(ItemData item)
    {
        foreach (Tile tile in GridGenerator.Instance.TownInventorySpawns)
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
        foreach (Tile tile in GridGenerator.Instance.TownShopSpawns)
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

    public void SetSelectedTile(Tile newTile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
        }

        _selectedTile = newTile;
        _selectedTile.SetSelected(true);
    }

    public void HandleTileHoverStart(Tile targetTile)
    {
        Pawn hoveredPawn = targetTile.GetPawn();
        ItemUIImmersive hoveredItem = targetTile.GetItem();

        if (hoveredPawn != null && _selectedPawn == null)
        {
            if (GameManager.Instance.PlayerFollowers.Contains(hoveredPawn.GameChar))
            {
                _followerDetails.gameObject.SetActive(true);
                _followerDetails.SetPawn(hoveredPawn);
            }
            else 
            {
                _recruitDetails.gameObject.SetActive(true);
                _recruitDetails.SetPawn(hoveredPawn);
            }
            
            Debug.Log("started hovering " + hoveredPawn.GameChar.CharName);
        }

        if (hoveredItem != null && _selectedItem == null)
        {
            if (hoveredItem.PlayerOwns)
            {
                _inventoryItemDetails.gameObject.SetActive(true);
                _inventoryItemDetails.SetItem(hoveredItem.Item);
            }
            else 
            {
                _shopItemDetails.gameObject.SetActive(true);
                _shopItemDetails.SetItem(hoveredItem.Item);
            }
        }
    }

    public void HandleTileHoverEnd(Tile t)
    {
        Pawn hoveredPawn = t.GetPawn();
        ItemUIImmersive hoveredItem = t.GetItem();

        if (hoveredPawn != null && _selectedPawn == null)
        {
            if (GameManager.Instance.PlayerFollowers.Contains(hoveredPawn.GameChar))
            {
                _followerDetails.gameObject.SetActive(false);
            }
            else 
            {
                _recruitDetails.gameObject.SetActive(false);
            }

            Debug.Log("stopped hovering " + hoveredPawn.GameChar.CharName);
        }

        if (hoveredItem != null && _selectedItem == null)
        {
            if (hoveredItem.PlayerOwns)
            {
                _inventoryItemDetails.gameObject.SetActive(false);
            }
            else 
            {
                _shopItemDetails.gameObject.SetActive(false);
            }
        }
    }

}
