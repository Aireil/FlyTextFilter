using System;
using System.Collections.Generic;
using System.Text;

namespace FlyTextFilter;

public unsafe class FlyTextKindTests
{
    private const int SwitchDefaultValue = 55;
    private static readonly List<(int logId, int flyTextKind, int option)> TestData = new()
    {
        (-1, 55, 5),
        (447, 1, 5),
        (448, 5, 5),
        (449, 31, 5),
        (450, 3, 5),
        (451, 7, 5),
        (504, 0, 5),
        (505, 2, 5),
        (506, 8, 5),
        (508, 0, 2),
        (509, 0, 3),
        (510, 4, 5),
        (511, 6, 5),
        (512, 4, 4),
        (513, 4, 4),
        (514, 40, 4),
        (515, 9, 5),
        (516, 11, 5),
        (517, 4, 2),
        (518, 4, 3),
        (519, 18, 5),
        (520, 31, 5),
        (521, 19, 5),
        (522, 20, 5),
        (523, 42, 5),
        (524, 43, 5),
        (525, 44, 5),
        (526, 12, 5),
        (527, 13, 5),
        (528, 12, 5),
        (529, 16, 5),
        (530, 17, 5),
        (531, 35, 5),
        (532, 36, 5),
        (536, 37, 5),
        (550, 35, 5),
        (551, 36, 5),
        (588, 14, 1),
        (589, 14, 1),
        (590, 24, 5),
        (594, 25, 5),
        (595, 25, 5),
        (596, 38, 5),
        (601, 18, 5),
        (602, 19, 5),
        (603, 12, 5),
        (604, 13, 5),
        (605, 39, 5),
        (607, 45, 5),
        (611, 41, 5),
        (612, 34, 5),
        (630, 46, 5),
        (631, 23, 5),
        (1472, 26, 5),
        (4794, 50, 5),
        (4795, 51, 5),
        (5206, 21, 5),
        (7300, 14, 1),
        (9051, 14, 1),
        (9919, 15, 5),
    };

    public static void RunTests(out bool hasPassed)
    {
        var convertLogMessageIdToCharaLogKind = GetConvertFunction();
        if (convertLogMessageIdToCharaLogKind == null)
        {
            hasPassed = false;
            return;
        }

        Service.PluginLog.Debug("FlyTextKind test start.");

        var hasChanged = false;
        foreach (var (logId, expectedFlyTextKind, expectedOption) in TestData)
        {
            var option = 0;
            var flyTextKind = convertLogMessageIdToCharaLogKind(logId, &option);

            if (flyTextKind != expectedFlyTextKind || option != expectedOption)
            {
                hasChanged = true;
                Service.PluginLog.Error($"/!\\ logId: {logId}, expected ({expectedFlyTextKind}, {expectedOption}), got ({flyTextKind}, {option}) (FlyTextKind, option).");
            }
        }

        if (hasChanged)
        {
            Service.PluginLog.Error("FlyTextKind enum has changed, please report the issue.");
        }

        hasPassed = !hasChanged;

        Service.PluginLog.Debug("FlyTextKind test end.");
    }

    public static void PrintData()
    {
        var convertLogMessageIdToCharaLogKind = GetConvertFunction();
        if (convertLogMessageIdToCharaLogKind == null)
        {
            return;
        }

        var data = new StringBuilder();
        for (var i = -1; i < 10000; i++)
        {
            var option = 0;
            var flyTextKind = convertLogMessageIdToCharaLogKind(i, &option);
            if (i == -1 || flyTextKind != SwitchDefaultValue)
            {
                data.Append($"\n({i}, {flyTextKind}, {option}),");
            }
        }

        Service.PluginLog.Information(data.ToString());
    }

    private static delegate* unmanaged<int, int*, int> GetConvertFunction()
    {
        var address = nint.Zero;
        try
        {
            address = Service.SigScanner.ScanText("C7 02 ?? ?? ?? ?? 81 F9");
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "FlyTextKind convert function sig scan failed.");
        }

        return (delegate* unmanaged<int, int*, int>)address;
    }
}
