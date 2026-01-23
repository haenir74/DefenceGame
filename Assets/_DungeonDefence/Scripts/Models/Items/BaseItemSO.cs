using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

public abstract class BaseItemSO : ScriptableObject, IStorable
{
    [Header("Base Settings")]
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private string description;
    [SerializeField] private int maxStack = 99;
    [SerializeField] private Sprite icon;
    [SerializeField] private int goldCost;

    public int ID => id;
    public string Name => name;
    public string Description => description;
    public int MaxStack => maxStack;
    public Sprite Icon => icon;
    public int GoldCost => goldCost;
}