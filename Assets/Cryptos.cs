using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.SimpleAndroidNotifications;

public class Cryptos : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Sprite> sprites;
    public GameObject crypto;
    public GameObject content;
    public GameObject tradeFunctionsObject;
    public GameObject totalUSDText;
    public Button turnBackButton;
    public Button walletButton;
    public GameObject warningMessage;
    public Sprite acceptTick;
    public Sprite cancelTick;
    public GameObject resetMessage;

    private float[] holdPrice;
    private static int indexOfChild;
    private static string buyOrSell;
    private float resultPrice;
    private static float tempPositionx;

    private void Start()
    {
        //PlayerPrefs.DeleteAll();
        holdPrice = new float[sprites.Count];
        tempPositionx = crypto.gameObject.transform.position.x;
        InitiliazeCryptos();
        InitializeTotalUSD();
        StartCoroutine(UpdatePrices());

        if (PlayerPrefs.HasKey("Time"))
        {
            var timeText = GameObject.Find("Date Time");
            if (timeText != null)
            {
                timeText.GetComponent<TextMeshProUGUI>().text = "Start Time: " + PlayerPrefs.GetString("Time");
            }
        }

    }


    private void InitializeTotalUSD()
    {

        if (!PlayerPrefs.HasKey("USD"))
        {
            PlayerPrefs.SetFloat("USD", 1000000);
        }
        else
        {
            totalUSDText.GetComponent<TextMeshProUGUI>().text = "Loading...";
            totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            return;
        }

        totalUSDText.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat("USD") + " $ ";
    }

    private float CalculationOfProfit(string walletMoney)
    {
        var data = walletMoney.Split(' ');
        var systemMoney = 1000000;
        var yourMoney = float.Parse(data[0]);
        var profit = systemMoney - yourMoney;

        return profit;
    }


    private void Update()
    {

    }

    private void InitiliazeCryptos()
    {

        if (content.transform.childCount > 0)
        {
            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
        }

        var parents = content.GetComponent<RectTransform>();
        var positionY = 400f;
        for (int i = 0; i < sprites.Count; i++)
        {
            var t = Instantiate(crypto, new Vector3(crypto.transform.position.x, positionY), Quaternion.identity);
            t.GetComponent<Image>().sprite = sprites[i];
            t.GetComponentInChildren<TextMeshProUGUI>().text = GetUSDPrice(sprites[i].name) + " $";
            t.gameObject.transform.GetChild(t.gameObject.transform.childCount - 2).name = "Buy " + i.ToString();
            t.gameObject.transform.GetChild(t.gameObject.transform.childCount - 1).GetComponent<Button>().name = "Sell " + i.ToString();
            t.GetComponent<RectTransform>().SetParent(parents);
            t.gameObject.SetActive(true);
            positionY -= 100;

        }
    }

    private IEnumerator UpdatePrices()
    {
        yield return new WaitForSeconds(5f);


        var currentUSD = PlayerPrefs.GetFloat("USD");
        if (content.transform.childCount > 0)
        {
            var i = 0;
            foreach (Transform child in content.transform)
            {
                var nameCrypto = child.GetComponent<Image>().sprite.name;
                child.GetComponentInChildren<TextMeshProUGUI>().text = GetUSDPrice(nameCrypto) + " $";

                if (PlayerPrefs.HasKey(nameCrypto))
                {
                    currentUSD += PlayerPrefs.GetFloat(nameCrypto) * float.Parse(GetUSDPrice(nameCrypto));
                }

                var price = child.GetComponentInChildren<TextMeshProUGUI>().text.Split(' ');
                if (!float.IsNaN(holdPrice[i]))
                {
                    if (holdPrice[i] > float.Parse(price[0]))
                        child.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                    else if (holdPrice[i] < float.Parse(price[0]))
                        child.GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
                }
                holdPrice[i] = float.Parse(price[0]);
                i++;

            }
        }
        totalUSDText.GetComponent<TextMeshProUGUI>().text = currentUSD.ToString() + " $";
        var profit = CalculationOfProfit(totalUSDText.GetComponent<TextMeshProUGUI>().text);
        //Debug.Log(profit);
        if (profit > 0)
        {
            if (totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>() != null)
            {
                totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "-" + profit;
                totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
            }
        }
        else
        {
            profit = profit * -1;
            if (totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>() != null)
            {
                totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + profit;
                totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.green;
            }

        }


        StartCoroutine(UpdatePrices());
    }



    private string GetUSDPrice(string name)
    {
        var result = "";
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://min-api.cryptocompare.com/data/price?fsym=" + name + "&tsyms=BTC,USD,EUR,TRY,RUB,CNY,GBP,ILS,ZAR,KRW"),

        };
        using (var response = client.SendAsync(request))
        {
            if (response.Exception == null)
            {
                response.Result.EnsureSuccessStatusCode();
                var body = response.Result.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<CryptoPriceData.Root>(body.Result);
                result = data.USD.ToString();
            }
        }


        return result;
    }

    public void BuyOrSellButton()
    {

        var intIndex = indexOfChild;
        var data = content.transform.GetChild(intIndex).GetComponentInChildren<TextMeshProUGUI>().text.Split(' ');
        var priceCrypto = float.Parse(data[0]);
        float yourPrice;
        var name = content.transform.GetChild(intIndex).GetComponent<Image>().sprite.name;

        if (!string.IsNullOrEmpty(tradeFunctionsObject.GetComponentInChildren<TMP_InputField>().text))
        {
            if (float.TryParse(tradeFunctionsObject.GetComponentInChildren<TMP_InputField>().text, out yourPrice))
            {

                if (buyOrSell.Equals("Buy"))
                {
                    var commission = yourPrice * 0.001;
                    Debug.Log("Commission: " + commission);

                    if ((yourPrice + commission) <= PlayerPrefs.GetFloat("USD"))
                    {

                        var currentUSD = PlayerPrefs.GetFloat("USD") - yourPrice - commission;
                        PlayerPrefs.SetFloat("USD", (float)currentUSD);

                        resultPrice = yourPrice / priceCrypto;

                        if (PlayerPrefs.HasKey(name))
                        {
                            var currentCoin = PlayerPrefs.GetFloat(name);
                            resultPrice += currentCoin;
                            PlayerPrefs.SetFloat(name, resultPrice);
                        }
                        else
                        {
                            PlayerPrefs.SetFloat(name, resultPrice);
                        }

                        StartCoroutine(WarningMessage("Order Success!", acceptTick));

                        tradeFunctionsObject.gameObject.SetActive(false);
                    }
                    else
                    {
                        StartCoroutine(WarningMessage("Your wallet is not enough for buy " + name, cancelTick));
                    }

                }
                else
                {
                    //Sell Crypto Part
                    if (PlayerPrefs.HasKey(name))
                    {
                        //You have that coin

                        if (PlayerPrefs.HasKey("USD"))
                        {
                            if (yourPrice <= PlayerPrefs.GetFloat(name))
                            {
                                var currentCrypto = PlayerPrefs.GetFloat(name) - yourPrice;
                                PlayerPrefs.SetFloat(name, currentCrypto);
                                var currentUSD = PlayerPrefs.GetFloat("USD");
                                var sellingPrice = yourPrice * priceCrypto;
                                var commission = sellingPrice * 0.001;
                                sellingPrice -= (float)commission;
                                sellingPrice += currentUSD;
                                PlayerPrefs.SetFloat("USD", sellingPrice);

                                tradeFunctionsObject.gameObject.SetActive(false);
                                StartCoroutine(WarningMessage("Order Success!", acceptTick));
                            }
                            else
                            {
                                StartCoroutine(WarningMessage("Your Crypto wallet is not enough!", cancelTick));
                            }

                        }


                    }
                    else
                    {
                        StartCoroutine(WarningMessage("You Don't have a coin! You can not sell coin.", cancelTick));
                    }



                }
            }
            else
            {
                StartCoroutine(WarningMessage("You must enter a float value!", cancelTick));
            }
        }
        else
        {
            StartCoroutine(WarningMessage("You must enter a float value!", cancelTick));
        }

        InitializeTotalUSD(); // We are refresh the Total USD

    }



    public void ActiveTradeFunctionObject()
    {
        var getButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var name = getButton.name.Split(' ');
        tradeFunctionsObject.gameObject.SetActive(true);
        tradeFunctionsObject.transform.GetChild(1).GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>().text = name[0];
        var index = int.Parse(name[1]);
        indexOfChild = index;
        buyOrSell = name[0];
        tradeFunctionsObject.transform.GetChild(0).GetComponent<TMP_InputField>().text = "";

        if (name[0].Equals("Buy"))
        {
            //Buy Functions
            tradeFunctionsObject.transform.GetChild(1).GetComponent<Button>().image.color = Color.green;
            tradeFunctionsObject.transform.GetChild(0).GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Price $";

        }
        else
        {
            //Sell Functions
            tradeFunctionsObject.transform.GetChild(1).GetComponent<Button>().image.color = Color.red;
            var cryptoName = content.transform.GetChild(index).GetComponent<Image>().sprite.name;
            if (PlayerPrefs.HasKey(cryptoName))
            {
                tradeFunctionsObject.transform.GetChild(0).GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetFloat(cryptoName) + " " + cryptoName;
            }
            else
            {
                tradeFunctionsObject.transform.GetChild(0).GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Price " + cryptoName;
            }

        }


    }

    public void DisActiveTradeFunctionObject()
    {
        tradeFunctionsObject.gameObject.SetActive(false);
    }


    public IEnumerator WarningMessage(string message, Sprite ticks)
    {
        warningMessage.gameObject.SetActive(true);
        warningMessage.GetComponent<Image>().sprite = ticks;
        warningMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
        walletButton.enabled = false;
        yield return new WaitForSeconds(2f);
        walletButton.enabled = true;
        warningMessage.gameObject.SetActive(false);
    }

    public void WalletButton()
    {
        StopAllCoroutines();
        if (ControlWalletEmpty()) { return; }
        walletButton.gameObject.SetActive(false);
        turnBackButton.gameObject.SetActive(true);
        totalUSDText.GetComponent<TextMeshProUGUI>().text = "";
        totalUSDText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";

        if (content.transform.childCount > 0)
        {
            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
        }


        crypto.transform.position = new Vector3(tempPositionx + 200, crypto.transform.position.y, 0f);
        var positionY = 400f;

        for (int i = 0; i < sprites.Count; i++)
        {
            if (PlayerPrefs.HasKey(sprites[i].name))
            {
                if (PlayerPrefs.GetFloat(sprites[i].name) > 0)
                {
                    Debug.Log("I Found Crypto: " + sprites[i].name);
                    var nameCrypto = sprites[i].name;
                    var parents = content.GetComponent<RectTransform>();
                    var usdPrice = float.Parse(GetUSDPrice(nameCrypto));
                    var t = Instantiate(crypto, new Vector3(crypto.transform.position.x, positionY), Quaternion.identity);
                    t.GetComponent<Image>().sprite = sprites[i];
                    t.transform.GetChild(t.transform.childCount - 1).gameObject.SetActive(false);
                    t.transform.GetChild(t.transform.childCount - 2).gameObject.SetActive(false);
                    var data = PlayerPrefs.GetFloat(nameCrypto) + " " + nameCrypto + "\n";
                    data += "Price: " + (PlayerPrefs.GetFloat(nameCrypto) * usdPrice) + " $" + "\n";
                    t.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta = new Vector2(318, t.GetComponentInChildren<TextMeshProUGUI>().rectTransform.sizeDelta.y);
                    t.GetComponentInChildren<TextMeshProUGUI>().text = data;
                    t.GetComponent<RectTransform>().SetParent(parents);
                    t.gameObject.SetActive(true);
                    positionY -= 100;
                }


            }
        }




    }

    private bool ControlWalletEmpty()
    {
        var isEmpty = true;
        for (int i = 0; i < sprites.Count; i++)
        {
            if (PlayerPrefs.HasKey(sprites[i].name))
            {

                if (PlayerPrefs.GetFloat(sprites[i].name) > 0)
                    isEmpty = false;
            }
        }

        if (isEmpty) { StartCoroutine(WarningMessage("Wallet Empty", cancelTick)); }

        return isEmpty;

    }

    public void TurnBackButton()
    {
        //crypto.transform.position = new Vector3(crypto.transform.position.x+100,crypto.transform.position.y,0f);
        InitiliazeCryptos();
        StartCoroutine(UpdatePrices());
        turnBackButton.gameObject.SetActive(false);
        walletButton.gameObject.SetActive(true);

    }

    public void ShowGrap()
    {
        var getObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var cryptoName = getObject.GetComponent<Image>().sprite.name;
        cryptoName = cryptoName.ToLower();
        Application.OpenURL("https://www.binance.com/tr/trade/" + cryptoName + "_USDT");

    }

    private void OnApplicationQuit()
    {
        if (!PlayerPrefs.HasKey("Time"))
        {
            PlayerPrefs.SetString("Time", DateTime.Now.ToString());
        }

        var title = "Today's Cryptocurrency Prices by Bex Crypto";
        var body = "I miss you! Come and grow up your business.";
        NotificationManager.CancelAll();
        DateTime timeToNotify = DateTime.Now.AddMinutes(120);
        TimeSpan time = timeToNotify - DateTime.Now;
        NotificationManager.SendWithAppIcon(time, title, body, Color.green, NotificationIcon.Bell);




    }

    public void ResetButton()
    {
        var isHasCrypto = false;
        for (int i = 0; i < sprites.Count; i++)
        {
            if (PlayerPrefs.HasKey(sprites[i].name))
            {
                isHasCrypto = true;
                break;
            }
        }

        if (isHasCrypto)
        {
            resetMessage.gameObject.SetActive(true);
        }
        else
        {
            StartCoroutine(WarningMessage("You don't have crypto data yet!", cancelTick));
        }

    }

    public void YesOrNoButton()
    {
        var getButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (getButton.name.Equals("Yes"))
        {
            resetMessage.gameObject.SetActive(false);
            StartCoroutine(WarningMessage("Your Crypto Data Successfully Deleted!", acceptTick));
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene("Cryptos");
        }
        else
        {
            resetMessage.gameObject.SetActive(false);
        }
    }


}
