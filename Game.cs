using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System;
//using GoogleMobileAds.Api;
using System.Threading;

public class Game : MonoBehaviour
{
    [Header("Счет")]
    public Text scoreText;
    public Text kingScoreText;
    [Header("Панель сообщения")]
    public GameObject WarningObj;
    public Text Warning;
    [Header("Текст для заданий")]
    public Text CostQuest;
    public Text RewardQuest;
    public Text QuestText;
    public Text EndQuest;
    [Header("Задания")]
    public List<Quest> QuestPoint = new List<Quest>();
    [Header("Кнопки заданий")]
    public Button[] questBttns;
    [Header("Панелька заданий")]
    public GameObject QuestPan;
    public GameObject QuestStr;
    public GameObject QuestEnd;
    [Header("Магазин")]
    public List<Item> shopItems = new List<Item>();
    [Header("Текст на кнопках товаров")]
    public Text[] shopItemsText;
    [Header("Кнопки товаров")]
    public Button[] shopBttns;
    [Header("Панелька магазина")]
    public GameObject shopPan;
    [Header("Достижения")]
    public Button[] achivements;
    public GameObject[] oneForAchive;
    public GameObject[] twoForAchive;
    public GameObject[] fhorForAchive;
    public bool[] checkAchive;
    [Header("Сообщения")]
    public GameObject MassegePan;
    public Text MassageText;
    [Header("Панелька таверны")]
    public GameObject TavernPan;
    [Header("Бросок кубика")]
    public Button diceButton;
    public Text diceRoll;
    public Text diceTime;
    [Header("Информация")]
    public GameObject InfoPan;
    [Header("Арена")]
    public List<LevelEnemy> enemyOfArena = new List<LevelEnemy>();
    public Button enemyScin;
    public Slider HealthEnemy;
    public Text HealthText;
    public Button NextBut;
    public GameObject finishPan;
    public GameObject fightPan;
    public GameObject firstPan;
    public bool tiket;
    public Text scoreUp;
    public Text AttackText;

    private float score; //Игровая валюта
    private int kingScore;
    private int scoreIncrease = 1; //Бонус при клике
    private int scoreBonus = 0; //Бонус в секунду
    private int timeBonus = 1;
    private int offlineBonus;
    private Save sv = new Save();
    private int indexQ;
    private float timeOfBonusPS;
    private int indexPS;
    private int timeTavern = 5;
    private int roll;
    private System.Random generator = new System.Random();
    private int timeInSecondsP;
    private int hoursP;
    private int minutsP;
    private int secondsP;
    private bool firstTry = false;
    private string secondText = " секунд)";
    private float healthEnemy = 70;
    private float maxHealthEnemy = 100;
    private int attackHero = 1;
    private int timeEnemy;
    private int coustUp = 1;
    private int quantityAchive;
    

    private void Awake()
    {
        if (PlayerPrefs.HasKey("SV"))
        {
            int totalBonusPS = 0;
            sv = JsonUtility.FromJson<Save>(PlayerPrefs.GetString("SV"));
            score = sv.score;
            kingScore = sv.kingScore;
            scoreBonus = sv.bonusPS;
            firstTry = sv.tryFirst;
            tiket = sv.tiket;
            quantityAchive = sv.quvantAch;
            checkAchive = sv.checkAch;
            if(sv.lvlTavern != 0) timeTavern = sv.lvlTavern;
            for (int i = 0; i < shopItems.Count; i++)
            {
                shopItems[i].levelOfItem = sv.levelOfItem[i];
                shopItems[i].bonusCounter = sv.bonusCounter[i];
                shopItems[i].flagItem = sv.flagItem[i];
                shopItems[i].CheckItem = sv.checkItem[i];
                if (shopItems[i].needCostMultiplier) shopItems[i].cost *= (int)Mathf.Pow(shopItems[i].costMultiplier, shopItems[i].levelOfItem);
                if (shopItems[i].bonusIncrease != 0 && shopItems[i].levelOfItem != 0) scoreIncrease += (int)Mathf.Pow(shopItems[i].bonusIncrease, shopItems[i].levelOfItem);
                totalBonusPS += shopItems[i].bonusPerSec * shopItems[i].bonusCounter;
            }
            for (int i = 0; i < QuestPoint.Count; i++)
            {
                QuestPoint[i].flagQuest = sv.flagQuest[i];
            }
            DateTime dt = new DateTime(sv.date[0], sv.date[1], sv.date[2], sv.date[3], sv.date[4], sv.date[5]);
            //var dateTime = CheckGlobalTime();
            TimeSpan ts = DateTime.Now - dt;
            int deltaSeconds = sv.secondsTimer - (int)ts.TotalSeconds;
            if(deltaSeconds > 0)
            {
                Debug.Log((int)ts.TotalSeconds);
                Debug.Log(deltaSeconds);
                hoursP = deltaSeconds / 3600;
                timeInSecondsP = deltaSeconds % 3600;
                //secondsP = inSecondsTime % 60;
                //minutsP = inSecondsTime / 60;
                diceButton.interactable = false;
                StartCoroutine(Timer());
            }
            offlineBonus = (int)ts.TotalSeconds * totalBonusPS / timeTavern;
            score += offlineBonus;
            MassageText.text = ("Вы отсутствовали: \n" + ts.Days + "Д. " + ts.Hours + "Ч. " + ts.Minutes + "М. " + ts.Seconds + "С." + "\n" +
            "Ваши рабочие заработали: \n" + offlineBonus + "$");
            
        }
        
        
        //Debug.Log(shopItems.Count);
        
    }

