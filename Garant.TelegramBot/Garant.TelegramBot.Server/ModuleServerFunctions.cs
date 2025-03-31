using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Garant.TelegramBot.Server
{
  public class ModuleFunctions
  {
    private static string[] GetSearchTerms(string name)
    {
      if (name != null)
        return name.Split(new char[] { ' ', ',', '.', '\'', '\"' })
          .Select(x => x.ToLower())
          .ToArray();
      else
        return new string[0];
    }
    
    /// <summary>
    /// Получить информацию о компаниях для телеграм-бота.
    /// </summary>
    /// <param name="name">Наименование для поиска.</param>
    /// <returns>Найденные по введенному наименованию компании. При поиске проверяется вхождение слов введенной строки в наименовании и юридическом наименовании записи справочника.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public List<TelegramBot.Structures.Module.IEntityInfo> GetCounterparties(string name)
    {
      var searchTerms = GetSearchTerms(name);
      if (!searchTerms.Any())
        return new List<TelegramBot.Structures.Module.IEntityInfo>();
      
      var query = Sungero.Parties.Companies.GetAll();
      foreach (var searchTerm in searchTerms)
        query = query.Where(x => (x.Name != null && x.Name != string.Empty && x.Name.ToLower().Contains(searchTerm))
                            || (x.LegalName != null && x.LegalName != string.Empty && x.LegalName.ToLower().Contains(searchTerm)));

      return query.OrderBy(x => x.Name)
        .Take(200)
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
    /// <param name="documentTypeGuid">ИД типа документа.</param>
    /// <param name="name">Наименование для поиска.</param>
    /// <returns>Найденные по введенному наименованию документы. При поиске проверяется вхождение слов введенной строки в наименовании и юридическом наименовании записи справочника.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public Structures.Module.IEntitiesWithError GetDocuments(long documentTypeId, string name, string username)
    {
      var documentType = Sungero.Docflow.DocumentTypes.GetAll(x => x.Id == documentTypeId).FirstOrDefault();
      if (documentType == null)
        return Structures.Module.EntitiesWithError.Create(new List<TelegramBot.Structures.Module.IEntityInfo>(), "Не найден тип документа по ИД");
      
      var employee = Functions.BotUser.GetEmployeeByUsername(username);
      if (employee == null)
        return Structures.Module.EntitiesWithError.Create(new List<TelegramBot.Structures.Module.IEntityInfo>(), "Не удалось найти сотрудника в Directum RX по логину в telegram");
      
      var searchTerms = GetSearchTerms(name);
      if (!searchTerms.Any())
        return Structures.Module.EntitiesWithError.Create(new List<TelegramBot.Structures.Module.IEntityInfo>(), "Не удалось найти сотрудника в Directum RX по логину в telegram");
      
      var query = Sungero.Docflow.OfficialDocuments.GetAll(x => x.DocumentKind != null
                                                           && Equals(x.DocumentKind.DocumentType, documentType));
      foreach (var searchTerm in searchTerms)
        query = query.Where(x => (x.Name != null && x.Name != string.Empty && x.Name.ToLower().Contains(searchTerm)));

      if (query.Count() > 500)
        return Structures.Module.EntitiesWithError.Create(null, "Пожалуйста, уточните запрос");
      
      var entities = query.OrderBy(x => x.Name)
        .Take(200)
        .ToList()
        .Where(x => x.AccessRights.CanRead(employee))
        .Select(x => TelegramBot.Structures.Module.EntityInfo.Create(x.Name, x.Id))
        .ToList();
      
      return Garant.TelegramBot.Structures.Module.EntitiesWithError.Create(entities, string.Empty);
    }
    
    /// <summary>
    /// Получить информацию о версии документа для телеграм-бота по его ИД.
    /// </summary>
    /// <param name="documentId">ИД документа.</param>
    /// <returns>Информация о версии документа для телеграм-бота.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public Structures.Module.IVersionInfo GetDocumentVersion(long documentId)
    {
      var document = Sungero.Docflow.OfficialDocuments.GetAll(x => x.Id == documentId).FirstOrDefault();
      if (document == null || document.LastVersion == null)
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
    public void SetChatId(string username, string chatId)
    {
      var botUser = BotUsers.GetAll(x => x.Username == username).FirstOrDefault();
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
    [Public(WebApiRequestType = RequestType.Post)]
    public void CreateBotUser(string mail, string chatId, string username)
    {
      var employee = Sungero.Company.Employees.GetAll(x => x.Email == mail).FirstOrDefault();
      if (employee != null)
      {
        var botUser = BotUsers.GetAll(x => x.Status == Garant.TelegramBot.BotUser.Status.Active && x.Username == username).FirstOrDefault();
        if (botUser == null)
        {
          botUser = BotUsers.Create();
          botUser.Username = username;
          botUser.ChatId = chatId;
          botUser.Employee = employee;
          botUser.Save();
        }
      }
    }
    
    /// <summary>
    /// Отправить заявку в работу.
    /// </summary>
    /// <param name="requestText">Текст заявки.</param>
    /// <param name="EmployeeId">Логин пользователя чат-бота, от имени которого отправляется заявка.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void CreateRequest(string requestText, string username, byte[] file, string filename)
    {
      var subject = Sungero.Docflow.PublicFunctions.Module.CutText(Garant.TelegramBot.Resources.RequestSubjectFormat(requestText), Sungero.Workflow.SimpleTasks.Info.Properties.Subject.Length);
      var requestsResponsibleRole = Roles.GetAll(x => x.Sid == Constants.Module.Roles.RequestsResponsibleRoleGuid).FirstOrDefault();
      var author = Functions.BotUser.GetEmployeeByUsername(username);
      if (requestsResponsibleRole != null && author != null)
      {
        var task = Sungero.Workflow.SimpleTasks.Create(subject, requestsResponsibleRole);
        task.ActiveText = requestText;
        task.Author = author;
        if (file != null && file.Any() && !string.IsNullOrEmpty(filename))
        {
          var document = Sungero.Docflow.SimpleDocuments.Create();
          document.Name = Sungero.Docflow.PublicFunctions.Module.CutText(filename, Sungero.Docflow.SimpleDocuments.Info.Properties.Name.Length);
          document.Author = author;
          document.PreparedBy = author;
          document.Department = author.Department;
          document.AccessRights.Grant(author, DefaultAccessRightsTypes.FullAccess);
          using (var stream = new System.IO.MemoryStream(file))
          {
            var extension = filename.Split('.').LastOrDefault();
            document.CreateVersionFrom(stream, extension ?? string.Empty);
          }
          document.Save();
          task.Attachments.Add(document);
        }
        task.Start();
      }
    }
  }
}