using System;
using System.Windows.Forms;
using VotiveBattleAuto.UI;

namespace VotiveBattleAuto;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
