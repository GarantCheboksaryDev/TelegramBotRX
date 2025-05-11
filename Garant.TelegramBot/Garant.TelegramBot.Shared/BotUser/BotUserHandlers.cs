using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Garant.TelegramBot.BotUser;

namespace Garant.TelegramBot
{
  partial class BotUserSharedHandlers
  {

    public override void StatusChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != Status.Active)
        _obj.Token = null;
    }

  }
}