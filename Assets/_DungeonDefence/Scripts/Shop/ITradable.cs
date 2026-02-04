using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurrencyType
{
    Gold,
}

public interface ITradable
{
    string ItemId { get; }
    string Name { get; }
    Sprite Icon { get; }
    Dictionary<CurrencyType, int> GetCosts();
}