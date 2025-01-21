//fib bundle --output "C:\Users\user1\Desktop\מוסיקה"
using System.CommandLine;

static List<string> BuildExtension( string[] lng)
{
    List<string> AllLangs = new List<string>() { ".cpp", ".cs", ".py", ".java", ".ts", ".js", ".html", ".json", ".tsx", ".jsx" };
    List<string> result = new List<string>();
    foreach (string l in lng)
        switch (l)
        {
            case "all":
                return AllLangs;
            case "c++":
                    result.Add(".cpp"); result.Add(".h");
                    break;
            case "python":
                    result.Add(".py");
                    break;
            case "c#":
                    result.Add(".cs");
                    break;
            case "java":
                    result.Add(".java");
                    break;
            case "react":
                    result.Add(".ts");
                    result.Add(".js");
                    result.Add(".html");
                    result.Add(".css");
                    result.Add(".tsx");
                    result.Add(".jsx");
                    break;
            case "angular":
                    result.Append(".ts").Append(".js").Append(".html").Append(".css");
                    break;
            default:
                    Console.WriteLine($"ERROR : {l} languahge is not valid");
                    break;
        }
    return result;
}

var outputOption = new Option<FileInfo>("--output", "File path ana name");
var languageOption = new Option<string>("--language", "Languages to include in the bundle.")
{
    IsRequired = true
};
var noteOption = new Option<bool>("--note", "Get the source of the filess");
var sortOption = new Option<string>("--sort", "Sort files by 'name' or 'extension default by name");
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines from the source code");
var authorOption = new Option<string>("--author", "Write the author");

var bundleCommand = new Command("bundle", "Bundle multiple code files into one file.");
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

outputOption.AddAlias("-o");
languageOption.AddAlias("-l");
noteOption.AddAlias("-n");
sortOption.AddAlias("-s");
removeEmptyLinesOption.AddAlias("-r");
authorOption.AddAlias("-a");

bundleCommand.SetHandler((language, output, note, sort, removeEmptyLines, author) =>
{
    try
    {
        
        List<string> fileNames = new List<string>(); // שמות הקבצים שיש לכלול
        string rootDirectory = Directory.GetCurrentDirectory(); // מקום נוכחי

        var excludedDirectories = new List<string> { "bin", "debug", "obj" ,"publish",".vs",
            "release","logs","vendor", "node_modules","venv","lib" };//תקיות שלא יכללו
        List<string> validExtension = BuildExtension(language.Split(','));//סיומות תקינות של קבצים

        //ולידציות על הניתוב של קובץ התוצאה
        string filePath = "";

        if (output == null || string.IsNullOrWhiteSpace(output.FullName))//output=null
        {
            output = new FileInfo(Path.Combine(rootDirectory, "bundle.txt"));
            filePath = output.FullName;
        }

        filePath = output.FullName;

        char[] invalidChars = Path.GetInvalidPathChars();// בדיקה אם יש תווים לא חוקיים
        if (filePath.IndexOfAny(invalidChars) != -1)
        {
            throw new ArgumentException("Error: Output path contains invalid characters!");
        }

        string directory = "";
        if (Path.GetDirectoryName(filePath) != null)
            directory = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(directory))// בדיקה אם הספריה לא קיימת
        {
            throw new DirectoryNotFoundException("Error: The specified directory does not exist!");
        }

        //לא ניתן ניתוב מלא
        if (string.IsNullOrWhiteSpace(output.DirectoryName))
        {
            output = new FileInfo(Path.Combine(rootDirectory, output.Name));
        }

        //אם לא הוגדרו שפות תופיע שגיאה
        if (language == null || !language.Any())
        {
            throw new InvalidOperationException("The --language option is required and must not be empty.");
        }

        foreach (var file in Directory.EnumerateFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(file).ToLower();//סיומת קובץ
            string fileName = Path.GetFileName(file).ToLower() + extension;

            if (validExtension.Contains(extension) && !excludedDirectories.Any(dir => Path.GetDirectoryName(file).Contains(dir, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Including file: {fileName} --- File extension - {extension}");
                fileNames.Add(file);
            }
            //else { Console.WriteLine($"Warning: Invalid file extension - {extension}. - Can`t Including file: {fileName} "); }
        }

        //sort
        if (!string.IsNullOrEmpty(sort) && sort.Equals("extension", StringComparison.OrdinalIgnoreCase))
        {
            fileNames = fileNames.OrderBy(Path.GetExtension).ToList();
        }
        else
        {
            fileNames = fileNames.OrderBy(Path.GetFileName).ToList();
        }



        //פונקציית כתיבת התוצאה
        using (var outputStream = new StreamWriter(output.FullName))
        {

            if (!string.IsNullOrEmpty(author))
                outputStream.WriteLine($"Author: {author}");

            foreach (var file in fileNames)
            {
                outputStream.WriteLine();
                outputStream.WriteLine("**************************************************************************");
                outputStream.WriteLine();
                string content = File.ReadAllText(file);
                if (removeEmptyLines)
                {
                    content = string.Join(Environment.NewLine, content
                        .Split(Environment.NewLine)
                        .Where(line => !string.IsNullOrWhiteSpace(line)));
                }

                outputStream.WriteLine(content);

                if (note)
                {
                    string relativePath = Path.GetRelativePath(rootDirectory, file);
                    outputStream.WriteLine("//source: " + relativePath);
                }
                Console.WriteLine($"File :{file} is added successfully! ");
            }
            if (language.Contains("all"))
                Console.WriteLine("All files included in the bundle.");
            Console.WriteLine($"File :{output} was created successfully! ");
        }
    }
    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine("ERORR: File path is invalid!");
    }

    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine();
        Console.WriteLine($"ERORR:{ex.Message} ");
        Console.WriteLine($"ERORR: Can`t create File :{output} ");
    }
}, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var rspCommand = new Command("create-rsp", "Create a response file with prepared bundle command");

