//fib bundle --output bundleFile.txt

using System.CommandLine;
using System.CommandLine.Invocation;
List<string> possibleLanguages = new List<string> { "cs", "java", "js", "cpp", "html", "css","ts" };
var createRspCommand = new Command("create-rsp", "Create a response file with the command");
var bundleCommand = new Command("bundle", "Bundle code files to a single file");
var bundleOption = new Option<FileInfo>("--output", "File path and name");
var languageOption = new Option<string>("--language", "Programming languages to include in the bundle");
languageOption.IsRequired = true;
var noteOption = new Option<bool>("--note", "Include folder path as a comment in the merged file");
var sortOption = new Option<bool>("--sort", "Sort files based on extension");
var removeEmptyLinesOption = new Option<bool>("--removeempty", "Remove empty lines from the file");
var authorOption = new Option<string>("--author", "Write who create the file");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);


bundleCommand.SetHandler((output, languages, includeNote,sortFiles, removeEmptyLines, author) =>
{
    var files = Directory.GetFiles(Directory.GetCurrentDirectory(),"*", SearchOption.TopDirectoryOnly);
    string combinedFile = output.FullName;
try
{
        var lines = new List<string>();
        foreach (var file in files)
        {
            lines.AddRange(File.ReadAllLines(file));
        }
        if (languages.ToLower() != "all")
        {
            var selectedLanguages = languages.Split(',').Select(lang => lang.Trim());
            files = files.Where(file =>
            {
                var extension = Path.GetExtension(file).ToLower();
                var selectedLanguage = possibleLanguages.FirstOrDefault(lang => extension.EndsWith(lang.ToLower()));
                return selectedLanguages.Contains(selectedLanguage);
            }).ToArray();
        }

       
        if (sortFiles)
        {
            files = files.OrderBy(file => Path.GetExtension(file)).ToArray();

        }
        else
        {
            files = files.OrderBy(file => file).ToArray();
        }
       

        if (removeEmptyLines)
        {
            Console.WriteLine("we are here " + lines[0]);
            lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        }
        

        File.WriteAllLines(combinedFile, files.SelectMany(file => File.ReadAllLines(file)));

        if (includeNote)
        {
            string note = "// Folder path: " + Directory.GetCurrentDirectory();
            Console.WriteLine("note: " + note + " heloo " + combinedFile);

            try

            {
                File.AppendAllLines(combinedFile, new string[] { note });
                Console.WriteLine("sucsses");
            }

            catch (Exception ex)

            {

                Console.WriteLine("Error writing to file: " + ex.Message);

            }
        }
        if (author != "")

        {
            string note =" The author is: "+ author;
            File.AppendAllLines(combinedFile, new string[] { note });
        }

        Console.WriteLine($"Code files grouped successfully into '{combinedFile}'.");
    }


    catch (IOException ex)
    {
        Console.WriteLine("the path is invalid or the file is duplicate");
    }
    catch(Exception e)
    {
        Console.WriteLine(e.Message);
    }
}, bundleOption, languageOption,noteOption,sortOption,removeEmptyLinesOption, authorOption);


createRspCommand.SetHandler(async () =>
{
    Console.Write("Enter output file name (with .rsp extension): ");
    string filename = Console.ReadLine();

    Console.Write("Enter language (enter all for all): ");
    string languages = Console.ReadLine();

    Console.Write("Include note? (true/false): ");
    bool includeNote = bool.Parse(Console.ReadLine());

    Console.Write("Sort files by files type? (true/false): ");
    bool sortFiles = bool.Parse(Console.ReadLine());

    Console.Write("Remove empty lines? (true/false): ");
    bool removeEmptyLines = bool.Parse(Console.ReadLine());

    Console.Write("Enter author name:(optional) ");
    string author = Console.ReadLine();

    var command = $"fib bundle --output {filename} --language {languages}";
    if (includeNote)
    {
        command += " --note";
    }
    if (sortFiles)
    {
        command += " --sort";
    }
    if (removeEmptyLines)
    {
        command += " --removeempty";
    }
    if (!string.IsNullOrWhiteSpace(author))
    {
        command += $" --author \"{author}\"";
    }

});

RootCommand root = new RootCommand("Root command for file bundle CLI");
root.AddCommand(bundleCommand);
root.AddCommand(createRspCommand);
root.InvokeAsync(args);

