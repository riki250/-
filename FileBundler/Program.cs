using System.CommandLine;
using System.IO;
using System.Linq;

var bundleOption = new Option<FileInfo>("--output", "file path and name");
var inputDirectoryOption = new Option<DirectoryInfo>("--input-directory", "directory to search for code files") { IsRequired = true };
var languageOption = new Option<string>("--language", "שפות תכנות לכלול (מופרדות בפסיקים או 'all')") { IsRequired = true };
var noteOption = new Option<bool>("--note", "האם לרשום את מקור הקוד כהערה בקובץ ה-bundle?") { IsRequired = false };
var sortOption = new Option<string>("--sort", "סדר קבצים לפי 'name' או 'type'. ברירת מחדל: name.") { IsRequired = false };
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "האם למחוק שורות ריקות?") { IsRequired = false };
var authorOption = new Option<string>("--author", "שם יוצר הקובץ") { IsRequired = false };

inputDirectoryOption.AddAlias("-ido");
languageOption.AddAlias("-l");
noteOption.AddAlias("-n");
sortOption.AddAlias("-s");
removeEmptyLinesOption.AddAlias("-rel");
authorOption.AddAlias("-a");

var bundleCommand = new Command("bundle", "מאחד קבצי קוד לקובץ אחד");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(inputDirectoryOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);
bundleCommand.SetHandler((output, inputDirectory, language, note, sort, removeEmptyLines, author) => {
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

            Console.WriteLine($"כולל שפות: {language}");

            // מציאת קבצי Java בתיקיה הנבחרת
            var files = Directory.GetFiles(inputDirectory.FullName, $"*.{language}", SearchOption.AllDirectories);
            Console.WriteLine($"נמצאו {files.Length} קבצים");

            foreach (var file in files)
            {
                Console.WriteLine($"מעבד את הקובץ: {file}");
                var content = File.ReadAllText(file);
                if (removeEmptyLines)
                {
                    content = string.Join("\n", content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                }
                if (note)
                {
                    var noteComment = $"// מקור: {file}\n";
                    byte[] noteBytes = System.Text.Encoding.UTF8.GetBytes(noteComment);
                    outputStream.Write(noteBytes, 0, noteBytes.Length);
                }
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content);
                outputStream.Write(fileBytes, 0, fileBytes.Length);
            }

            Console.WriteLine($"הקבצים אוחדו בהצלחה לקובץ {output.FullName}");
        }
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("שגיאה: נתיב הקובץ אינו תקף.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"אירעה שגיאה: {ex.Message}");
    }
}, bundleOption, inputDirectoryOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var createRspCommand = new Command("create-rsp", "יוצר קובץ תגובה עם פקודה מוכנה");
var rspOutputOption = new Option<FileInfo>("--rsp-output", "נתיב ושם הקובץ לתגובה") { IsRequired = true };
createRspCommand.AddOption(rspOutputOption);

createRspCommand.SetHandler((rspOutput) => {
    Console.WriteLine("נא הזן את הנתיב לקובץ הפלט:");
    var outputPath = Console.ReadLine();
    Console.WriteLine("נא הזן את נתיב התיקייה להכנסת קבצים:");
    var inputDirectory = Console.ReadLine();
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
    var command = $"dotnet run -- --input-directory=\"{inputDirectory}\" --output=\"{outputPath}\" --language=\"{languages}\" --note={note.ToString().ToLower()} --sort=\"{sortInput}\" --remove-empty-lines={removeEmptyLines.ToString().ToLower()} --author=\"{author}\"";
    // כתיבה לקובץ התגובה
    File.WriteAllText(rspOutput.FullName, command);
    Console.WriteLine($"קובץ התגובה נוצר: {rspOutput.FullName}");
}, rspOutputOption);

var rootCommand = new RootCommand("פקודה ראשית עבור כלי האיחוד של קבצים");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

// הרצת הפקודה עם הארגומנטים שסופקו
rootCommand.Invoke(args);
