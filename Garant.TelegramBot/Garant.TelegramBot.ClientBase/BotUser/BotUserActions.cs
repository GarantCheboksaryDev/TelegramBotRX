using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Garant.TelegramBot.BotUser;

namespace Garant.TelegramBot.Client
{
  partial class BotUserActions
  {
    public virtual void GetToken(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.Token))
      {
        _obj.Token = Guid.NewGuid().ToString();
        _obj.Save();
      }
      
      var dialog = Dialogs.CreateInputDialog(Garant.TelegramBot.BotUsers.Resources.RegistrationToken);
      var field = dialog.AddString(string.Empty, false);
      field.IsEnabled = false;
      field.Value = _obj.Token;
      dialog.Show();
    }

    public virtual bool CanGetToken(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted
        && string.IsNullOrEmpty(_obj.UserId)
        && _obj.Status == Status.Active
        // Проверка, что пользователь является Администратором.
        && Settings.AccessRights.CanUpdate();
    }

  }

}