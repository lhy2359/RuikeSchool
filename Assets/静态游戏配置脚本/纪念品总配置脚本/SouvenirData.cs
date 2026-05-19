using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 单个纪念品的
/// </summary>
[Serializable]
public class SouvenirData
{
    [Header("成就唯一ID")]
    public string SouvenirId;

    [Header("纪念品名称")]
    public string SouvenirName;

    [Header("纪念品图标")]
    public Sprite Icon;

    [Header("纪念品描述")]
    public string Description;
    
}
