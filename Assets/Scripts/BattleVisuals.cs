using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private int currHealth;
    private int maxHealth;
    private int level;

    private const string LEVEL_ABB = "Lvl: ";

    // Start is called before the first frame update
    void Start()
    {
        SetStartingValues(10, 10, 5);
    }

    public void SetStartingValues(int currHealth, int maxHealth, int level)
    {
        this.currHealth = currHealth;
        this.maxHealth = maxHealth;
        this.level = level;
        levelText.text = LEVEL_ABB + this.level.ToString();
        UpdateHealthBar();
    }

    public void ChangeHealth(int currHealth) 
    {
        this.currHealth = currHealth;
        // if health is 0 -> play death animation -> Destroy our battle visual
    }


    public void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currHealth;
    }
}
