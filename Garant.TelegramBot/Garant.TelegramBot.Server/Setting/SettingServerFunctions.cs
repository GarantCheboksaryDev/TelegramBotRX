using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Garant.TelegramBot.Setting;

namespace Garant.TelegramBot.Server
{
  partial class SettingFunctions
  {

    /// <summary>
    /// Получить запись справочника "Настройки чат-бота".
    /// </summary>
    /// <returns></returns>
    [Remote]
    public static Garant.TelegramBot.ISetting GetChatbotSettings()
    {
      return Garant.TelegramBot.Settings.GetAll().FirstOrDefault();
    }

  }
}