using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Newtonsoft.Json;
using NATS.Client;
using System.Text;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IConnection _natsConnection;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        try
        {
            // Создание подключения к NATS
            Options options = ConnectionFactory.GetDefaultOptions();
            options.Url = "127.0.0.1:4222";
            _natsConnection = new ConnectionFactory().CreateConnection(options);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при подключении к NATS: {ex.Message}");
            throw;
        }
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);


        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        IDatabase db = _redis.GetDatabase();
        db.StringSet(textKey, text);

        //string rankKey = "RANK-" + id;
        ////TODO: посчитать rank и сохранить в БД по ключу rankKey
        //double rank = CalculateRank(text);
        //string rankJson = JsonConvert.SerializeObject(rank);
        //db.StringSet(rankKey, rankJson); 

        var messageObject = new
        {
            Text = text,
            Id = id
        };
        // Отправка текста в NATS
        string textMessage = JsonConvert.SerializeObject(messageObject);
        byte[] messageBytes = Encoding.UTF8.GetBytes(textMessage);
        _natsConnection.Publish("text.processing", messageBytes);

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        int res = (IsSimilitary(text, id) == true) ? 1 : 0;
        db.StringSet(similarityKey, res);

        return Redirect($"summary?id={id}");
    }

    public bool IsSimilitary(string text, string currentId)
    {
        foreach (var key in _redis.GetServer(_redis.GetEndPoints()[0]).Keys())
        {
            if (key.ToString().StartsWith("TEXT-") && !key.ToString().EndsWith(currentId) && !string.IsNullOrEmpty(text))
            {
                string storedText = _redis.GetDatabase().StringGet(key);
                if (text.Equals(storedText, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Если найден дубликат, возвращаем true
                }
            }
        }

        return false;
    }

    //public double CalculateRank(string text)
    //{
    //    if (string.IsNullOrEmpty(text))
    //    {
    //        return 0;
    //    }

    //    int nonAlphabeticCount = text.Count(c => !IsAlphabetic(c));
    //    int totalCharacters = text.Length;

    //    return ((double)nonAlphabeticCount / totalCharacters);
    //}

    //private static bool IsAlphabetic(char c)
    //{
    //    return char.IsLetter(c) || c == ' ';
    //}
}
