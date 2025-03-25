using System;
using System.Collections.Generic;
using System.Text;

namespace FlyTextFilter;

public unsafe class FlyTextKindTests
{
    private const int SwitchDefaultValue = 58;
    private static readonly List<(int logId, int flyTextKind, int option)> TestData =
    [
        (-1, 58, 5),
        (447, 1, 5),
        (448, 5, 5),
        (449, 34, 5),
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
        (514, 43, 4),
        (515, 9, 5),
        (516, 11, 5),
        (517, 4, 2),
        (518, 4, 3),
        (519, 21, 5),
        (520, 34, 5),
        (521, 22, 5),
        (522, 23, 5),
        (523, 45, 5),
        (524, 46, 5),
        (525, 47, 5),
        (526, 12, 5),
        (527, 13, 5),
        (528, 12, 5),
        (529, 19, 5),
        (530, 20, 5),
        (531, 38, 5),
        (532, 39, 5),
        (536, 40, 5),
        (550, 38, 5),
        (551, 39, 5),
        (588, 14, 1),
        (589, 14, 1),
        (590, 27, 5),
        (594, 28, 5),
        (595, 28, 5),
        (596, 41, 5),
        (601, 21, 5),
        (602, 22, 5),
        (603, 12, 5),
        (604, 13, 5),
        (605, 42, 5),
        (607, 48, 5),
        (611, 44, 5),
        (612, 37, 5),
        (630, 49, 5),
        (631, 26, 5),
        (1472, 29, 5),
        (4794, 53, 5),
        (4795, 54, 5),
        (5206, 24, 5),
        (7300, 14, 1),
        (9051, 14, 1),
        (9919, 15, 5),
    ];

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
        TestData.Sort((item1, item2) => item1.flyTextKind.CompareTo(item2.flyTextKind));
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
