using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Orders 
{

    public enum OrderCommand
    {
        MoveOrder
    }

    private static List<Order> orders;

    public static void AddOrder(Order order)
    {
        orders.Add(order);
    }
}

/// <summary>
/// Class encapsulating individual orders given to units during the planning phase. 
/// </summary>
public class Order
{
    public Order()
    {
    }

    public Orders.OrderCommand command { get; set; }
    public Squad squad {get; set;}
    public int targetNode {get; set;}
}
