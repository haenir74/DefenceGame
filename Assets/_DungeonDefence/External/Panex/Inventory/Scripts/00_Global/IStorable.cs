using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panex.Inventory {
    public interface IStorable
    {
        int ID { get; }
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }
    }
}