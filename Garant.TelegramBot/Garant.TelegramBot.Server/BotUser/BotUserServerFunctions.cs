using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Garant.TelegramBot.BotUser;

namespace Garant.TelegramBot.Server
{
  partial class BotUserFunctions
  {
    /// <summary>
    /// Получить пользователя чат-бота по логину в telegram.
    /// </summary>
    /// <param name="userId">ИД пользвателя в telegram.</param>
    /// <returns>Запись справочника "Пользователи чат-бота".</returns>
    public static IBotUser GetBotUserByTelegramUserId(long userId)
    {
      return BotUsers.GetAll(x => x.UserId == userId.ToString() && x.Status == TelegramBot.BotUser.Status.Active).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить сотрудника по логину в telegram.
    /// </summary>
    /// <param name="userId">ИД пользвателя в telegram.</param>
    /// <returns>Найденная запись справочника "Сотрудники".</returns>
    public static Sungero.Company.IEmployee GetEmployeeByTelegramUserId(long userId)
    {
      var botUser = GetBotUserByTelegramUserId(userId);
      return botUser?.Employee;
    }

  }
}