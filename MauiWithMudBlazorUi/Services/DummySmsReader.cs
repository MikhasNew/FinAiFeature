using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Services;
using System.Globalization;
using Sms = EfcToXamarinAndroid.Core.Sms;

namespace MauiAppWithMudBlazor.Services;

public class DummySmsReader : ISmsReader
{
    public event EventHandler<Sms> SmsReceived;
    public async Task<List<Sms>> GetAllSmsAsync(List<BankConfiguration> addressFilter) => await GenereteSms(100);
    public void StartListening(List<BankConfiguration> addressFilter) { }
    public void StopListening() { }

    public async Task<List<Sms>> GenereteSms(int count)
    {
        List<Sms> smsList = SmsGenerator.GenerateSmsList(100);
        return smsList;
    }

}

public class SmsGenerator
{
    private static readonly Random random = new Random();
    private static readonly string[] folders = { "inbox", "sent", "draft" };
    private static readonly string[] words =
    {
        "Lorem", "ipsum", "dolor", "sit", "amet", "consectetur",
        "adipiscing", "elit", "sed", "do", "eiusmod", "tempor",
        "incididunt", "ut", "labore", "et", "dolore", "magna"
    };

    public static List<Sms> GenerateSmsList(int count)
    {
        var smsList = new List<Sms>();
        var startDate = DateTime.Now.AddDays(-365);

        for (int i = 0; i < count; i++)
        {
            var sms = new Sms
            {
                Id = GenerateId(),
                Address = GeneratePhoneNumber(),
                Msg = GenerateMessage(),
                ReadState = GenerateReadState(),
                Time = GenerateTime(startDate),
                FolderName = GenerateFolder()
            };
            smsList.Add(sms);
        }

        return smsList;
    }
    // Шаблоны для генерации платежных сообщений
    private static readonly string[] paymentTemplates =
    {
        "OPLATA: {0} BYN KARTA #{1} DATA {2} ERIP>BY MCC:{3} OSTATOK {4} BYN",
        "SPISANIE: {0} BYN KARD #{1} {2} ERIP>BY MCC:{3} BALANCE: {4} BYN",
        "PLATEZH: {0} BYN KARTA #{1} DATA {2} TERMINAL MCC:{3} OSTATOK {4} BYN",
        "OPLATA USLUGI: {0} BYN KARTA #{1} {2} ERIP MCC:{3} BALANCE {4} BYN"
    };

    private static readonly string[] merchants =
    {
        "GROCERY STORE", "SUPERMARKET", "ONLINE SHOP", "CAFE", "RESTAURANT",
        "TRANSPORT", "FUEL STATION", "PHARMACY", "CLOTHING STORE", "SERVICE"
    };
    private static string GenerateId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }

    private static string GeneratePhoneNumber()
    {
        //return "+79" + random.Next(100000000, 999999999).ToString();
        return "Mbank";
    }

    private static string GenerateMessage()
    {
        // Генерация суммы платежа
        double amount = Math.Round(random.NextDouble() * 100 + 1, 2);

        // Генерация номера карты
        string cardNumber = random.Next(1000, 9999).ToString();

        // Генерация даты и времени операции
        DateTime operationDate = GenerateRandomDateTime(DateTime.Now.AddDays(-365));
        string dateTimeStr = operationDate.ToString("dd.MM.yyyy HH:mm:ss");

        // Генерация MCC кода
        string mcc = random.Next(4000, 6000).ToString();

        // Генерация остатка (больше суммы платежа)
        double balance = Math.Round(amount + random.NextDouble() * 200, 2);

        // Выбор шаблона и подстановка значений
        string template = paymentTemplates[random.Next(paymentTemplates.Length)];

        // Иногда добавляем название мерчанта
        if (random.Next(100) > 70) // 30% случаев
        {
            string merchant = merchants[random.Next(merchants.Length)];
            template = template.Replace("ERIP", merchant);
        }

        return string.Format(template,
            amount.ToString("F2", CultureInfo.InvariantCulture),
            cardNumber,
            dateTimeStr,
            mcc,
            balance.ToString("F2", CultureInfo.InvariantCulture));
    }
    private static DateTime GenerateRandomDateTime(DateTime startDate)
    {
        int randomDays = random.Next(30);
        int randomHours = random.Next(24);
        int randomMinutes = random.Next(60);
        int randomSeconds = random.Next(60);

        return startDate.AddDays(randomDays)
                       .AddHours(randomHours)
                       .AddMinutes(randomMinutes)
                       .AddSeconds(randomSeconds);
    }
    private static string GenerateReadState()
    {
        return random.Next(2).ToString();
    }

    private static string GenerateTime(DateTime startDate)
    {
        int randomDays = random.Next(365);
        int randomHours = random.Next(24);
        int randomMinutes = random.Next(60);
        DateTime randomDate = startDate.AddDays(randomDays).AddHours(randomHours).AddMinutes(randomMinutes);

        return randomDate.ToString("dd.MM.yyyy HH:mm");
    }

    private static string GenerateFolder()
    {
        return folders[random.Next(folders.Length)];
    }
}