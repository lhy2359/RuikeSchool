using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "物品总配置", menuName = "游戏配置/物品总配置")]

public class ItemConfigSO : ScriptableObject
{
    public List<ItemData> ItemDataList;

    private static ItemConfigSO _instance;
    public static ItemConfigSO Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动查找项目里唯一的配置文件
                _instance = Resources.Load<ItemConfigSO>("物品总配置/物品总配置");
            }
            return _instance;
        }
    }

    // 根据ID获取物品配置
    public ItemData GetItemDataById(string id)
    {
        return ItemDataList.Find(item => item.PropId == id);
    }
}
