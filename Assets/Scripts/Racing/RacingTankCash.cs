
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RacingTankCash : MonoBehaviour
{
    public int m_StartingCash = 100;
    public Text m_CashText;

    private float m_CurrentCash;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(m_StartingCash);
        m_CurrentCash = m_StartingCash;
        m_CashText = GameObject.Find("CashText").GetComponent<Text>();

        SetCashUI();
    }

    private void SetCashUI()
    {
        m_CashText.text = m_CurrentCash.ToString("#,#");
    }

    public void Add(int amount)
    {
        m_CurrentCash += amount;
        SetCashUI();
    }

    public void Subtract(int amount)
    {
        if (m_CurrentCash >= amount)
        {
            m_CurrentCash -= amount;
            SetCashUI();
        }
    }

}
