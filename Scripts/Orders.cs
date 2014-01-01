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

    public static event GamePlayEventDelegates.OrderAddedEventHandler OrderAdded;

    private static List<Order> orders;

    public static void AddOrder(Order order)
    {
        if(orders == null)
        {
            orders = new List<Order>();
        }
        orders.Add(order);

        // notify all our event listeners that an order has been added. 
        OrderAdded(order);
    }

    /// <summary>
    /// Reads and clears all orders.
    /// </summary>
    /// <returns>The and clear orders.</returns>
    public static List<Order> ReadAndClearOrders()
    {
        // make a copy of the orders to return.
        List<Order> returnList = orders.Select(order => order).ToList();

        // clear the orders.  It's expected that the caller of this method
        // will process all the orders in action mode, and it doesn't make
        // sense to keep old orders in the queue. 
        orders.Clear();
        return returnList;
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
    public Guid SquadGuid {get; set;}
    public List<int> Path {get; set;}
    public Vector3 StartPosition { get; set; }
    public Vector3 EndPosition { get; set; }
}
