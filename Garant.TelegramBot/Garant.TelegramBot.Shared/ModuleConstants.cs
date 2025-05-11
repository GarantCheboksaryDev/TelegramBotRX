using System;
using Sungero.Core;

namespace Garant.TelegramBot.Constants
{
  public static class Module
  {

    /// <summary>
    /// Значение по-умолчанию для масимального количества сущностей, получаемых чат-ботом из сервиса интеграции.
    /// </summary>
    public const int DefaultMaxEntitiesCount = 200;
    
    /// <summary>
    /// Guid-ы ролей.
    /// </summary>
    public static class Roles
    {
      /// <summary>
      /// Guid роли "Ответственные за заявки".
      /// </summary>
      public static readonly Guid RequestsResponsibleRoleGuid = Guid.Parse("EC42E4B8-AB42-43DB-8FE3-6A3FE023C01C");
    }
  }
}