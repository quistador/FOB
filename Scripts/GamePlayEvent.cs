using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GamePlayEvent 
{
    public enum EventKind
    {
        UnitArrived
    }

    public GamePlayEvent()
    {
    }

    public int nodeId { get; set; }
    public GamePlayEvent.EventKind eventKind { get; set; }
}
