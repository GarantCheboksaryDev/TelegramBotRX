using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Garant.TelegramBot.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateRoles();
    }
    
    /// <summary>
    /// Инициализация ролей.
    /// </summary>
    public static void CreateRoles()
    {
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Garant.TelegramBot.Resources.RequestResponsibles,
                                                                      Garant.TelegramBot.Resources.RequestResponsibles,
                                                                      Constants.Module.Roles.RequestsResponsibleRoleGuid);
    }
  }
}
