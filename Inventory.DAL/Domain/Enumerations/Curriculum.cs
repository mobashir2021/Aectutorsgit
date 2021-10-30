using System.ComponentModel;

namespace AECMIS.DAL.Domain.Enumerations
{
    public enum Curriculum
    {
        [Description("Key Stage 1")]
        KeyStage1 = 6001,
        [Description("Key Stage 2")]
        KeyStage2 = 6002,
        [Description("11+")]
        ElevenPlus = 6003,
        [Description("13+")]
        ThirteenPlus = 6004,
        [Description("Key Stage 3")]
        KeyStage3 = 6005,
        [Description("GCSE")]
        Gcse = 6006,
        [Description("A'Level")]
        ALevel = 6007,
        [Description("Senior")]
        Senior = 6008
    }
}