    DateTime CheckGlobalTime()
    {
        //var www = new WWW("https://google.com");
        UnityWebRequest www = UnityWebRequest.Get("https://google.com");
        //while (!www.isDone && www.error == null)
        //    Thread.Sleep(1);

        var str = www.GetResponseHeader("Date");
        DateTime dateTime;

        if (!DateTime.TryParse(str, out dateTime))
            return DateTime.Now;

        return dateTime.ToUniversalTime();
    }

    private void Start()
    {
        if(offlineBonus != 0) MassegePan.SetActive(true);
        if (firstTry == false)
        {
            InfoPan.SetActive(true);
            firstTry = true;
        }
        if(tiket == true)
        {
            firstPan.SetActive(false);
        }
        kingScoreText.text = kingScore + "";
        
        updateCosts(); //Обновить текст с ценами
        updateQuest(); //обновить выполненные задания
        StartCoroutine(BonusPerSec()); //Запустить просчёт бонуса в секунду
    }

    private void Update()
    {
        //scoreText.text = score + ""; //Отображаем деньги
        if(score > 100) scoreText.text =  String.Format("{0:0,0}", score);
        else scoreText.text = score + "";
    }

    public void startTimer()
    {
        hoursP = 11;
        timeInSecondsP = 3600;
        diceButton.interactable = false;
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while (true) // Бесконечный цикл
        {
            timeInSecondsP -= 1;
            secondsP = timeInSecondsP % 60;
            minutsP = timeInSecondsP / 60;
            if (minutsP == 0 && secondsP == 0)
            {
                hoursP--;
                timeInSecondsP = 3600;
            }
            if (hoursP == 0)
            {
                diceTime.text = "Бесплатно";
                diceButton.interactable = true;
                break;
            }
            diceTime.text = hoursP + ":" + minutsP;
            yield return new WaitForSeconds(1); // Делаем задержку в 1 секунду
        }
        
    }

    public void QuestBut(int index)
    {
        QuestText.text = QuestPoint[index].Text;
        EndQuest.text = QuestPoint[index].EndText;
        CostQuest.text = "Стоимость: " + QuestPoint[index].cost + " gold";
        RewardQuest.text = "Награда: " + QuestPoint[index].nameReward;
        indexQ = index;
        if (QuestPoint[index].flagQuest == true)
        {
            QuestEnd.SetActive(true);
        }
        else QuestStr.SetActive(true);
    }

