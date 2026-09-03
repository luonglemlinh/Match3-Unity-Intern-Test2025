using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraySlotsHolder : MonoBehaviour
{
    private Transform[] m_slots;
    
    public void Initialize(int TraySlotCount, int boardSizeY)
    {
        m_slots = new Transform[TraySlotCount];
        
        float spacing = 1.1f;  
        float totalWidth = (TraySlotCount - 1) * spacing;
        float originX = -totalWidth * 0.5f;
        float y = -4f;

        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int i = 0; i < TraySlotCount; i++)
        {
            GameObject slotGO;

            if (prefabBG != null)
            {
                slotGO = Instantiate(prefabBG);
                slotGO.name = "TraySlot_" + i;

                Cell cell = slotGO.GetComponent<Cell>();
                if (cell != null)
                {
                    Destroy(cell);
                }
            }
            else
            {
                slotGO = new GameObject("TraySlot_" + i);
                slotGO.AddComponent<BoxCollider2D>();
            }
            slotGO.transform.SetParent(transform);
            slotGO.transform.position = new Vector3(originX + i * spacing, y, 0f);
            slotGO.transform.localScale = new Vector3(1f, 1f, 1f);
            m_slots[i] = slotGO.transform;
        }
    }

    public int GetSlotCount()
    {
        return m_slots != null ? m_slots.Length : 0;
    }

    public Transform GetSlotTransform(int index)
    {
        if (m_slots != null && index >= 0 && index < m_slots.Length)
        {
            return m_slots[index];
        }
        return null;
    }

}
