using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAutoLose : MonoBehaviour
{
    private GameManager m_gameManager;

    private void Start()
    {
        m_gameManager = FindObjectOfType<GameManager>();
    }

    public void LoadAutoLoseLevel()
    {
        if (m_gameManager != null)
        {
            m_gameManager.LoadLevel(GameManager.eLevelMode.AUTO_LOSE);
        }
    }
}
