using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cryptos : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Sprite> sprites;
    public GameObject crypto;
    public GameObject content;
    public GameObject tradeFunctionsObject;
    private float[] holdPrice;
    private static int indexOfChild;
    private static string buyOrSell;
    private float resultPrice;

    private void Start()
    {
        PlayerPrefs.DeleteAll();
        holdPrice = new float[sprites.Count];
        InitiliazeCryptos();
        StartCoroutine(UpdatePrices());
    }

    private void Update()
    {

    }

    private void InitiliazeCryptos()
    {
        var parents = content.GetComponent<RectTransform>();
        var positionY = 400f;
        for (int i = 0; i < sprites.Count; i++)
        {
            var t = Instantiate(crypto, new Vector3(crypto.transform.position.x, positionY), Quaternion.identity);
            t.GetComponent<Image>().sprite = sprites[i];
            t.GetComponentInChildren<TextMeshProUGUI>().text = GetUSDPrice(sprites[i].name) + " $";
            t.GetComponentInChildren<Button>().name = "Buy " + i.ToString();
            t.gameObject.transform.GetChild(t.gameObject.transform.childCount - 1).GetComponent<Button>().name = "Sell " + i.ToString();
            t.GetComponent<RectTransform>().SetParent(parents);
            t.gameObject.SetActive(true);
            positionY -= 100;

        }
    }

    private IEnumerator UpdatePrices()
    {
        yield return new WaitForSeconds(5f);

        if (content.transform.childCount > 0)
        {
            var i = 0;
            foreach (Transform child in content.transform)
            {
                var nameCrypto = child.GetComponent<Image>().sprite.name;
                child.GetComponentInChildren<TextMeshProUGUI>().text = GetUSDPrice(nameCrypto) + " $";
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

                    tradeFunctionsObject.gameObject.SetActive(false);

                }
                else
                {
                    //Sell Crypto Part
                    if (PlayerPrefs.HasKey(name))
                    {
                        //You have that coin

                        if (PlayerPrefs.HasKey("USD"))
                        {
                            var currentUSD = PlayerPrefs.GetFloat("USD");
                            var sellingPrice = yourPrice * priceCrypto;
                            sellingPrice += currentUSD;
                            PlayerPrefs.SetFloat("USD",sellingPrice);
                        }
                        else
                        {
                            var sellingPrice = yourPrice * priceCrypto;
                            PlayerPrefs.SetFloat("USD", sellingPrice);
                        }
                        tradeFunctionsObject.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("You Don't have a coin! You can not sell coin.");
                    }



                }
            }
            else
            {
                Debug.Log("You must enter a float value!");
            }
        }
        else
        {
            Debug.Log("You must enter a float value!");
        }



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
            tradeFunctionsObject.transform.GetChild(0).GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Price " + cryptoName;

        }


    }

    public void DisActiveTradeFunctionObject()
    {
        tradeFunctionsObject.gameObject.SetActive(false);
    }


    public IEnumerator WarningMessage()
    {

        yield return new WaitForSeconds(2f);
    }

    public void WalletButton()
    {
        if(content.transform.childCount > 0)
        {
            foreach (Transform child in content.transform)
            {
                Destroy(child);
            }
        }
    }




}
