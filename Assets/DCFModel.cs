using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DCFModel : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject periodsInputField;
    public GameObject rateInputField;
    public GameObject verticalLayout;
    public GameObject warningImage;
    public GameObject resultImage;

    private static float rate;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    public void ClearButton()
    {
        periodsInputField.GetComponent<TMP_InputField>().text = "";
        rateInputField.GetComponent<TMP_InputField>().text = "";

        if (verticalLayout.transform.childCount > 0)
        {
            foreach (Transform child in verticalLayout.transform)
            {
                Destroy(child.gameObject);
            }
        }


    }

    public void AddButton()
    {
        if (verticalLayout.transform.childCount > 0)
        {
            foreach (Transform child in verticalLayout.transform)
            {
                Destroy(child.gameObject);
            }
        }

        int periods;
        var parents = verticalLayout.GetComponent<RectTransform>();
        var positionY = 500f;
        if (!string.IsNullOrEmpty(periodsInputField.GetComponent<TMP_InputField>().text) &&
            !string.IsNullOrEmpty(rateInputField.GetComponent<TMP_InputField>().text))
        {
            if (int.TryParse(periodsInputField.GetComponent<TMP_InputField>().text, out periods) &&
               float.TryParse(rateInputField.GetComponent<TMP_InputField>().text, out rate))
            {
                if(periods <= 2000)
                {
                    for (int i = 0; i < periods; i++)
                    {
                        var t = Instantiate(periodsInputField, new Vector3(periodsInputField.transform.position.x, positionY), Quaternion.identity);
                        t.GetComponent<TMP_InputField>().text = "";
                        t.GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = (i + 1) + ".Enter Cash $";
                        t.GetComponent<RectTransform>().SetParent(parents);
                        t.gameObject.SetActive(true);
                        positionY -= 100;
                    }
                }
                else
                {
                    StartCoroutine(WarningMessage("Periods too large value! It must smaller or equal 2000."));
                }
               
            }
            else
            {
                //Debug.Log("Invalid periods or rate value!");
                StartCoroutine(WarningMessage("Invalid periods or rate value!"));
            }
        }
        else
        {
            StartCoroutine(WarningMessage("You must enter rate and periods input!"));
        }
    }


    public void CalculateButton()
    {
        float cashValue;
        float dcfResult = 0;
        var list = new List<string>();

        if (verticalLayout.transform.childCount > 0)
        {
            foreach (Transform child in verticalLayout.transform)
            {
                var index = child.gameObject.GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text.Split('.');
                if (!string.IsNullOrEmpty(child.gameObject.GetComponent<TMP_InputField>().text))
                {
                    if (float.TryParse(child.gameObject.GetComponent<TMP_InputField>().text, out cashValue))
                    {
                        var data = DCFFormula(cashValue, int.Parse(index[0]), rate).ToString() + "," + index[0] + "," + rate + "," + cashValue;
                        list.Add(data);
                        dcfResult += DCFFormula(cashValue, int.Parse(index[0]), rate);
                    }
                    else
                    {
                        list.Clear();
                        var message = "Cash Value invalid at " + index[0];
                        StartCoroutine(WarningMessage(message));
                        return;
                    }
                }
                else
                {
                    list.Clear();
                    var message = "Cash Value invalid at " + index[0];
                    StartCoroutine(WarningMessage(message));
                    return;
                }
            }

            resultImage.gameObject.SetActive(true);
            var averageCash = AvarageCashes(list);
            var irr = InternalRateReturn(list);
            var result = "";

            result += "Total Discount Flow Cash: " + dcfResult + "\n";
            result += "Average of Cash: " + averageCash + "\n";
            result += "Internal Rate Return: " + irr + "\n";

            resultImage.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = result;

        }
        else
        {
            //Debug.Log("You must enter Rate and Periods Input!");
            StartCoroutine(WarningMessage("You must enter rate and periods input!"));

        }
    }

    public void AllValuesSame()
    {
        float cash = 0;
        var str_cash = "";
        if (verticalLayout.transform.childCount > 0)
        {
            var count = 0;
            foreach (Transform child in verticalLayout.transform)
            {
                if(count == 0)
                {
                    if (!string.IsNullOrEmpty(child.gameObject.GetComponent<TMP_InputField>().text))
                    {
                         if(float.TryParse(child.gameObject.GetComponent<TMP_InputField>().text, out cash))
                        {
                            str_cash = cash.ToString();
                        }
                        else
                        {
                            StartCoroutine(WarningMessage("You must enter first cash!"));
                            return;
                        }
                    }
                    else
                    {
                        StartCoroutine(WarningMessage("You must enter first cash!"));
                        return;
                    }
                }
                else
                {
                    child.gameObject.GetComponent<TMP_InputField>().text = str_cash;
                }
                count++;
            }
        }
        else
        {
            StartCoroutine(WarningMessage("You must enter rate and periods input!"));
        }
    }


    private float AvarageCashes(List<string> list)
    {
        float totalCash = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var data = list[i].Split(',');
            totalCash += float.Parse(data[data.Length - 1]);
        }

        return totalCash / list.Count;

    }

    private float InternalRateReturn(List<string> list)
    {
        float totalDCF = 0;
        float totalCash = 0;
        for (int i = 0; i < list.Count; i++)
        {
            var data = list[i].Split(',');
            totalDCF += float.Parse(data[0]);
            totalCash += float.Parse(data[data.Length - 1]);
        }

        return totalDCF - totalCash;

    }



    private float DCFFormula(float cashFlow, int periods, float rate)
    {
        rate = rate / 100;
        return cashFlow / Mathf.Pow((1 + rate), periods);
    }


    private IEnumerator WarningMessage(string message)
    {
        warningImage.gameObject.SetActive(true);
        warningImage.gameObject.GetComponent<Image>().GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(2f);
        warningImage.gameObject.GetComponent<Image>().GetComponentInChildren<TextMeshProUGUI>().text = "";
        warningImage.gameObject.SetActive(false);

    }

    public void CloseButton()
    {
        resultImage.gameObject.SetActive(false);
    }


}
