using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST}

public class battleSystem : MonoBehaviour
{
    public battleHUD playerHUD;
    public battleHUD enemyHUD;

    public BattleState state;

    public Text enemyNameText;
    public Text DialogText;
    public Transform enemySpawnPt;
    public Transform playerSpawnPt;

    public unit playerUnit;
    public unit enemyUnit;

    public GameObject enemyprefab;
    public GameObject playerprefab;


    //the balancing data desing numbers
    public float BalanceAtkPHVal = 1f;
    public float BalanceAtkJoyVal = 1f;
    public float BalanceAtkMeaningVal = 1f;

    //the balancing data desing numbers
    public float BalanceDefPHVal = 1f;
    public float BalanceDefJoyVal = 1f;
    public float BalanceDefMeaningVal = 1f;

    public CardSystem cardsystem;
    public Button drawbutton;

    public Text PAtkModtxtVal;
    public Text PDefModtxtVal;
    public Text PHPtxtVal;

    public Text EAtkModtxtVal;
    public Text EDefModtxtVal;
    public Text EHPtxtVal;

    public Image playerHUDFlash;
    public Image enemyHUDFlash;

    string sceneName;
    public EnemyAI AIscript;

    public GameObject CardDisplayArea;


    //SETTING UP AND START STATE------------------------------------------------------------
    void Start()
    {
        state = BattleState.START;
        
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        StartCoroutine(setupBattle());
    }

