using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text.RegularExpressions;

namespace Garant.TelegramBot.Server
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Разбить поисковый запрос на слова.
    /// </summary>
    /// <param name="name">Исходный поисковый запрос.</param>
    /// <returns>Слова из поискового запроса.</returns>
    private static string[] GetSearchTerms(string name)
    {
      return Regex.Matches(name, @"\b\w+\b")
        .Cast<Match>()
        .Select(m => m.Value)
        .ToArray();
    }
    
    /// <summary>
    /// Получить информацию о компаниях для телеграм-бота.
    /// </summary>
    /// <param name="name">Наименование для поиска.</param>
    /// <returns>Компании, найденные по совпадению слов в наименовании и юридическом наименовании.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public List<TelegramBot.Structures.Module.IEntityInfo> GetCounterparties(string name)
    {
      var setting = Functions.Setting.GetChatbotSettings();
      var entitiesCount = setting != null && setting.EntitiesCount.HasValue ? setting.EntitiesCount.Value : Constants.Module.DefaultMaxEntitiesCount;
      
      var searchTerms = GetSearchTerms(name);
      if (!searchTerms.Any())
        return new List<TelegramBot.Structures.Module.IEntityInfo>();
      
      var query = Sungero.Parties.Companies.GetAll();
      foreach (var searchTerm in searchTerms)
        query = query.Where(x => (x.Name != null && x.Name != string.Empty && x.Name.ToLower().Contains(searchTerm))
                            || (x.LegalName != null && x.LegalName != string.Empty && x.LegalName.ToLower().Contains(searchTerm)));

      return query.OrderBy(x => x.Name)
        .Take(entitiesCount)
        .Select(x => TelegramBot.Structures.Module.EntityInfo.Create(x.Name, x.Id))
        .ToList();
    }
    
    /// <summary>
    /// Получить информацию о типах документов для телеграм-бота.
    /// </summary>
    /// <param name="name">Наименование типа документа для поиска.</param>
    /// <returns>Список найденных типов документов. Если передано имя, то происходит фильтрация по имени. Если не передано, то возвращаются все типы документов.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public List<TelegramBot.Structures.Module.IEntityInfo> GetDocumentTypes()
    {
      return Sungero.Docflow.DocumentTypes.GetAll()
        .OrderBy(x => x.Name)
        .Select(x => TelegramBot.Structures.Module.EntityInfo.Create(x.Name, x.Id))
        .ToList();
    }
    
    /// <summary>
    /// Получить информацию о документах для телеграм-бота.
    /// </summary>
    /// <param name="documentTypeId">ИД типа документа.</param>
    /// <param name="name">Наименование для поиска.</param>
    /// <returns>Найденные по введенному наименованию документы. При поиске проверяется вхождение слов введенной строки в наименовании и юридическом наименовании записи справочника.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public Structures.Module.IEntitiesWithError GetDocuments(long documentTypeId, string name, long userId)
    {
      var setting = Functions.Setting.GetChatbotSettings();
      var entitiesCount = setting != null && setting.EntitiesCount.HasValue ? setting.EntitiesCount.Value : Constants.Module.DefaultMaxEntitiesCount;
      
      var documentType = Sungero.Docflow.DocumentTypes.GetAll(x => x.Id == documentTypeId).FirstOrDefault();
      if (documentType == null)
        return Structures.Module.EntitiesWithError.Create(new List<TelegramBot.Structures.Module.IEntityInfo>(), Garant.TelegramBot.Resources.DocumentTypeNotFound);
      
      var employee = Functions.BotUser.GetEmployeeByTelegramUserId(userId);
      if (employee == null)
        return Structures.Module.EntitiesWithError.Create(new List<TelegramBot.Structures.Module.IEntityInfo>(), Garant.TelegramBot.Resources.EmployeeNotFound);
      
      var searchTerms = GetSearchTerms(name);
      if (!searchTerms.Any())
        return Structures.Module.EntitiesWithError.Create(new List<TelegramBot.Structures.Module.IEntityInfo>(), Garant.TelegramBot.Resources.ClarifyRequest);
      
      var query = Sungero.Docflow.OfficialDocuments.GetAll(x => x.DocumentKind != null
                                                           && Equals(x.DocumentKind.DocumentType, documentType));
      // HACK: На больших объемах документов потребуется оптимизация.
      foreach (var searchTerm in searchTerms)
        query = query.Where(x => (x.Name != null && x.Name != string.Empty && x.Name.ToLower().Contains(searchTerm)));
      
      var entities = query.OrderBy(x => x.Name)
        .Take(500)
        .ToList()
        .Where(x => x.AccessRights.CanRead(employee))
        .Take(entitiesCount)
        .Select(x => TelegramBot.Structures.Module.EntityInfo.Create(x.Name, x.Id))
        .ToList();
      
      return Garant.TelegramBot.Structures.Module.EntitiesWithError.Create(entities, string.Empty);
    }
    
    /// <summary>
    /// Получить информацию о версии документа для телеграм-бота по его ИД.
    /// </summary>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="userId">ИД пользователя чат-бота.</param>
    /// <returns>Информация о версии документа для телеграм-бота.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public Structures.Module.IVersionInfo GetDocumentVersion(long documentId, long userId)
    {
      var document = Sungero.Docflow.OfficialDocuments.GetAll(x => x.Id == documentId).FirstOrDefault();
      if (document == null || document.LastVersion == null)
        return null;
      
      var employee = Functions.BotUser.GetEmployeeByTelegramUserId(userId);
      if (employee == null || !document.AccessRights.CanRead(employee))
        return null;
      
      var body = document.LastVersion.PublicBody;
      if (!document.HasPublicBody)
        body = document.LastVersion.Body;
      
      if (body != null)
      {
        using (var stream = body.Read())
        {
          using (var memoryStream = new System.IO.MemoryStream())
          {
            stream.CopyTo(memoryStream);
            return Structures.Module.VersionInfo.Create(memoryStream.ToArray(), document.LastVersion.AssociatedApplication?.Extension, document.Name);
          }
        }
      }
      
      return null;
    }
    
    /// <summary>
    /// Установить Id чата для пользователя чат-бота.
    /// </summary>
    /// <param name="username">Логин пользователя в телеграм.</param>
    /// <param name="chatId">Id чата.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void SetChatId(long userId, string chatId)
    {
      var botUser = BotUsers.GetAll(x => x.UserId == userId.ToString()).FirstOrDefault();
      if (botUser != null)
      {
        if (botUser.ChatId != chatId)
          botUser.ChatId = chatId;
        
        if (botUser.State.IsChanged)
          botUser.Save();
      }
    }
    
    /// <summary>
    /// Создание записи справочника "Пользователи чат-бота".
    /// </summary>
    /// <param name="mail">E-mail пользователя.</param>
    /// <param name="chatId">Id чата.</param>
    /// <param name="username">Логин пользователя в телеграм.</param>
    /// <param name="userId">Идентификатор пользователя в телеграм.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void CreateBotUser(string mail, string chatId, string username, long userId)
    {
      var employee = Sungero.Company.Employees.GetAll(x => x.Email == mail).FirstOrDefault();
      if (employee != null)
      {
        var botUser = BotUsers.GetAll(x => x.Status == Garant.TelegramBot.BotUser.Status.Active && x.UserId == userId.ToString()).FirstOrDefault();
        if (botUser == null)
        {
          botUser = BotUsers.Create();
          botUser.Username = username;
          botUser.UserId = userId.ToString();
          botUser.ChatId = chatId;
          botUser.Employee = employee;
          botUser.Save();
        }
      }
    }
    
    /// <summary>
    /// Привязка ИД пользователя телеграм и логина в телеграм к карточке пользователя чат-бота по регистрационному токену.
    /// </summary>
    /// <param name="token">Регистрационный токен пользователя.</param>
    /// <param name="chatId">Id чата.</param>
    /// <param name="username">Логин пользователя в телеграм.</param>
    /// <param name="userId">Идентификатор пользователя в телеграм.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public string RegisterUserByToken(string token, string chatId, string username, long userId)
    {
      var botUser = BotUsers.GetAll(x => x.Token == token).FirstOrDefault();
      if (botUser != null)
      {
        if (botUser.Status == TelegramBot.BotUser.Status.Active)
        {
          botUser.Username = username;
          botUser.UserId = userId.ToString();
          botUser.ChatId = chatId;
          botUser.Token = null;
          botUser.Save();
          return null;
        }
        else
          return "Запись справочника \"Пользователи чат-бота\" заблокирована.";
      }
      else
        return "Введен недействительный токен.";
    }
    
    /// <summary>
    /// Отправить заявку, полученную из чат-бота в работу.
    /// </summary>
    /// <param name="requestText">Текст заявки.</param>
    /// <param name="userId">ИД пользователя телеграм, от имени которого отправляется заявка.</param>
    /// <param name="fileInfos">Структура с информацией о файлах в формате json.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void CreateRequestFromBot(string requestText, long userId, string fileInfos)
    {
      var author = Functions.BotUser.GetEmployeeByTelegramUserId(userId);
      if (author != null)
      {
        var files = IsolatedFunctions.Deserialization.DesirializeDocumentsInfo(fileInfos);
        CreateRequest(requestText, author, files);
      }
    }
    
    /// <summary>
    /// Создать и отправить заявку в работу.
    /// </summary>
    /// <param name="requestText">Текст заявки.</param>
    /// <param name="author">Сотрудник, от имени которого отправляется заявка.</param>
    /// <param name="files">Список файлов, прикрепляемых к заявке.</param>
    public virtual void CreateRequest(string requestText, Sungero.Company.IEmployee author, List<Structures.Module.IFileInfo> files)
    {
      var requestsResponsibleRole = Roles.GetAll(x => x.Sid == Constants.Module.Roles.RequestsResponsibleRoleGuid).FirstOrDefault();
      if (requestsResponsibleRole != null)
      {
        var subject = Sungero.Docflow.PublicFunctions.Module.CutText(Garant.TelegramBot.Resources.RequestSubjectFormat(requestText), Sungero.Workflow.SimpleTasks.Info.Properties.Subject.Length);
        var task = Sungero.Workflow.SimpleTasks.Create(subject, requestsResponsibleRole);
        task.ActiveText = requestText;
        task.Author = author;
        foreach (var file in files)
        {
          if (!string.IsNullOrEmpty(file.Body) && !string.IsNullOrEmpty(file.Name))
          {
            var document = Sungero.Docflow.SimpleDocuments.Create();
            document.Name = Sungero.Docflow.PublicFunctions.Module.CutText(System.IO.Path.GetFileNameWithoutExtension(file.Name), Sungero.Docflow.SimpleDocuments.Info.Properties.Name.Length);
            document.Author = author;
            document.PreparedBy = author;
            document.Department = author.Department;
            document.AccessRights.Grant(author, DefaultAccessRightsTypes.FullAccess);
            using (var stream = new System.IO.MemoryStream(Convert.FromBase64String(file.Body)))
            {
              var extension = System.IO.Path.GetExtension(file.Name);
              document.CreateVersionFrom(stream, extension ?? string.Empty);
            }
            document.Save();
            task.Attachments.Add(document);
          }
        }
        task.Start();
      }
    }
  }
}