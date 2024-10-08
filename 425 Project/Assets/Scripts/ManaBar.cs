using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{

    private Image barImage;
    private Mana mana;

    

    // Start is called before the first frame update
    void Start()
    {
        barImage = transform.Find("Bar").GetComponent<Image>();

        mana = new Mana();

    }


    // Update is called once per frame
    void Update()
    {
        mana.Update();

        barImage.fillAmount = mana.GetMana();
    }

    public bool UseMana(int amt)
    {

        
        return mana.spendMana(amt);
    }
}

public class Mana
{
    private const int MANA_MAX = 100;
    private float manaAmount;
    private float regenAmount;

    public Mana()
    {
        manaAmount = 0;
        regenAmount = 20f;
    }

    public void Update()
    {
        
        float temp = regenAmount * Time.deltaTime + manaAmount;
        manaAmount = Mathf.Clamp(temp, 0f, MANA_MAX);
    }

    public bool spendMana(int amount)
    {
        if (manaAmount >= amount)
        {
            manaAmount -= amount;
            return true;
        } else
        {
            return false;
        }
    }

    public float GetMana()
    {
        return manaAmount / MANA_MAX;
    }
}
