using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GamePlayEvent 
{
    public enum EventKind
    {
        UnitArrived,
        UnitDeparted
    }

    public GamePlayEvent()
    {
        id = Guid.NewGuid();
    }

    public int nodeId { get; set; }
    public GamePlayEvent.EventKind eventKind { get; set; }
    public Guid id { get; set; }
}
