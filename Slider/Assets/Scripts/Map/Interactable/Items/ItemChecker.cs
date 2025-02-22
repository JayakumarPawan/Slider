using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemChecker : MonoBehaviour
{
    public string itemName;
    public UnityEvent OnEnter;

    public void OnTriggerEnter2D(Collider2D other)
    {
        Item item = other.GetComponent<Item>();
        if(item !=null && item.itemName == itemName)
        {
            OnEnter?.Invoke();
        }
    }
}
