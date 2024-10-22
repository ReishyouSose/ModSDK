//using HarmonyLib;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;

//namespace Assets.AllSkills
//{
//    [HarmonyPatch]
//    public static class Patch_OnUpdate
//    {
//        [HarmonyPatch(typeof(SaveSkillsSystem),nameof(SaveSkillsSystem.OnUpdate))] // 替换为你的类名
//        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
//        {
//            var codes = new List<CodeInstruction>(instructions);

//            // 定义跳转标签
//            Label labelToHandleBelow60 = generator.DefineLabel(); // 用于处理 < 60 的逻辑
//            Label flagLabel = generator.DefineLabel(); // 用于设置 flag = true 的位置

//            for (int i = 0; i < codes.Count; i++)
//            {
//                // 查找模运算 % 5 的 IL 代码
//                if (codes[i].opcode == OpCodes.Ldc_I4_5) // 检测常量 5
//                {
//                    // 将现有的指令替换为自定义逻辑
//                    codes[i].opcode = OpCodes.Nop; // 移除旧指令

//                    // 插入自定义逻辑：判断是否 >= 60
//                    codes.InsertRange(i, new[]
//                    {
//                    new CodeInstruction(OpCodes.Ldloc_S, 7), // 加载 levelFromSkill2
//                    new CodeInstruction(OpCodes.Ldc_I4_S, 60), // 加载常量 60
//                    new CodeInstruction(OpCodes.Blt_S, labelToHandleBelow60), // 如果 < 60，跳到前 60 级处理逻辑

//                    // 处理后 40 级每 2 级
//                    new CodeInstruction(OpCodes.Ldloc_S, 7), // 加载 levelFromSkill2
//                    new CodeInstruction(OpCodes.Ldc_I4_2), // 加载常量 2
//                    new CodeInstruction(OpCodes.Rem), // 计算 levelFromSkill2 % 2
//                    new CodeInstruction(OpCodes.Brfalse_S, flagLabel), // 如果结果为 0，跳到设置 flag = true 的位置

//                    // 处理前 60 级每 3 级
//                    new CodeInstruction(OpCodes.Ldloc_S, 7) { labels = new List<Label> { labelToHandleBelow60 } }, // 添加跳转标签
//                    new CodeInstruction(OpCodes.Ldc_I4_3), // 加载常量 3
//                    new CodeInstruction(OpCodes.Rem), // 计算 levelFromSkill2 % 3
//                    new CodeInstruction(OpCodes.Brfalse_S, flagLabel), // 如果结果为 0，跳到设置 flag = true 的位置
//                });
//                }
//            }

//            return codes.AsEnumerable();
//        }
//    }
//}
