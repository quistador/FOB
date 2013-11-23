using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Army.
/// </summary>
public class Army 
{
    /// <summary>
    /// The squads.
    /// </summary>
    List<Squad> _Squads;

    /// <summary>
    /// Initializes a new instance of the <see cref="Army"/> class.
    /// </summary>
    /// <param name='player'>
    /// Player.
    /// </param>
    public Army(Profile player)
    {
        _Squads = new List<Squad>();

        // default team for now.  This will 
        // be replaced when I've implemented a saved-state/player profile
        _Squads.Add (new Squad(Squad.SquadType.Rifle));
        _Squads.Add (new Squad(Squad.SquadType.Assault));
        _Squads.Add (new Squad(Squad.SquadType.Marksman));
    }
    
    public List<Squad> Squads
    {
        get {return this._Squads;}
    }
}

/// <summary>
/// Squad.
/// </summary>
public class Squad
{
    /// <summary>
    /// Squad type.
    /// </summary>
    public enum SquadType
    {
        Rifle,
        Assault,
        Marksman
    }

    public Squad()
    {
    }

    public Squad(Squad.SquadType type)
    {
    }

    private Squad.SquadType _type;
}
