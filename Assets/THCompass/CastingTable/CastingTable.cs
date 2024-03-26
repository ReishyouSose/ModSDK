using PugMod;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastingTable : EntityMonoBehaviour
{
    public GameObject graphics;

    protected override void DeathEffect()
    {
        //API.Effects.PlayPuff(100, RenderPosition);
        graphics.SetActive(false);
    }
}
