using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Garant.TelegramBot.Setting;

namespace Garant.TelegramBot
{
  partial class SettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Name = Garant.TelegramBot.Settings.Resources.ModuleSettings;
    }
  }

}