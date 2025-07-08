using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.ScriptEngine
{
    public enum MicroInstructionType
    {
        MouseMove = 1,
        LeftClick = 2,
        LeftDown = 3,
        LeftUp = 4,
        MouseWheel = 5,
        KeyboardPress = 6,
        KeyboardRelease = 7,
        KeyboardPressAndRelease = 8,
        CheckColorAndHitKey = 9,
        Empty = 10,
        MoveMouseAndLeftClick = 11,
        JumpTo = 16,
        FindCollectMap = 18,
        FindHero = 19,
        EndFreeGame = 20,
        StartAutoRound = 21,
        FindReplayPosition = 22,
        IsHeroCanPlace = 23,
        SkillReleaseBeforeRgb = 24,
        SkillReleaseAfterRgb = 25,
        CheckPlaceSuccess = 26
    }

    public class MicroInstruction
    {
        public List<int> AllArguments { get; private set; } = new List<int>(6);
        public MicroInstructionType Type { get => (MicroInstructionType)AllArguments[0]; set => AllArguments[0] = (int)value; }

        // 用AllArguments替代之前的5个字段
        public int Argument1 { get => AllArguments[1]; set => AllArguments[1] = value; }
        public int Argument2 { get => AllArguments[2]; set => AllArguments[2] = value; }
        public int Argument3 { get => AllArguments[3]; set => AllArguments[3] = value; }
        public int Argument4 { get => AllArguments[4]; set => AllArguments[4] = value; }
        public int Argument5 { get => AllArguments[5]; set => AllArguments[5] = value; }


        public MicroInstruction(MicroInstructionType type, int arg1 = -1, int arg2 = -1, int arg3 = -1, int arg4 = -1, int arg5 = -1)
        {
            AllArguments = new List<int> { (int)type, arg1, arg2, arg3, arg4, arg5 };
        }

        public MicroInstruction(List<int> arguments)
        {
            AllArguments = arguments;
        }

        // 改为可读写索引器
        //public int this[int index] => index >= 0 && index < AllArguments.Count? AllArguments[index] : 0;
        public int this[int index] { get => index >= 0 && index < AllArguments.Count? AllArguments[index] : 0; set => AllArguments[index] = value; }
    }
}
