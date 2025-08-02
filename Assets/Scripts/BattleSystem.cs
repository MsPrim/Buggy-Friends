using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private enum BattleState { Start, Selection, Battle, Won, Lost, Run }

    [Header("Battle State")]
    [SerializeField] private BattleState state;

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
    private const string WIN_MESSAGE = "Your party won the battle!";
    private const int TURN_DURATION = 2;

    // Start is called before the first frame update
    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        //AttackAction(allBattlers[0], allBattlers[1]);
    }

    private IEnumerator BattleRoutine()
    {
        enemySelectionMenu.SetActive(false); // enemy selection menu disabled 
        state = BattleState.Battle; //change our state to the battle state 
        bottomTextPopUp.SetActive(true); //enable our bottom text
        
        //loop through all our battlers 
            //-> do their approriate action 

        for (int i = 0; i < allBattlers.Count; i++)
        {
            switch(allBattlers[i].BattleAction)
            {
                case BattleEntities.Action.Attack:
                    //do the attack
                    //Debug.Log(allBattlers[i].Name + "is attacking: " + allBattlers[allBattlers[i].Target].Name);
                    yield return StartCoroutine(AttackRoutine(i));
                    break;
                case BattleEntities.Action.Run:
                    //run
                    break;
                default:
                    Debug.Log("Error - incorrect battle action");
                    break;
            }
        }

        if (state == BattleState.Battle)
        {
            bottomTextPopUp.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu();
        }

        yield return null;
        //if we haven't won or lost, repeat the loop by opening the battle menu
    }

    private IEnumerator AttackRoutine(int i)
    {
        // players turn 
        if (allBattlers[i].IsPlayer == true)
        {
            // attack selected enemy (attack action)
            BattleEntities currAttacker = allBattlers[i];
            if (allBattlers[currAttacker.Target].IsPlayer == true || currAttacker.Target >= allBattlers.Count) 
            {
                currAttacker.SetTarget(GetRandomEnemy());
            }
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            AttackAction(currAttacker, currTarget);

            // wait a few seconds 
            yield return new WaitForSeconds(TURN_DURATION);

            // kill the enemy 
            if(currTarget.CurrHealth <=0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name); 
                yield return new WaitForSeconds(TURN_DURATION); // wait a few seconds
                enemyBattlers.Remove(currTarget); 
                allBattlers.Remove(currTarget); 

                if(enemyBattlers.Count <=0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION); // wait a few seconds
                    Debug.Log("Go back to overworld scene");
                }
            }

            // if no enemies remain 


            // -> we won the battle 

        }

        // enemies turn 
        // get random party member (target)
        // attack selected party member (attack action)
        // wait a few seconds 
        // kill the party member 

        // if no party members remain 
        // -> we lost the battle 
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
            StartCoroutine(BattleRoutine());
            
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

    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>();// create a temporary list of type int (index)
        // find all party members -> add them to our list 
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == true) // we have a party member 
            {
                partyMembers.Add(i);
            }
        }
        return partyMembers[Random.Range(0, partyMembers.Count)]; //return a random party member
    }

    private int GetRandomEnemy()
    {
        List<int> enemies = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == false)
            {
                enemies.Add(i);
            }
        }
        return enemies[Random.Range(0, enemies.Count)];
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
