using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

public class BattleSystem : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopUp;
    [SerializeField] private TextMeshProUGUI bottomText;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Action";

    // Start is called before the first frame update
    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        AttackAction(allBattlers[0], allBattlers[1]);
    }

    private void CreatePartyEntities()
    {
        //get current party 
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetCurrentParty();

        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrHealth, currentParty[i].MaxHealth, currentParty[i].Initiative, currentParty[i].Strength, currentParty[i].Level, true);

            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab, partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentParty[i].MaxHealth, currentParty[i].MaxHealth, currentParty[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);

        }
    }

    private void CreateEnemyEntities ()
    {
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Initiative, currentEnemies[i].Strength, currentEnemies[i].Level, false);

            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab, enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentEnemies[i].MaxHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);


        }
    }

    public void ShowBattleMenu()
    {
        //whos action it is 
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;
        //enabling our battle menu
        battleMenu.SetActive(true);
    }

    public void ShowEnemySelectionMenu()
    {
        // disable the battle menu
        battleMenu.SetActive(false);

        // set our enemy selection buttons
        SetEnemySelectionButtons();

        // enable our selection menu
        enemySelectionMenu.SetActive(true);
    }

    private void SetEnemySelectionButtons()
    {
        //disable all of our buttons 
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);
        }

        for (int j = 0; j < enemyBattlers.Count; j++)
        {
            enemySelectionButtons[j].SetActive(true);
            enemySelectionButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[j].Name;
        }

        //enable buttons for each enemy 


        //change the buttons text

    }

    public void SelectEnemy(int currentEnemy)
    {
        // setting the current members target 
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));

        //tell the battle system this member intends to attack
        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;

        //increment through our party members 
        currentPlayer++;

        if(currentPlayer >= playerBattlers.Count)
        {
            //Start the battle
            Debug.Log("Start the battle!");
            Debug.Log("We are attacking: " + allBattlers[currentPlayerEntity.Target].Name);
        }
        else
        {
            enemySelectionMenu.SetActive(false); //show the battle menu for the next player 
            ShowBattleMenu();
        }
        //if all players have selected an action
            // start the battle 
        //else
            //show the battle meny for the mext player 
    }

    private void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
        int damage = currAttacker.Strength; // get damage (can use an algorithm)
        currAttacker.BattleVisuals.PlayAttackAnimation(); // play the attack animation 
        currTarget.CurrHealth -= damage; // deal the damage 
        currTarget.BattleVisuals.PlayHitAnimation(); // play their hit animation 
        currTarget.UpdateUI(); // update the UI 
        bottomText.text = string.Format("{0} attacks {1} for {2} damage", currAttacker.Name, currTarget.Name,damage);
    }
}

[System.Serializable]
public class BattleEntities
{
    public enum Action { Attack, Run}
    public Action BattleAction;

    public string Name;
    public int CurrHealth;
    public int MaxHealth;
    public int Initiative;
    public int Strength;
    public int Level;
    public bool IsPlayer;
    public BattleVisuals BattleVisuals;
    public int Target;

    public void SetEntityValues(string name, int currHealth, int maxHealth, int initiative, int strength, int level, bool isPlayer)
    {
        Name = name;
        CurrHealth = currHealth;
        MaxHealth = maxHealth;
        Initiative = initiative;
        Strength = strength;
        Level = level;
        IsPlayer = isPlayer;
    }

    public void SetTarget(int target)
    {
        Target = target;
    }

    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrHealth);
    }
}
