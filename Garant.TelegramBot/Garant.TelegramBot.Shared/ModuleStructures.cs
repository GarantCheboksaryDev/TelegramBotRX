using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Garant.TelegramBot.Structures.Module
{

  /// <summary>
  /// Структура для получения информации о файлах из чат-бота.
  /// </summary>
  [Public(Isolated=true)]
  partial class FileInfo
  {
    /// <summary>
    /// Тело файла.
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// Имя файла.
    /// </summary>
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Структура для передачи информации о версии документа в чат-бот.
  /// </summary>
  [Public]
  partial class VersionInfo
  {
    public byte[] VersionBody { get; set; }
    public string Extension { get; set; }
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Структура для передачи информации о сущностях в чат-бот.
  /// </summary>
  [Public]
  partial class EntityInfo
  {
    public string Name { get; set; }
    public long Id { get; set; }
  }
  
  /// <summary>
  /// Структура для передачи информации о сущностях и об ошибках в чат-бот.
  /// </summary>
  [Public]
  partial class EntitiesWithError
  {
    public List<Garant.TelegramBot.Structures.Module.IEntityInfo> EntityInfos { get; set; }
    public string Error { get; set; }
  }

}