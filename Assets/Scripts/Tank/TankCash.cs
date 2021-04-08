using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class TankCash : NetworkBehaviour
{
    public int m_StartingCash = 100;
    public Text m_CashText;

    private int m_CurrentCash;

    private void Start()
    {
        m_CurrentCash = m_StartingCash;
        m_CashText = GameObject.Find("CashText").GetComponent<Text>();

        SetCashUI();
    }

    private void SetCashUI()
    {
        m_CashText.text = m_CurrentCash.ToString("#,#");
    }

    public int getCash()
    {
        return m_CurrentCash;
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

    [ClientRpc]
    public void RpcAdd(int amount)
    {
        if (!isLocalPlayer) return;

        Add(amount);
    }

    [ClientRpc]
    public void RpcSubtract(int amount)
    {
        if (!isLocalPlayer) return;

        Subtract(amount);
    }
}
