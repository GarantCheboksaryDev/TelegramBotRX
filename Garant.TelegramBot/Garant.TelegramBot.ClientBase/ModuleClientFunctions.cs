using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Garant.TelegramBot.Client
{
  public class ModuleFunctions
  {
    
    #region Обложка

    /// <summary>
    /// Открыть настройки чат-бота.
    /// </summary>
    [LocalizeFunction("OpenChatbotSettingsFunctionName", "OpenChatbotSettingsFunctionDescription")]
    public void OpenChatbotSettings()
    {
      Functions.Setting.Remote.GetChatbotSettings()?.Show();
    }

    #endregion
    
  }
}