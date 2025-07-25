using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ResticTG
{
    internal class Program
    {
        private static string TelegramBotToken;
        private static string TelegramChatId;
        private static string LogFilePath;
        private static string ServerName;
        private static readonly SettingsBot settings = new SettingsBot();
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            // if problem on older ver windows
            // Console.OutputEncoding = Encoding.UTF8;

            try
            {
                // Check if registry values exist
                if (!settings.RegistryValuesExist())
                {
                    Console.WriteLine("Configuration not found in registry. Enter encryption data:");

                    Console.Write("Enter token Telegram bot: ");
                    string token = Console.ReadLine();

                    Console.Write("Enter chat ID: ");
                    string chatId = Console.ReadLine();

                    Console.Write("Enter the path to the log file: ");
                    string logPath = Console.ReadLine()?.Trim('\"', '\'');

                    Console.Write("Enter server name: ");
                    string serverName = Console.ReadLine();

                    // Crypt the entered values and save them to the registry
                    settings.CryptBot(token, chatId, logPath, serverName);

                    // Decrypt for current user
                    settings.Decrypt();
                }
                else
                {
                    // If the data is in the registry, we simply decrypt it
                    settings.Decrypt();
                }

                // Assigning the decrypted values
                TelegramBotToken = settings.DecryptedString1;
                TelegramChatId = settings.DecryptedString2;
                LogFilePath = settings.DecryptedString3;
                ServerName = settings.DecryptedString4;

                // We check that all values are received
                if (string.IsNullOrEmpty(TelegramBotToken) ||
                    string.IsNullOrEmpty(TelegramChatId) ||
                    string.IsNullOrEmpty(LogFilePath) ||
                    string.IsNullOrEmpty(ServerName))
                {
                    throw new Exception("Failed to retrieve all required configuration parameters");
                }

                // Основная логика программы
                string logContent = File.ReadAllText(LogFilePath);
                string errors = CheckForErrors(logContent);
                Console.WriteLine(logContent);

                if (!string.IsNullOrEmpty(errors))
                {
                    string detailedMessage = await BuildErrorMessage(errors);
                    await SendTelegramMessage(detailedMessage);
                }
                else
                {
                    Console.WriteLine("No errors found");
                }
            }
            catch (Exception ex)
            {
                await SendTelegramMessage(await BuildCatchErrorMessage(ex));
            }

            //Console.ReadKey();
        }

        static string CheckForErrors(string logContent)
        {
            StringBuilder errors = new StringBuilder();
            string[] lines = logContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string[] errorPatterns = new[]
            {
        @"\bERROR\b",                          // rclone ERROR and other errors
        @"\berror\b",                          // any mention of"error"
        @"\bfailed\b",                         // Mention "failed"
        @"unable to\b",                        // "unable to"
        @"\bfatal\b",                          // "fatal"
        @"\bcannot\b",                         // "cannot"
        @"no snapshot found\b",                // No snapshot
        @"lock.*failed\b",                     // Problems with locks
        @"repository.*corrupt\b",              // Repository corruption
        @"\btimeout\b",                        // Timeouts
        @"connection.*lost\b",                 // Lost connection
        @"connection.*closed\b",               // Closed connection
        @"Save\(<.*>\).*error",                // Error Save() с retrying
        @"Remove\(<.*>\).*error",              // Error Remove()
        @"Post request.*error",                // POST
        @"HTTP.*5\d{2}",                       // HTTP
        @"Internal Server Error",              // 500
        @"ReadFrom failed"                     // Reading errors
    };

            // Maximum size of all errors (leave room for the rest of the message)
            const int maxErrorsLength = 3000; // Telegram limit ~4096 characters
            int currentLength = 0;
            bool truncated = false;

            // We check each line for errors
            foreach (var line in lines)
            {
                foreach (var errorPattern in errorPatterns)
                {
                    if (Regex.IsMatch(line, errorPattern, RegexOptions.IgnoreCase))
                    {
                        string errorLine = $"- {line.Trim()}\n";

                        // Проверяем, не превысим ли лимит
                        if (currentLength + errorLine.Length > maxErrorsLength)
                        {
                            if (!truncated)
                            {
                                errors.AppendLine("... (the list of errors is truncated)");
                                truncated = true;
                            }
                            break;
                        }

                        errors.Append(errorLine);
                        currentLength += errorLine.Length;
                        break; // Add the line only once
                    }
                }

                if (truncated) break;
            }

            return errors.ToString();
        }

        static async Task<string> BuildErrorMessage(string errors)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Restic ERR ⚠️");
            message.AppendLine($"Server Name: {ServerName}");
            message.AppendLine($"User: {Environment.UserName}");
            message.AppendLine($"Computer: {Environment.MachineName}");
            message.AppendLine($"Time: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            string ipAddress = "Unknown";
            try
            {
                ipAddress = await httpClient.GetStringAsync("https://api.ipify.org");
                ipAddress = ipAddress.Trim();
            }
            catch (Exception ex)
            {
                ipAddress = $"Error getting IP: {ex.Message}";
            }
            message.AppendLine($"Ip-Address: {ipAddress}");

            string freeSpace = "Unknown";
            try
            {
                DriveInfo drive = new DriveInfo("C");
                freeSpace = $"{(drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0)):F2} GB";
            }
            catch { }
            message.AppendLine($"Free space on C:\\ drive: {freeSpace}");
            message.AppendLine($"\nErrors:\n{errors}");

            return message.ToString();
        }

        static async Task<string> BuildCatchErrorMessage(Exception ex)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Restic ERR ⚠️");
            message.AppendLine($"Server Name: {ServerName}");
            message.AppendLine($"User: {Environment.UserName}");
            message.AppendLine($"Computer: {Environment.MachineName}");
            message.AppendLine($"Time: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            string ipAddress = "Unknown";
            try
            {
                ipAddress = await httpClient.GetStringAsync("https://api.ipify.org");
                ipAddress = ipAddress.Trim();
            }
            catch (Exception ipEx)
            {
                ipAddress = $"Error getting IP: {ipEx.Message}";
            }
            message.AppendLine($"Ip-Address: {ipAddress}");

            string freeSpace = "Unknown";
            try
            {
                DriveInfo drive = new DriveInfo("C");
                freeSpace = $"{(drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0)):F2} GB";
            }
            catch { }
            message.AppendLine($"Free space on C:\\ drive: {freeSpace}");
            message.AppendLine($"\nParser error: {ex.Message}");
            message.AppendLine($"Stack trace:\n{ex.StackTrace}");

            return message.ToString();
        }

        static async Task SendTelegramMessage(string message)
        {
            using (var client = new HttpClient())
            {
                string url = $"https://api.telegram.org/bot{TelegramBotToken}/sendMessage";
                var content = new StringContent(
                    $"chat_id={TelegramChatId}&text={Uri.EscapeDataString(message)}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"
                );

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error sending to Telegram: {response.StatusCode}");
                }
            }
        }
    }
}
