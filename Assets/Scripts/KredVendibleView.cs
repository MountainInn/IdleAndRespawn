using UnityEngine;
using UnityEngine.UI;

public class KredVendibleView : MonoBehaviour
{
    [SerializeField] Button buyButton;
    [SerializeField] Image icon;
    [SerializeField] Text nameText, desc, price, buttonText;

    KredVendible good ;


    public void ConnectVendible(KredVendible good)
    {
        this.good = good;

        icon.sprite = good.icon;

        UpdateBuyButtonInteractable();

        KredVendibles.Kredits.onChanged_Amount += (change) =>{ UpdateBuyButtonInteractable(); };

        buyButton.onClick.AddListener(()=>
        {
            if ( good.CanBuy(KredVendibles.Kredits)  )
            {
                good.Buy(KredVendibles.Kredits);

                UpdateIsOwned(good.isOwned);
            }
        });

        transform.localScale = Vector3.one;

        UpdateView();
    }

    public void UpdateView()
    {
        nameText.text = good.name;
        desc.text = good.description;
        price.text = good.price.ToString();

        if (!good.isOwned) UpdateBuyButtonInteractable();
    }

    public void UpdateIsOwned(bool goodIsOwned)
    {
        if (goodIsOwned)
        {
            buyButton.interactable = false;
            buyButton.image.color = buyButton.image.color.SetA(0);

            buttonText.text = "Owned";
        }
    }


    public void UpdateBuyButtonInteractable()
    {
        buyButton.interactable = good.CanBuy(KredVendibles.Kredits);
    }
}
