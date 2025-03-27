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
    /// Получить пользователя чат-бота по логину в телеграм.
    /// </summary>
    /// <param name="username">Логин в telegram.</param>
    /// <returns>Запись справочника "Пользователи чат-бота".</returns>
    public static IBotUser GetBotUserByUsername(string username)
    {
      return BotUsers.GetAll(x => x.Username == username && x.Status == TelegramBot.BotUser.Status.Active).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить сотрудника по логину в телеграм.
    /// </summary>
    /// <param name="username">Логин в telegram</param>
    /// <returns>Найденная запись справочника "Сотрудники".</returns>
    public static Sungero.Company.IEmployee GetEmployeeByUsername(string username)
    {
      var botUser = GetBotUserByUsername(username);
      return botUser?.Employee;
    }

  }
}