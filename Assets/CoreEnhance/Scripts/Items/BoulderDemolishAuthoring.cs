using Pug.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class BoulderDemolishAuthoring : MonoBehaviour
    {
    }
    public struct BoulderDemolishCD : IComponentData
    {

    }
    public class BoulderDemolishConverter : SingleAuthoringComponentConverter<BoulderDemolishAuthoring>
    {
        protected override void Convert(BoulderDemolishAuthoring authoring)
        {
            AddComponentData(new BoulderDemolishCD());
        }
    }
}
