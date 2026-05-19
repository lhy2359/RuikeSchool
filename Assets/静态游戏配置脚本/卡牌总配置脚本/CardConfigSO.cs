using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "卡牌总配置", menuName = "游戏配置/卡牌总配置")]
public class CardConfigSO : ScriptableObject
{
    public List<CardData> CardDataList;

    private static CardConfigSO _instance;
    public static CardConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CardConfigSO>("卡牌总配置/卡牌总配置");
            }
            return _instance;
        }
    }

    // 根据ID获取卡牌配置
    public CardData GetCardDataById(string id)
    {
        return CardDataList.Find(card => card.PropId == id);
    }
}