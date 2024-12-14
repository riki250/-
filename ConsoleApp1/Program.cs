using System.CommandLine;
using System.IO; // מייבא את הספרייה לעבודה עם קבצים
using System.Linq; // מייבא את הספרייה לשימוש ב-LINQ

var bundleOption = new Option<FileInfo>("--output", "file path and name");
var languageOption = new Option<string>("--language", "שפות תכנות לכלול (מופרדות בפסיקים או 'all')") { IsRequired = true };
var noteOption = new Option<bool>("--note", "האם לרשום את מקור הקוד כהערה בקובץ ה-bundle?") { IsRequired = false };
var sortOption = new Option<string>("--sort", "סדר קבצים לפי 'name' או 'type'. ברירת מחדל: name.") { IsRequired = false };
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "האם למחוק שורות ריקות?") { IsRequired = false };
var authorOption = new Option<string>("--author", "שם יוצר הקובץ") { IsRequired = false };

var bundleCommand = new Command("bundle", "מאחד קבצי קוד לקובץ אחד");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((output, language, note, sort, removeEmptyLines, author) => {
    try
    {
        // פתיחת קובץ פלט לכתיבה
        using (var outputStream = new FileStream(output.FullName, FileMode.Create, FileAccess.Write))
        {
            // אם הוזן שם יוצר, הוסף הערה בתחילת הקובץ
            if (!string.IsNullOrEmpty(author))
            {
                var authorComment = $"// יוצר: {author}\n";
                byte[] authorBytes = System.Text.Encoding.UTF8.GetBytes(authorComment);
                outputStream.Write(authorBytes, 0, authorBytes.Length);
            }

            Console.WriteLine($"Included languages: {language}");
            // כאן תוסיף את הלוגיקה לאיחוד הקבצים לפי השפות והאפשרויות הנוספות
        }

        Console.WriteLine($"הקבצים אוחדו בהצלחה לקובץ {output.FullName}");
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("שגיאה: נתיב הקובץ אינו תקף.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"אירעה שגיאה: {ex.Message}");
    }
}, bundleOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var createRspCommand = new Command("create-rsp", "יוצר קובץ תגובה עם פקודה מוכנה");
var rspOutputOption = new Option<FileInfo>("--rsp-output", "נתיב ושם הקובץ לתגובה") { IsRequired = true };

createRspCommand.AddOption(rspOutputOption);

createRspCommand.SetHandler((rspOutput) =>
{
    Console.WriteLine("נא הזן את הנתיב לקובץ הפלט:");
    var outputPath = Console.ReadLine();

    Console.WriteLine("נא הזן את שפת התכנות (מופרדות בפסיקים או 'all'):");
    var languages = Console.ReadLine();

    Console.WriteLine("האם לרשום את מקור הקוד כהערה בקובץ הבנדל? (yes/no):");
    var noteInput = Console.ReadLine();
    var note = noteInput?.ToLower() == "yes";

    Console.WriteLine("נא הזן את סוג הסדר (name/type):");
    var sortInput = Console.ReadLine();

    Console.WriteLine("האם למחוק שורות ריקות? (yes/no):");
    var removeEmptyLinesInput = Console.ReadLine();
    var removeEmptyLines = removeEmptyLinesInput?.ToLower() == "yes";

    Console.WriteLine("נא הזן את שם יוצר הקובץ:");
    var author = Console.ReadLine();

    // יצירת הפקודה המלאה
    var command = $"dotnet run -- --output=\"{outputPath}\" --language=\"{languages}\" --note={note.ToString().ToLower()} --sort=\"{sortInput}\" --remove-empty-lines={removeEmptyLines.ToString().ToLower()} --author=\"{author}\"";

    // כתיבה לקובץ התגובה
    File.WriteAllText(rspOutput.FullName, command);
    Console.WriteLine($"קובץ התגובה נוצר: {rspOutput.FullName}");
}, rspOutputOption);

var rootCommand = new RootCommand("פקודה ראשית עבור כלי האיחוד של קבצים");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

// הרצת הפקודה עם הארגומנטים שסופקו
rootCommand.Invoke(args);