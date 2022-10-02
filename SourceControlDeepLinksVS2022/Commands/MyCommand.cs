﻿using System.Threading.Tasks;

namespace SourceControlDeepLinks
{
  [Command(PackageIds.MyCommand)]
  internal sealed class MyCommand : BaseCommand<MyCommand>
  {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
      await VS.MessageBox.ShowWarningAsync("SourceControlDeepLinks", "Button clicked");
    }
  }
}
