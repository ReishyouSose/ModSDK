using PugConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public class AutoFisherTerminalAuthoring :MonoBehaviour
    {
    }
    public struct AutoFisherTerminalCD : IComponentData
    {

    }
    public class AutoFisherTerminalConverter: SingleAuthoringComponentConverter<AutoFisherTerminalAuthoring>
    {
        protected override void Convert(AutoFisherTerminalAuthoring authoring)
        {
            AddComponentData(new AutoFisherTerminalCD());
        }
    }
}
