namespace FlyTextFilter.Model;

public class FlyTextSetting
{
    public FlyTextTargets SourceYou;
    public FlyTextTargets SourceParty;
    public FlyTextTargets SourceOthers;

    public bool IsSettingEmpty()
    {
        return this.SourceYou == FlyTextTargets.None
               && this.SourceParty == FlyTextTargets.None
               && this.SourceOthers == FlyTextTargets.None;
    }
}
