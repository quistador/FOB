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
        this.id = Guid.NewGuid();
    }

    public Squad(Squad.SquadType type) : this()
    {
        this.squadType = type;
    }

    public Guid id { get; set; }
    public Squad.SquadType squadType { get; set; }
    public string squadTypeDisplayName
    {
        get
        {
            switch(this.squadType)
            {
            case Squad.SquadType.Assault:
                return "Assault Squad";
            case Squad.SquadType.Marksman:
                return "Marksman Squad";
            case Squad.SquadType.Rifle:
                return "Rifle Squad";
            default:
                return "unspecified squad type!";
            }

        }
    }
}
