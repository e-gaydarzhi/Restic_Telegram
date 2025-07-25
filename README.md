```
────────────────────────────────────────────────────────────

//  █████████████████████████████████    ███████████████    ██████████████████ ████████    ███ 
//  ██   ███    ██       ██  ███            ██  ██    ██    ██   ██     ██   ███   █████  ████ 
//  ███████████ ███████  ██  ███            ██  █████ ██    ███████   ████████████████ ████ ██ 
//  ██   ███         ██  ██  ███            ██  ██    ██    ██   ██    ███   ███   ███  ██  ██ 
//  ██   ██████████████  ██  ████████       ██  ██████████████████████████   ███   ███      ██ 
//                                                                                             
//                                                                                                     
────────────────────────────────────────────────────────────
        Restic Telegram Console Notifier [Win]
────────────────────────────────────────────────────────────

[>] Console utility to send Telegram alerts for failed backups
[>] Built in C# .NET Framework 4.6
[>] No dependencies. No mercy.

────────────────────────────────────────────────────────────
 USAGE EXAMPLE (Batch Mode)
────────────────────────────────────────────────────────────

restic.exe -r C:\Destinationbackup C:\Source --use-fs-snapshot >> C:\mirvari\LogName.txt 2>&1
restic.exe -r C:\Destinationbackup forget --keep-last 5 --prune >> C:\mirvari\LogName.txt 2>&1
start C:\Mirvari\Restic Telegram.exe

💥 At the end of the backup run — launch `Restic Telegram.exe`
🧠 It reads the log file and sniffs for failure keywords
📡 If failed — sends a Telegram alert to your group

────────────────────────────────────────────────────────────
 FEATURES
────────────────────────────────────────────────────────────

✔ Windows-only lightweight console app
✔ Sends alerts to Telegram group/channel
✔ Encrypted API key and chat ID (stored in Registry)
✔ No internet libs or external dependencies
✔ Logs? We eat them.

────────────────────────────────────────────────────────────
 FIRST RUN
────────────────────────────────────────────────────────────

Launch as Administrator — just once.

Then follow prompts:

[?] Enter token Telegram bot:        (ex: 1561651561:ABCDEF123456...)
[?] Enter chat ID:                   (ex: -34792156)
[?] Enter path to log file:          (ex: C:\mirvari\LogName.txt)
[?] Enter server name:               (ex: Backup Node Alpha)

All data encrypted & saved to registry.

────────────────────────────────────────────────────────────
(☞ ͡° ͜ʖ ͡°)☞
────────────────────────────────────────────────────────────

```

