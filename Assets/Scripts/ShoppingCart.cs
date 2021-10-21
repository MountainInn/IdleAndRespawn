using UnityEngine;
using UnityEngine.UI;
using System;

public class ShoppingCart : MonoBehaviour
{
    static int buyLevelQuantity = 1;
    static public int _BuyLevelQuantity
    {
        get => buyLevelQuantity;
        set
        {
            buyLevelQuantity = value;

            onChangedBuyLevelQuantity?.Invoke();
        }
    }

    static public event Action onChangedBuyLevelQuantity;

    [SerializeField]
    Button
        butt1,
        butt5,
        butt25;

    [SerializeField]
    InputField
        customQuantityField;

    

    void Set1() => _BuyLevelQuantity = 1;
    void Set5() => _BuyLevelQuantity = 5;
    void Set25() => _BuyLevelQuantity = 25;



    void Start()
    {
        // butt1.onClick.AddListener(() =>{ Set1();  HighlightButton(butt1); });
        // butt5.onClick.AddListener(() =>{ Set5();  HighlightButton(butt5); });
        // butt25.onClick.AddListener(()=>{ Set25(); HighlightButton(butt25); });

        // butt5.onClick.Invoke();
    }

    int quantityID = 0;
    int[] quantityArray = new int[]{ 1, 5, 25 };
    public void CycleQuantity(Text buttonText)
    {
        _BuyLevelQuantity = quantityArray[( ++quantityID )%quantityArray.Length];

        buttonText.text = "x"+_BuyLevelQuantity.ToString();
    }

    void HighlightButton(Button button)
    {
        butt1.interactable = true;
        butt5.interactable = true;
        butt25.interactable = true;

        button.interactable = false;
    }

    private void InitCustomQuantity()
    {
        customQuantityField.text = "50";

        customQuantityField.onValidateInput += (strInput, charIndex, c) =>
        {
            if (char.IsDigit(c)) return c;
            else return '\0';
        };

        customQuantityField.onValueChanged.AddListener(
            (str) =>
            {
                _BuyLevelQuantity = int.Parse(str.FilterNonNumbers());
            }
        );
    }


    void CustomQuantityInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            if (!customQuantityField.isFocused)
            {
                customQuantityField.ActivateInputField();
            }
            else
            {
                customQuantityField.DeactivateInputField();
            }
    }
}