    public void QuestBuy()
    {
        if (QuestPoint[indexQ].cost <= score)
        {
            Debug.Log(indexQ);
            score -= QuestPoint[indexQ].cost;
            scoreIncrease += QuestPoint[indexQ].number;

            QuestEnd.SetActive(true);
            QuestStr.SetActive(false);
            QuestPoint[indexQ].flagQuest = true;
            //questReward[indexQ].SetActive(true);
            if (indexQ < 4) questBttns[indexQ + 1].GetComponent<Button>().interactable = true;
            else 
            { 
                //QuestComplited = true;
                Warning.text = ("Поздравляю! Все задания выполнены");
                WarningObj.SetActive(true);
                StartCoroutine(NullWar());
            }

            if(QuestPoint[1].flagQuest == true) CheckAchive(1);
            if (QuestPoint[5].flagQuest == true) CheckAchive(3);
        }
        else
        {
            Warning.text = ("Нужно больше золота!");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
    }

    public void updateQuest()
    {
        for(int i = 0; i < questBttns.Length - 1; i++)
        {
            if(QuestPoint[i].flagQuest == true) 
                questBttns[i + 1].GetComponent<Button>().interactable = true;
        }
    }

    public void BuyBttn(int index) //Метод при нажатии на кнопку покупки товара (индекс товара)
    {
        int cost = shopItems[index].cost * scoreBonus; //Рассчитываем цену в зависимости от кол-ва рабочих (к примеру)
        if(shopItems[index].levelOfItem >= shopItems[index].maxLevelOfItem)
        {
            Warning.text = ("Достигнуто максимальное улучшение");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
        else if(shopItems[index].levelOfItem >= 5 && timeTavern == 5)
        {
            Warning.text = ("Достигнуто максимальное улучшение на этом уровне таверны");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
        else if (shopItems[index].levelOfItem >= 10 && timeTavern == 4)
        {
            Warning.text = ("Достигнуто максимальное улучшение на этом уровне таверны");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
        else if (shopItems[index].levelOfItem >= 15 && timeTavern == 3)
        {
            Warning.text = ("Достигнуто максимальное улучшение на этом уровне таверны");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
        else if (shopItems[index].levelOfItem >= 20 && timeTavern == 2)
        {
            Warning.text = ("Достигнуто максимальное улучшение на этом уровне таверны");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
        else
        {
            if (shopItems[index].itsBonus) //Если товар нажатой кнопки - это бонус, и денег >= цены(е)
            {


                timeOfBonusPS = shopItems[index].timeOfBonus;
                indexPS = index;
                //тут была реклама
                //score -= cost; // Вычитаем цену из денег
                //StartCoroutine(BonusTimer(shopItems[index].timeOfBonus, index)); //Запускаем бонусный таймер

            }
            else if (score >= shopItems[index].cost) // Иначе, если товар нажатой кнопки - это не бонус, и денег >= цена
            {
                if (shopItems[index].itsItemPerSec)
                {
                    shopItems[index].bonusCounter++;// Если нанимаем рабочего (к примеру), то прибавляем кол-во рабочих
                    scoreBonus++;
                }
                else scoreIncrease += shopItems[index].bonusIncrease; // Иначе бонусу при клике добавляем бонус товара
                score -= shopItems[index].cost; // Вычитаем цену из денег
                if (shopItems[index].needCostMultiplier) shopItems[index].cost *= shopItems[index].costMultiplier; // Если товару нужно умножить цену, то умножаем на множитель
                shopItems[index].levelOfItem++; // Поднимаем уровень предмета на 1
                if (shopItems[index].levelOfItem > 5) shopItems[index].costMultiplier = 3;
            }
            else
            {
                Warning.text = ("Нужно больше золота!"); // Иначе если 2 проверки равны false, то выводим в консоль текст.
                WarningObj.SetActive(true);
                StartCoroutine(NullWar());
            }
            updateCosts(); //Обновить текст с ценами
        }
        
    }
    private void updateCosts() // Метод для обновления текста с ценами
    {
        for (int i = 0; i < shopItems.Count; i++) // Цикл выполняется, пока переменная i < кол-ва товаров
        {
            if (shopItems[i].itsBonus) // Если товар является бонусом, то:
            {
                //int cost = shopItems[i].cost * scoreBonus; // Рассчитываем цену в зависимости от кол-ва рабочих (к примеру)
                shopItemsText[i].text = shopItems[i].name; // Обновляем текст кнопки с рассчитанной ценой
            }
            else if(shopItems[i].itsItemPerSec) shopItemsText[i].text = shopItems[i].name + timeTavern + secondText + "\n" + String.Format("{0:0,0}", shopItems[i].cost) + " gold";
            else shopItemsText[i].text = shopItems[i].name + "\n" + String.Format("{0:0,0}", shopItems[i].cost) + " gold"; // Иначе если товар не является бонусом, то обновляем текст кнопки с обычной ценой
        }
    }

    public void UpdateTavern(int index)
    {
        //int cost = shopItems[index].cost * scoreBonus; //Рассчитываем цену в зависимости от кол-ва рабочих (к примеру)
        
        //if (shopItems[index].needCostMultiplier == true) cost = shopItems[index].cost * (shopItems[index].levelOfItem + 1);
        
        
        int cost = shopItems[index].cost;
        //Debug.Log(shopItems[index].levelOfItem);
        if (score >= cost)
        {
            score -= cost;
            if (timeTavern > 2)
            {
                timeTavern -= 1;
                secondText = " секунды)";
            }
            else
            {
                shopBttns[index].interactable = false;
                secondText = " секунда)";
                timeTavern = 1;
                CheckAchive(5);
            }

            if (timeTavern == 4)
            {
                CheckAchive(4);
            }
            shopItems[index].levelOfItem++;
            if (shopItems[index].needCostMultiplier) shopItems[index].cost *= shopItems[index].costMultiplier;
            updateCosts();
            //Refresh();
        }
        else
        {
            Warning.text = ("Не хватает денег!"); // Иначе если 2 проверки равны false, то выводим в консоль текст.
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
            
        Debug.Log(timeTavern);
    }

    public void CheckAchive(int index)
    {
        Warning.text = ("Достижение получено");
        WarningObj.SetActive(true);
        StartCoroutine(NullWar());
        achivements[index].interactable = true;
        fhorForAchive[index].SetActive(false);
        oneForAchive[index].SetActive(true);
        twoForAchive[index].SetActive(true);
        checkAchive[index] = true;
        Achive();
    }

    public void Achive()
    {
        quantityAchive = 0;
        for(int i = 1; i < achivements.Length; i++)
        {
            if (achivements[i].interactable == true) quantityAchive++;

        }
        if (quantityAchive == achivements.Length - 1) CheckAchive(0);
    }

    public void DiceRollBttn()
    {
        StartCoroutine(Roll());
    }

    public void DiceRollCmpl()
    {
        if (roll > 1 && roll <= 10) score -= roll * 100;
        else if (roll > 10 && roll < 20) score += roll * 100;
        else if (roll == 20) score += 10000;
        else if (roll == 1)
        {
            score -= 10000;
            CheckAchive(2);
        }
        
    }

    IEnumerator Roll()
    {
        for (int i = 0; i < 10; i++)
        {
            roll = generator.Next(1, 21);
            diceRoll.text = roll + "";
            yield return new WaitForSeconds(0.2f);
        }
        DiceRollCmpl();
    }

    public void Refresh()
    {
        score = 100000;
        scoreIncrease = 1;
        scoreBonus = 0;
        timeTavern = 5;
        for (int i = 0; i < shopItems.Count - 1; i++)
        {
            shopItems[i].levelOfItem = 0;
            shopItems[i].bonusCounter = 0;
            shopItems[i].cost = shopItems[i].startCost;
        }
        //for (int i = 0; i < QuestPoint.Count; i++)
        //{
        //    QuestPoint[i].flagQuest = false;
        //}
        updateCosts();
    }

    IEnumerator NullWar()
    {
        yield return new WaitForSeconds(2);
        WarningObj.SetActive(false);
        Warning.text = "";
    }

    IEnumerator BonusPerSec() // Бонус в секунду
    {
        while (true) // Бесконечный цикл
        {

            score += (timeBonus * scoreBonus);
            
            yield return new WaitForSeconds(timeTavern); // Делаем задержку в 5 секунду
        }
    }
    IEnumerator BonusTimer(float time, int index) // Бонусный таймер (длительность бонуса (в сек.),индекс товара)
    {
        shopBttns[index].interactable = false; // Выключаем кликабельность кнопки бонуса
        timeBonus *= 2; // Удваиваем бонус рабочих в секунду (к примеру)
        yield return new WaitForSeconds(time); // Делаем задержку на столько секунд, сколько указали в параметре
        timeBonus /= 2; // Возвращаем бонус в нормальное состояние
        shopBttns[index].interactable = true; // Включаем кликабельность кнопки бонуса
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            sv.score = score;
            sv.kingScore = kingScore;
            sv.bonusPS = scoreBonus;
            int seconds = (hoursP * 3600) + timeInSecondsP;
        sv.secondsTimer = seconds;
        sv.tryFirst = firstTry;
        sv.tiket = tiket;
        sv.quvantAch = quantityAchive;
        sv.lvlTavern = timeTavern;
        sv.checkAch = new bool[checkAchive.Length];
        sv.levelOfItem = new int[shopItems.Count];
        sv.bonusCounter = new int[shopItems.Count];
        sv.flagItem = new bool[shopItems.Count];
        sv.checkItem = new bool[shopItems.Count];
        sv.flagQuest = new bool[QuestPoint.Count];
        for (int i = 0; i < shopItems.Count; i++)
        {
            sv.levelOfItem[i] = shopItems[i].levelOfItem;
            sv.bonusCounter[i] = shopItems[i].bonusCounter;
            sv.flagItem[i] = shopItems[i].flagItem;
            sv.checkItem[i] = shopItems[i].CheckItem;
        }
        for (int i = 0; i < QuestPoint.Count; i++)
        {
            sv.flagQuest[i] = QuestPoint[i].flagQuest;
        }
        sv.date[0] = DateTime.Now.Year; sv.date[1] = DateTime.Now.Month; sv.date[2] = DateTime.Now.Day; sv.date[3] = DateTime.Now.Hour; sv.date[4] = DateTime.Now.Minute; sv.date[5] = DateTime.Now.Second;
        PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
        }
    }
#else
    private void OnApplicationQuit()
    {
        sv.score = score;
        sv.kingScore = kingScore;
        sv.bonusPS = scoreBonus;
        int seconds = (hoursP * 3600) + timeInSecondsP;
        sv.secondsTimer = seconds;
        sv.tryFirst = firstTry;
        sv.tiket = tiket;
        sv.quvantAch = quantityAchive;
        sv.lvlTavern = timeTavern;
        sv.checkAch = new bool[checkAchive.Length];
        sv.levelOfItem = new int[shopItems.Count];
        sv.bonusCounter = new int[shopItems.Count];
        sv.flagItem = new bool[shopItems.Count];
        sv.checkItem = new bool[shopItems.Count];
        sv.flagQuest = new bool[QuestPoint.Count];
        for (int i = 0; i < shopItems.Count; i++)
        {
            sv.levelOfItem[i] = shopItems[i].levelOfItem;
            sv.bonusCounter[i] = shopItems[i].bonusCounter;
            sv.flagItem[i] = shopItems[i].flagItem;
            sv.checkItem[i] = shopItems[i].CheckItem;
        }
        for (int i = 0; i < QuestPoint.Count; i++)
        {
            sv.flagQuest[i] = QuestPoint[i].flagQuest;
        }
        sv.date[0] = DateTime.Now.Year; sv.date[1] = DateTime.Now.Month; sv.date[2] = DateTime.Now.Day; sv.date[3] = DateTime.Now.Hour; sv.date[4] = DateTime.Now.Minute; sv.date[5] = DateTime.Now.Second;
        PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
    }
#endif

    public void showShopPan() 
    {
        shopPan.SetActive(!shopPan.activeSelf); 
    }

    public void showMessPan() 
    {
        MassegePan.SetActive(!MassegePan.activeSelf); 
    }

    public void showQuestPan() 
    {
        QuestPan.SetActive(!QuestPan.activeSelf); 
    }

    public void showTavernPan() 
    {
        TavernPan.SetActive(!TavernPan.activeSelf); 
    }

    public void InfoTavPan() 
    {
        InfoPan.SetActive(!InfoPan.activeSelf);
    }

    public void OnClick() 
    {
        score += scoreIncrease; // К игровой валюте прибавляем бонус при клике
    }

    public void startArena(int level)
    {
        if (score >= 100)
        {
            score -= 100;
            int index = generator.Next(0, enemyOfArena[level].enemyArena.Count);
            maxHealthEnemy = enemyOfArena[level].enemyArena[index].health;
            healthEnemy = enemyOfArena[level].enemyArena[index].health;
            enemyScin.image.sprite = enemyOfArena[level].enemyArena[index].skin;
            HealthEnemy.value = 1;
            HealthText.text = healthEnemy + "";
        }
        else
        {
            Warning.text = ("Нужно больше золота!");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
    }

    public void attackEnemy()
    {
        Debug.Log(maxHealthEnemy);
        Debug.Log(healthEnemy);
        healthEnemy -= attackHero;
        HealthEnemy.value = healthEnemy / maxHealthEnemy;
        HealthText.text = healthEnemy + "";
        if (healthEnemy <= 0 && timeEnemy < 2)
        {
            timeEnemy++;
            enemyScin.gameObject.SetActive(false);
            HealthEnemy.gameObject.SetActive(false);
            HealthText.gameObject.SetActive(false);
            NextBut.gameObject.SetActive(true);
        }
        else if(healthEnemy <= 0 && timeEnemy == 2)
        {
            timeEnemy = 0;
            fightPan.SetActive(false);
            finishPan.SetActive(true);
        }
    }

    public void NextEnemy()
    {
            enemyScin.gameObject.SetActive(true);
            HealthEnemy.gameObject.SetActive(true);
            HealthText.gameObject.SetActive(true);
            NextBut.gameObject.SetActive(false);
            startArena(timeEnemy);
    }

    public void buyTiket()
    {
        if(score >= 500)
        {
            score -= 500;
            firstPan.SetActive(false);
            tiket = true;
        }
        else
        {
            Warning.text = ("Нужно больше золота!");
            WarningObj.SetActive(true);
            StartCoroutine(NullWar());
        }
    }

    public void winArena()
    {
        kingScore += 1;
        kingScoreText.text = kingScore + "";
    }

    public void UpAtt()
    {
        if(kingScore >= coustUp)
        {
            kingScore -= coustUp;
            attackHero += 1;
            coustUp *= 2;
            if(coustUp < 5) scoreUp.text = coustUp + " короны";
            else if(coustUp >= 5) scoreUp.text = coustUp + " корон";
            AttackText.text = "сила бойца: " + attackHero;
            kingScoreText.text = kingScore + "";
        }
    }

    

}
[Serializable]
public class Item // Класс товара
{
    [Tooltip("Название используется на кнопках")]
    [TextArea]public string name;
    public int maxLevelOfItem;
    [Tooltip("Цена товара")]
    public int cost;
    public int startCost;
    [Tooltip("Бонус, который добавляется к бонусу при клике")]
    public int bonusIncrease;
    [HideInInspector]
    public int levelOfItem; // Уровень товара
    [Space]
    [Tooltip("Нужен ли множитель для цены?")]
    public bool needCostMultiplier;
    [Tooltip("Множитель для цены")]
    public int costMultiplier;
    [Space]
    [Tooltip("Этот товар даёт бонус в секунду?")]
    public bool itsItemPerSec;
    [Tooltip("Бонус, который даётся в секунду")]
    public int bonusPerSec;
    [HideInInspector]
    public int bonusCounter; // Кол-во рабочих (к примеру)
    [Space]
    [Tooltip("Это временный бонус?")]
    public bool itsBonus;
    [Tooltip("Множитель товара, который управляется бонусом (Умножается переменная bonusPerSec)")]
    public int itemMultiplier;
    [Tooltip("Длительность бонуса")]
    public float timeOfBonus;
    [Space]
    [Tooltip("Это одноразовый бонус?")]
    public bool CheckItem;
    public bool flagItem;
}
[Serializable]
public class Quest
{
    [Tooltip("Название используется на кнопках")]
    public string name;
    [Tooltip("Текст задания")]
    [TextArea] public string Text;
    [Tooltip("Стоимость задания")]
    public int cost;
    [Tooltip("Текст завершения задания")]
    [TextArea] public string EndText;
    [Tooltip("бонус к клику")]
    public int number;
    [Tooltip("Название награды")]
    public string nameReward;
    [Tooltip("Проверка выполнения")]
    public bool flagQuest;
}

[Serializable]
public class Enemy
{
    public int health;
    public Sprite skin;
}

[Serializable]
public class LevelEnemy
{
    public List<Enemy> enemyArena = new List<Enemy>();
}

[Serializable]
public class Save
{
    public float score;
    public int kingScore;
    public int bonusPS;
    public int secondsTimer;
    public bool tryFirst;
    public bool tiket;
    public int quvantAch;
    public int lvlTavern;
    public bool[] checkAch;
    public int[] levelOfItem;
    public int[] bonusCounter;
    public bool[] flagItem;
    public bool[] checkItem;
    public bool[] flagQuest;
    public int[] date = new int[6];
}
