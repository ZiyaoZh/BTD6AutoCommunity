using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using System.Collections.Generic;

namespace BTD6AutoCommunity.Services.Interfaces
{
    public interface IScriptService
    {
        /// <summary>
        /// 设置脚本元数据
        /// </summary>
        /// <param name="metadata"></param>
        void SetMetadata(ScriptMetadata metadata);

        /// <summary>
        /// 获取脚本元数据
        /// </summary>
        /// <returns></returns>
        ScriptMetadata GetMetadata();
        
        /// <summary>
        /// 设置脚本指令序列
        /// </summary>
        /// <returns></returns>
        InstructionSequence GetInstructions();

        /// <summary>
        /// 获取脚本指令序列的副本
        /// </summary>
        /// <returns></returns>
        InstructionSequence GetInstructionsCopy();

        /// <summary>
        /// 添加指令
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <param name="roundTrigger"></param>
        /// <param name="coinTrigger"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        Instruction AddInstruction(ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords);

        /// <summary>
        /// 添加指令包
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        int AddInstructionBundle(InstructionSequence bundle, int times);

        /// <summary>
        /// 插入指令
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <param name="roundTrigger"></param>
        /// <param name="coinTrigger"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        Instruction InsertInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords);

        /// <summary>
        /// 插入指令包
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bundle"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        int InsertInstructionBundle(int index, InstructionSequence bundle, int times);

        /// <summary>
        /// 修改指令
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <param name="roundTrigger"></param>
        /// <param name="coinTrigger"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        Instruction ModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords);

        /// <summary>
        /// 尝试修改指令
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <param name="roundTrigger"></param>
        /// <param name="coinTrigger"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        bool TryModifyInstruction(int index, ActionTypes type, List<int> args, int roundTrigger, int coinTrigger, (int, int) coords);

        /// <summary>
        /// 删除指令
        /// </summary>
        /// <param name="index"></param>
        void RemoveInstruction(int index);

        /// <summary>
        /// 删除指令序列
        /// </summary>
        /// <param name="indices"></param>
        void RemoveInstructions(List<int> indices);

        /// <summary>
        /// 移动指令
        /// </summary>
        /// <param name="index"></param>
        /// <param name="up"></param>
        void MoveInstruction(int index, bool up);

        /// <summary>
        /// 获取指令
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Instruction GetInstruction(int index);

        /// <summary>
        /// 获取猴子Id列表
        /// </summary>
        /// <returns></returns>
        List<int> GetInstructionsMonkeyIds();

        /// <summary>
        /// 清空指令
        /// </summary>
        void ClearInstructions();

        /// <summary>
        /// 构建指令
        /// </summary>
        void BuildInstructions();

        /// <summary>
        /// 保存指令到脚本文件
        /// </summary>
        /// <returns></returns>
        string SaveScript();

        /// <summary>
        /// 获取脚本文件路径
        /// </summary>
        /// <param name="seletedMap"></param>
        /// <param name="selectedDifficulty"></param>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        string GetScriptPath(string seletedMap, string selectedDifficulty, string scriptName);


        /// <summary>
        /// 用脚本绝对路径加载脚本文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool LoadScript(string filePath);

        /// <summary>
        /// 获取预览
        /// </summary>
        /// <returns></returns>
        List<string> GetPreview(); // 供绑定 PreviewLB 的显示

    }
}
