using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Logging;

namespace FlyTextFilter;

public unsafe class FlyTextKindTests
{
    private const int SwitchDefaultValue = 54;
    private static readonly List<(int logId, int flyTextKind, int option)> TestData = new()
    {
        (-1, 54, 5),
        (447, 1, 5),
        (448, 5, 5),
        (449, 30, 5),
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
        (514, 39, 4),
        (515, 9, 5),
        (516, 11, 5),
        (517, 4, 2),
        (518, 4, 3),
        (519, 17, 5),
        (520, 30, 5),
        (521, 18, 5),
        (522, 19, 5),
        (523, 41, 5),
        (524, 42, 5),
        (525, 43, 5),
        (526, 12, 5),
        (527, 13, 5),
        (528, 12, 5),
        (529, 15, 5),
        (530, 16, 5),
        (531, 34, 5),
        (532, 35, 5),
        (536, 36, 5),
        (550, 34, 5),
        (551, 35, 5),
        (588, 14, 1),
        (589, 14, 1),
        (590, 23, 5),
        (594, 24, 5),
        (595, 24, 5),
        (596, 37, 5),
        (601, 17, 5),
        (602, 18, 5),
        (603, 12, 5),
        (604, 13, 5),
        (605, 38, 5),
        (607, 44, 5),
        (611, 40, 5),
        (612, 33, 5),
        (630, 45, 5),
        (631, 22, 5),
        (1472, 25, 5),
        (4794, 49, 5),
        (4795, 50, 5),
        (5206, 20, 5),
        (7300, 14, 1),
        (9051, 14, 1),
    };

    public static void RunTests(out bool hasPassed)
    {
        var convertLogMessageIdToCharaLogKind = GetConvertFunction();
        if (convertLogMessageIdToCharaLogKind == null)
        {
            hasPassed = false;
            return;
        }

        PluginLog.Debug("FlyTextKind test start.");

        var hasChanged = false;
        foreach (var (logId, expectedFlyTextKind, expectedOption) in TestData)
        {
            var option = 0;
            var flyTextKind = convertLogMessageIdToCharaLogKind(logId, &option);

            if (flyTextKind != expectedFlyTextKind || option != expectedOption)
            {
                hasChanged = true;
                PluginLog.Error($"/!\\ logId: {logId}, expected ({expectedFlyTextKind}, {expectedOption}), got ({flyTextKind}, {option}) (FlyTextKind, option).");
            }
        }

        if (hasChanged)
        {
            PluginLog.Error("FlyTextKind enum has changed, please report the issue.");
        }

        hasPassed = !hasChanged;

        PluginLog.Debug("FlyTextKind test end.");
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

        PluginLog.Information(data.ToString());
    }

    private static delegate* unmanaged<int, int*, int> GetConvertFunction()
    {
        var address = IntPtr.Zero;
        try
        {
            address = Service.SigScanner.ScanText("C7 02 ?? ?? ?? ?? 81 F9");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "FlyTextKind convert function sig scan failed.");
        }

        return (delegate* unmanaged<int, int*, int>)address;
    }
}
