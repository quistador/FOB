using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface IHousable
{
    void HandleUnitArrived(GamePlayEvent eventInfo);
    void HandleUnitDeparted(GamePlayEvent eventInfo);
    bool ContainsNode(int NodeId);
    bool ContainsSquadId(Guid squadId);
    UnitListControl UnitsInHousing();
    List<int> NodeIdsInHousing();
}
