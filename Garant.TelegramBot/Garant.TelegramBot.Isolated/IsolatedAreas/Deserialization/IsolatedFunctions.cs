using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sungero.Core;
using Garant.TelegramBot.Structures.Module;

namespace Garant.TelegramBot.Isolated.Deserialization
{
  public class IsolatedFunctions
  {
    /// <summary>
    /// Десериализовать Json с информацией о файлах из чат-бота.
    /// </summary>
    /// <returns>Список структур с информацией о файлах.</returns>
    [Public]
    public virtual List<Structures.Module.IFileInfo> DesirializeDocumentsInfo(string jsonValue)
    {
      var files = new List<Structures.Module.IFileInfo>();

      if (!string.IsNullOrEmpty(jsonValue))
      {
        try
        {
          JArray jArray = JArray.Parse(jsonValue);
          foreach (var item in jArray)
          {
            var file = JsonConvert.DeserializeObject<Structures.Module.FileInfo>(item.ToString());
            if (file != null)
              files.Add(file);
          }
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      return files;
    }
  }
}