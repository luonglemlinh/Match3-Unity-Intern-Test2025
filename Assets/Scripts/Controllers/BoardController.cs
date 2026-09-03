using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private Camera m_cam;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private bool m_gameOver;

    private NormalItem[] m_trayItems;

    private Cell[] m_trayslotOriginCells;

    private Transform[] m_traySlots;

    private GameManager.eLevelMode m_playMode;

    private Transform m_boardRoot;

    private TraySlotsHolder m_trayHolder;

    public void StartGame(GameManager gameManager, GameSettings gameSettings, TraySlotsHolder trayHolder)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_trayHolder = trayHolder;

        m_playMode = gameManager.CurrentLevelMode;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_boardRoot = this.transform;

        m_board = new Board(this.transform, gameSettings);

        int slotCount = 5;

        if (trayHolder != null)
        {
            slotCount = trayHolder.GetSlotCount();
        }
        else if (gameSettings != null)
        {
            slotCount = gameSettings.TraySlotCount;
        }
        m_trayItems = new NormalItem[slotCount];
        m_trayslotOriginCells = new Cell[slotCount];
        m_traySlots = new Transform[slotCount];

        if (trayHolder != null)
        {
            for (int i = 0; i < slotCount; i++)
            {
                m_traySlots[i] = trayHolder.GetSlotTransform(i);
            }
        }

        Fill();

        if (m_playMode == GameManager.eLevelMode.AUTO_PLAY)
        {
            StartCoroutine(AutoPlayRoutine());
        }

        else if (m_playMode == GameManager.eLevelMode.AUTO_LOSE)
        {
            StartCoroutine(AutoLoseRoutine());
        }

        UnityEngine.UI.Text levelConditionView = m_gameManager.GetLevelConditionView();
        if (levelConditionView != null)
        {
            levelConditionView.transform.parent.gameObject.SetActive(m_playMode == GameManager.eLevelMode.TIMER);   
        }
    }

    private void Fill()
    {
        m_board.FillWithTripleCounts();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.WIN:
                m_gameOver = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = m_cam.ScreenToWorldPoint(Input.mousePosition);
            if (m_playMode == GameManager.eLevelMode.TIMER)
            {
                int traySlot = GetNearTraySlot(worldPos);
                if (traySlot >= 0)
                {
                    ReturnItemToBoard(traySlot);
                    return;
                }
            }

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && !cell.IsEmpty)
                {
                    OnCellTapped(cell);
                }
            }
        }
    }

    private void OnCellTapped(Cell cell)
    {
        if (IsBusy) return;
        if (cell == null || cell.Item == null) return;
        int slotIndex = GetFreeTraySlot();
        if (slotIndex < 0) return;
        NormalItem item = cell.Item as NormalItem;
        if (item == null || item.View == null) return;

        if (m_traySlots[slotIndex] == null) return;

        IsBusy = true;

        cell.Free();

        m_trayslotOriginCells[slotIndex] = cell;
        Transform slot = m_traySlots[slotIndex];

        item.View.DOMove(slot.position, 0.25f).OnComplete(() =>
        {
            item.View.SetParent(slot);
            PlaceInTray(slotIndex, item);
        });
    }

    private int GetFreeTraySlot()
    {
        for ( int i = 0; i < m_trayItems.Length; i++)
        {
            if (m_trayItems[i] == null) return i;
        }
        return -1;
    }

    private void PlaceInTray(int slotIndex, NormalItem item)
    {
        if (item == null)
        {
            IsBusy = false;
            return;
        }

        m_trayItems[slotIndex] = item;
        List<int> matchIndices = new List<int>();
        for (int i = 0; i < m_trayItems.Length; i++)
        {
            if (m_trayItems[i] != null && m_trayItems[i].ItemType == item.ItemType)
            {
                matchIndices.Add(i);
            }
        }

        if (matchIndices.Count >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                int idx = matchIndices[i];
                if (m_trayItems[idx] != null)
                {
                    m_trayItems[idx].ExplodeView();
                }
                m_trayItems[idx] = null;

                m_trayslotOriginCells[idx] = null;
            }

            if (m_board.IsEmpty())
            {
                IsBusy = false;
                m_gameManager.Win();
                return;
            }
            else if (GetFreeTraySlot() < 0)
            {
                IsBusy = false;
                m_gameManager.Lose();
                return;
            }
        }

        // Check if tray is now full after placing item
        if (GetFreeTraySlot() < 0 && !m_board.IsEmpty())
        {
            IsBusy = false;
            m_gameManager.Lose();
            return;
        }

        IsBusy = false;
    }

    private int GetNearTraySlot(Vector3 worldPos)
    {
        float radius = 0.5f;
        for (int i = 0; i < m_traySlots.Length; i++)
        {
            if (m_trayItems[i] != null && m_traySlots[i] != null)
            {
                float dist = Vector2.Distance(worldPos, m_traySlots[i].position);
                if (dist < radius) return i;
            }
        }
        return -1;
    }

    private void ReturnItemToBoard(int slotIndex)
    {
        NormalItem item = m_trayItems[slotIndex];
        Cell originCell = m_trayslotOriginCells[slotIndex];
        if (item == null || originCell == null || item.View == null) return;
        IsBusy = true;
        m_trayItems[slotIndex] = null;
        m_trayslotOriginCells[slotIndex] = null;

        item.View.SetParent(m_boardRoot);
        item.View.DOMove(originCell.transform.position, 0.25f).OnComplete(() =>
        {
            originCell.Assign(item);
            originCell.ApplyItemPosition(false);
            IsBusy = false;
        });
    }

    private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    {
        if (cell1.Item is BonusItem)
        {
            cell1.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else if (cell2.Item is BonusItem)
        {
            cell2.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else
        {
            List<Cell> cells1 = GetMatches(cell1);
            List<Cell> cells2 = GetMatches(cell2);

            List<Cell> matches = new List<Cell>();
            matches.AddRange(cells1);
            matches.AddRange(cells2);
            matches = matches.Distinct().ToList();

            if (matches.Count < m_gameSettings.MatchesMin)
            {
                m_board.Swap(cell1, cell2, () =>
                {
                    IsBusy = false;
                });
            }
            else
            {
                OnMoveEvent();

                CollapseMatches(matches, cell2);
            }
        }
    }



    private List<Cell> GetMatches(Cell cell)
    {
        List<Cell> listHor = m_board.GetHorizontalMatches(cell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        List<Cell> listVert = m_board.GetVerticalMatches(cell);
        if (listVert.Count < m_gameSettings.MatchesMin)
        {
            listVert.Clear();
        }

        return listHor.Concat(listVert).Distinct().ToList();
    }

    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if(matches.Count > m_gameSettings.MatchesMin)
        {
            m_board.ConvertNormalToBonus(matches, cellEnd);
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        m_board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        m_board.Shuffle();

        yield return new WaitForSeconds(0.3f);
    }


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    private IEnumerator AutoPlayRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (!m_gameOver)
        {
            NormalItem.eNormalType target = default(NormalItem.eNormalType);
            bool hasTarget = false;
            for (int i = 0; i < m_trayItems.Length; i++)
            {
                if (m_trayItems[i] != null)
                {
                    target = m_trayItems[i].ItemType;
                    hasTarget = true;
                    break;
                }
            }
            if (!hasTarget)
            {
                NormalItem.eNormalType[] allTypes = (NormalItem.eNormalType[])System.Enum.GetValues(typeof(NormalItem.eNormalType));
                for (int t = 0; t < allTypes.Length; t++)
                {
                    if (m_board.FindCellOfType(allTypes[t]) != null)
                    {
                        target = allTypes[t];
                        hasTarget = true;
                        break;
                    }
                }
            }

            if (!hasTarget) yield break;

            Cell cell = m_board.FindCellOfType(target);
            if (cell != null) OnCellTapped(cell);
            yield return new WaitForSeconds(0.5f);
            while (IsBusy) yield return null;
        }
        
    }

    private IEnumerator AutoLoseRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        while (!m_gameOver)
        {
            List<NormalItem.eNormalType> typesInTray = new List<NormalItem.eNormalType>();
            for (int i = 0; i < m_trayItems.Length; i++)
            {
                if (m_trayItems[i] != null && !typesInTray.Contains(m_trayItems[i].ItemType))
                {
                    typesInTray.Add(m_trayItems[i].ItemType);
                }
            }

            List<NormalItem.eNormalType> boardTypes = m_board.GetDistinctTypesOnBoard();
            NormalItem.eNormalType target = default(NormalItem.eNormalType);

            bool hasTarget = false;

            for(int i = 0; i < boardTypes.Count; i++)
            {
                if (!typesInTray.Contains(boardTypes[i]))
                {
                    target = boardTypes[i];
                    hasTarget = true;
                    break;
                }
            }
            if (!hasTarget && boardTypes.Count > 0)
            {
                target = boardTypes[0];
                hasTarget = true;
            }

            if (!hasTarget) yield break;
            Cell cell = m_board.FindCellOfType(target);
            if (cell != null)
            {
                OnCellTapped(cell);
            }
            yield return new WaitForSeconds(0.5f);

            while (IsBusy) yield return null;
        }
    }


                internal void Clear()
    {
        m_board.Clear();
    }

}