    IEnumerator setupBattle()
    {
        GameObject playerGO = Instantiate(playerprefab, playerSpawnPt);
        playerUnit = playerGO.GetComponent<unit>();

        GameObject enemyGO = Instantiate(enemyprefab, enemySpawnPt);
        enemyUnit = enemyGO.GetComponent<unit>();

        enemyNameText.text = enemyUnit.UnitName;

        AIscript = enemyGO.GetComponent<EnemyAI>();

        //change enemy name
        if (sceneName == "1_level") { enemyUnit.UnitName = "Domenic"; enemyUnit.currentDefMod = 2; }
        if (sceneName == "2_level") { enemyUnit.UnitName = "Jake"; enemyUnit.currentDefMod = 2; }
        if (sceneName == "3_level") { enemyUnit.UnitName = "Vivian the Destroyer"; enemyUnit.currentAtkMod = 2; }

        playerHUD.setHUD(playerUnit);
        enemyHUD.setHUD(enemyUnit);
        DialogText.text = "lifes hand";

        yield return new WaitForSeconds(1f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    private void Update()
    {
        if (state != BattleState.PLAYERTURN) { drawbutton.interactable = false; } else { drawbutton.interactable = true; }
        PAtkModtxtVal.text = ""+playerUnit.currentAtkMod;
        PHPtxtVal.text = "" + playerUnit.currentHP;
        PDefModtxtVal.text = "" + playerUnit.currentDefMod;

        EAtkModtxtVal.text = "" + enemyUnit.currentAtkMod;
        EHPtxtVal.text = "" + enemyUnit.currentHP;
        EDefModtxtVal.text = "" + enemyUnit.currentDefMod;
    }
    

    //ENEMYTURN STATE---------------------------------------------------------------------
    IEnumerator EnemyTurn()
    {
        DialogText.text = "Enemy's turn";
        yield return new WaitForSeconds(1f);
        //do things

        //call function in enemyAI
        AIscript.EnemyLevelCheck();
    }

//PLAYERTURN STATE--------------------------------------------------------------

    public void PlayerTurn() // can play card to attack or heal
    {
        DialogText.text = "Player 1's Turn";
    }

    IEnumerator DrawCard()
    {
        //draw card and add to deck on cardsystem script
        Debug.Log("cross call worked");
        state = BattleState.ENEMYTURN;
        yield return new WaitForSeconds(1f);
        StartCoroutine(EnemyTurn());
    }

    public void OnDrawButton() //draw card and end turn
    {
        StartCoroutine(DrawCard());
    }

//Attack and defens play funcitons---------------------------------------------------------------
//CALLED BY CARDUNIT ATKCARDUSED() which is called by button press
    public void OnAttackCard(float PlayerHPVal, float PlayerDefModVal, float PlayerAtkModVal, GameObject card, float EnemyHPVal, float EnemyDefModVal, float EnemyAtkModVal)
    {
        //what state we in?
        //if (state == BattleState.PLAYERTURN)//player attacking
       // {
            StartCoroutine(DisplayCard(card));

            StartCoroutine(PlayerAttack(PlayerHPVal,PlayerDefModVal,PlayerAtkModVal,EnemyHPVal,EnemyDefModVal,EnemyAtkModVal));
            //Destroy(card);
            return;
       // }
    }
    //PlayerHPVal, PlayerDefModVal, PlayerAtkModVal

    public void EnemyCardUsed(float PlayerHPVal, float PlayerDefModVal, float PlayerAtkModVal, GameObject card, float EnemyHPVal, float EnemyDefModVal, float EnemyAtkModVal)
    {
        // Destroy(card);
        StartCoroutine(DisplayCard(card));
        StartCoroutine(EnemyAttack(PlayerHPVal, PlayerDefModVal, PlayerAtkModVal, EnemyHPVal, EnemyDefModVal, EnemyAtkModVal));
    }

    //CALLED BY ON ATTACKCARDUSED()
    IEnumerator PlayerAttack(float PlayerHPVal, float PlayerDefModVal, float PlayerAtkModVal, float EnemyHPVal, float EnemyDefModVal, float EnemyAtkModVal)
    {
        //do attack change Enemy stats 
        //convention card Enemyvals affect enemy
        enemyUnit.currentHP = Mathf.Round(enemyUnit.currentHP + EnemyHPVal*playerUnit.currentAtkMod/enemyUnit.currentDefMod);

        enemyUnit.currentAtkMod = enemyUnit.currentAtkMod + EnemyAtkModVal;
        enemyUnit.currentDefMod = enemyUnit.currentDefMod + EnemyDefModVal;

        if (enemyUnit.currentAtkMod < 1) { enemyUnit.currentAtkMod = 1; }
        if (enemyUnit.currentDefMod < 1) { enemyUnit.currentDefMod = 1; }

        if (enemyUnit.currentAtkMod > enemyUnit.maxAtkMod) { enemyUnit.currentAtkMod = enemyUnit.maxAtkMod; }
        if (enemyUnit.currentDefMod > enemyUnit.maxDefMod) { enemyUnit.currentDefMod = enemyUnit.maxDefMod; }

        if (enemyUnit.currentHP > enemyUnit.maxHP) { enemyUnit.currentHP = enemyUnit.maxHP; }
        if (enemyUnit.currentHP < 0) { enemyUnit.currentHP = 0; }

        enemyHUD.AtkModSlider.value = enemyUnit.currentAtkMod;
        enemyHUD.DefModSlider.value = enemyUnit.currentDefMod;
        enemyHUD.HPSlider.value = enemyUnit.currentHP;

        //do attack changing  Player stats    
        playerUnit.currentHP = Mathf.Round(playerUnit.currentHP + PlayerHPVal);

        playerUnit.currentAtkMod = playerUnit.currentAtkMod + PlayerAtkModVal;
        playerUnit.currentDefMod = playerUnit.currentDefMod + PlayerDefModVal;
 

        if (playerUnit.currentAtkMod < 1) { playerUnit.currentAtkMod = 1; }
        if (playerUnit.currentDefMod < 1) { playerUnit.currentDefMod = 1; }

        if (playerUnit.currentAtkMod > playerUnit.maxAtkMod) { playerUnit.currentAtkMod = playerUnit.maxAtkMod; }
        if (playerUnit.currentDefMod > playerUnit.maxDefMod) { playerUnit.currentDefMod = playerUnit.maxDefMod; }

        if (playerUnit.currentHP > playerUnit.maxHP) { playerUnit.currentHP = playerUnit.maxHP; }
        if (playerUnit.currentHP < 0) { playerUnit.currentHP = 0; }

        playerHUD.AtkModSlider.value = playerUnit.currentAtkMod;
        playerHUD.DefModSlider.value = playerUnit.currentDefMod;
        playerHUD.HPSlider.value = playerUnit.currentHP;


        playerHUDFlash.GetComponent<Image>().color = Color.yellow;
        enemyHUDFlash.GetComponent<Image>().color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        playerHUDFlash.GetComponent<Image>().color = Color.blue;
        enemyHUDFlash.GetComponent<Image>().color = Color.red;


        //check if dead
        if (enemyUnit.isDead() == true)
        {
            state = BattleState.WON;
            StartCoroutine(WonFunction());
        }
        else if (playerUnit.isDead() == true)
        {
            state = BattleState.LOST;
            StartCoroutine(LostFunction());
        } else if (enemyUnit.isDead() == false || playerUnit.isDead() == false)
        {
            state = BattleState.ENEMYTURN;
            yield return new WaitForSeconds(2f);
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyAttack(float PlayerHPVal, float PlayerDefModVal, float PlayerAtkModVal, float EnemyHPVal, float EnemyDefModVal, float EnemyAtkModVal)
    {
        //convention: enemy Vals affect player if enemy playing card

        //do attack changing Enemy player stats 
        enemyUnit.currentHP = Mathf.Round(enemyUnit.currentHP + PlayerHPVal);

        enemyUnit.currentAtkMod = enemyUnit.currentAtkMod + PlayerAtkModVal;
        enemyUnit.currentDefMod = enemyUnit.currentDefMod + PlayerDefModVal;

        if (enemyUnit.currentAtkMod < 1) { enemyUnit.currentAtkMod = 1; }
        if (enemyUnit.currentDefMod < 1) { enemyUnit.currentDefMod = 1; }

        if (enemyUnit.currentAtkMod > enemyUnit.maxAtkMod) { enemyUnit.currentAtkMod = enemyUnit.maxAtkMod; }
        if (enemyUnit.currentDefMod > enemyUnit.maxDefMod) { enemyUnit.currentDefMod = enemyUnit.maxDefMod; }

        if (enemyUnit.currentHP > enemyUnit.maxHP) { enemyUnit.currentHP = enemyUnit.maxHP; }
        if (enemyUnit.currentHP < 0) { enemyUnit.currentHP = 0; }

        enemyHUD.AtkModSlider.value = enemyUnit.currentAtkMod;
        enemyHUD.DefModSlider.value = enemyUnit.currentDefMod;
        enemyHUD.HPSlider.value = enemyUnit.currentHP;

        //do attack changing  Player stats   
        //use attack mod to increase stats
        //use player defense mode to decrease
        playerUnit.currentHP = Mathf.Round(playerUnit.currentHP + EnemyHPVal* enemyUnit.currentAtkMod / playerUnit.currentDefMod);

        playerUnit.currentAtkMod = playerUnit.currentAtkMod + EnemyAtkModVal;
        playerUnit.currentDefMod = playerUnit.currentDefMod + EnemyDefModVal;

        if (playerUnit.currentAtkMod < 1) { playerUnit.currentAtkMod = 1; }
        if (playerUnit.currentDefMod < 1) { playerUnit.currentDefMod = 1; }

        if (playerUnit.currentAtkMod > playerUnit.maxAtkMod) { playerUnit.currentAtkMod = playerUnit.maxAtkMod; }
        if (playerUnit.currentDefMod > playerUnit.maxDefMod) { playerUnit.currentDefMod = playerUnit.maxDefMod; }

        if (playerUnit.currentHP > playerUnit.maxHP) { playerUnit.currentHP = playerUnit.maxHP; }
        if (playerUnit.currentHP < 0) { playerUnit.currentHP = 0; }

        playerHUD.AtkModSlider.value = playerUnit.currentAtkMod;
        playerHUD.DefModSlider.value = playerUnit.currentDefMod;
        playerHUD.HPSlider.value = playerUnit.currentHP;

        playerHUDFlash.GetComponent<Image>().color = Color.yellow;
        enemyHUDFlash.GetComponent<Image>().color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        playerHUDFlash.GetComponent<Image>().color = Color.blue;
        enemyHUDFlash.GetComponent<Image>().color = Color.red;

        //check if dead
        if (enemyUnit.isDead() == true)
        {
            state = BattleState.WON;
            StartCoroutine(WonFunction());
        }
        else if (playerUnit.isDead() == true)
        {
            state = BattleState.LOST;
            StartCoroutine(LostFunction());
        }
        else if (enemyUnit.isDead() == false || playerUnit.isDead() == false)
        {
            state = BattleState.PLAYERTURN;
            yield return new WaitForSeconds(2f);
            PlayerTurn();
        }

    }
    //-------------------------------------------------------------------------------------------------------------------------

    IEnumerator DisplayCard(GameObject card)
    {

        if (state == BattleState.PLAYERTURN)
        { 
            card.transform.parent = CardDisplayArea.transform;
            card.transform.position = new Vector3(0f, 0f, 0f);
            CardDisplayArea.transform.localScale = new Vector3(1.5f, 1.5f, 0.2f);
            card.GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            card.transform.parent = CardDisplayArea.transform;
            card.transform.Rotate(180, 0, 0, Space.Self);
            card.transform.position = new Vector3(0f, 0f, 0f);
            card.transform.localScale = new Vector3(1f, 1f, 0.2f);
            CardDisplayArea.transform.localScale = new Vector3(1.5f, 1.5f, 0.2f);
        }

        yield return new WaitForSeconds(2f);
        Destroy(card);
    }


    //WON STATE---------------------------------------------------------------------------
    IEnumerator WonFunction()
    {
        DialogText.text = "You Won! You have defeaten " + enemyUnit.UnitName + " You can advance.";
        yield return new WaitForSeconds(5f);
        //go to next scene
        if (sceneName == "1_level") { SceneManager.LoadScene("2_level"); }
        else if (sceneName == "2_level") { SceneManager.LoadScene("3_level"); }
        else if (sceneName == "3_level") { SceneManager.LoadScene("1_level"); }
    }

//LOST STATE------------------------------------------------------------------------
    IEnumerator LostFunction()
    {
        DialogText.text = "You Lost! You have to defeat "+enemyUnit.UnitName+" to advance.";
        yield return new WaitForSeconds(5f);
        //reset scene
        Application.LoadLevel(Application.loadedLevel);
    }

}