var rootCommand = new RootCommand("Root command for File Bundler CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(rspCommand);

//create_rsp hendler
rspCommand.SetHandler(async () =>
{
    try
    {
        // שימוש בבלוק using כדי להבטיח שהכותב ייסגר בסוף
        using (var writer = new StreamWriter("response.rsp"))
        {
            writer.WriteLine("bundle");

            // שאלה על הניתוב
            Console.WriteLine("Output file path (default: 'bundle.txt'): ");
            var output = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(output))
            {
                output = "bundle.txt"; // ברירת מחדל
            }

            writer.WriteLine("--output");
            writer.WriteLine(output);

            // שאלה על שפת הקוד
            Console.Write("Enter languages you want to include (e.g., 'c#, java'): ");
            var language = Console.ReadLine()?.Trim();
            if (language == null || language.Length == 0)
            {
                throw new Exception("You must specify at least one language or use 'all'!");
            }
            writer.WriteLine("--language");
            writer.WriteLine(language);

            // שאלה על אם להוסיף הערות
            Console.Write("Add source file notes? (yes/no): ");
            var noteResponse = Console.ReadLine()?.Trim().ToLower();
            if (noteResponse == "yes")
            {
                writer.WriteLine("--note");
            }

            // שאלה על סוג המיון
            Console.Write("Sort by file extension? (yes/no): ");
            var isSort = Console.ReadLine()?.Trim().ToLower();
            if (isSort == "yes")
            {
                writer.WriteLine("--sort");
                writer.WriteLine("extension");
            }

            // שאלה על בקשת שורות ריקות
            Console.Write("Remove empty lines? (yes/no): ");
            var removeEmptyLinesResponse = Console.ReadLine()?.Trim().ToLower();
            if (removeEmptyLinesResponse == "yes")
            {
                writer.WriteLine("--remove-empty-lines");
            }

            // שאלה על שם היוצר
            Console.Write("Author name (optional): ");
            var author = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(author))
            {
                writer.WriteLine("--author");
                writer.WriteLine(author);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERORR:{ex.Message} ");
    }
  

    Console.WriteLine("Response file 'response.rsp' created successfully.");

    // הרצת הפקודה bundle באופן אוטומטי
    Console.WriteLine("Executing the 'bundle' command using the response file...");

    // קריאת הפקודה עם קובץ ה-`response.rsp`
    //await rootCommand.InvokeAsync(new[] { "bundle", $"@response.rsp" });
    await rootCommand.InvokeAsync(new[] { "@response.rsp" });
});

rootCommand.InvokeAsync(args);





