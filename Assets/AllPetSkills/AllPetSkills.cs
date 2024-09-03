using PugMod;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class AllPetSkills : IMod
{
    public void EarlyInit()
    {
    }

    public void Init()
    {
        string targetName = "maxLevel";
        Type type = typeof(PetExtensions);
        /*var fieldInfo = type.GetMembersChecked().FirstOrDefault(info => info.GetNameChecked().Equals(targetName))
                ?? throw new Exception("Not found" + targetName);
        API.Reflection.SetValue(fieldInfo, type, 20);*/

        var members = type.GetMembers(BindingFlags.Static | BindingFlags.Public);
        Debug.Log(string.Join("\n", members.Select(x => x.Name)));
    }

    public void ModObjectLoaded(UnityEngine.Object obj)
    {
    }

    public void Shutdown()
    {
    }

    public void Update()
    {
    }
}
