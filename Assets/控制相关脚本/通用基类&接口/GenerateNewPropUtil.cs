using UnityEngine;

public static class GenerateNewPropUtil
{
    public static ItemData GenerateNewItem(string Id)
    {
        return ItemConfigSO.Instance.GetItemDataById(Id);
    }
    public static void PlayerGetNewItemById(string Id)
    {
        var itemdata = GenerateNewItem(Id);
        if (itemdata == null)
        {
            Debug.Log("没有对应物品");
            return;
        }
        Debug.Log("得到了" + itemdata.PropName);
        ItemManageSystem.Instance.AddItem(Id, itemdata.DefaultMaxUses, itemdata.DefaultMaxUses);
    }
    public static MedicineData GenerateNewMedicine(string Id)
    {
        return MedicineConfigSO.Instance.GetMedicineDataById(Id);
    }

    public static void PlayerGetNewMedicineById(string Id)
    {
        var medicinedata = GenerateNewMedicine(Id);
        if (medicinedata == null)
        {
            Debug.Log("没有对应药品");
            return;
        }
        Debug.Log("得到了" + medicinedata.PropName);
        MedicineManageSystem.Instance.AddMedicine(Id, 1);
    }

    public static CardData GenerateNewCard(string Id)
    {
        return CardConfigSO.Instance.GetCardDataById(Id);
    }

    public static void PlayerGetNewCardById(string Id)
    {
        var carddata = GenerateNewCard(Id);
        if (carddata == null)
        {
            Debug.Log("没有对应卡牌");
            return;
        }
        Debug.Log("得到了" + carddata.PropName);
        CardManageSystem.Instance.AddCard(Id, 1);
    }

}