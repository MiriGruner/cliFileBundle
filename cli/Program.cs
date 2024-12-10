using System.CommandLine;
using System.CommandLine.Invocation;

List<string> possibleLanguages = new List<string> { "cs", "java", "js", "cpp", "html", "css", "ts" };
var excludedDirectories = new[] { "bin", "debug", "obj" };

var createRspCommand = new Command("create-rsp", "Create a response file with the command");
var bundleCommand = new Command("bundle", "Bundle code files to a single file");

var bundleOption = new Option<FileInfo>(new[] { "-o" ,"--output"}, "File path and name") {  };
var languageOption = new Option<List<string>>(new[] { "--language", "-l" })
{
    IsRequired = true
};

var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Add notes with file paths and names.");
var sortOption = new Option<bool>(  new[] { "-s", "--sort" }, "Sort files based on extension");
var removeEmptyLinesOption = new Option<bool>(new[] { "-e", "--removeempty" }, "Remove empty lines from the file") ;
var authorOption = new Option<string>(new[] { "-a"  ,"--author"}, "Write who created the file") {  };

bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((output, languages, includeNote, sortFiles, removeEmptyLines, author) =>
{
    try
    {
        //var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
        //    .Where(file => !excludedDirectories.Any(dir => file.Contains(dir, StringComparison.OrdinalIgnoreCase)))
        //    .ToArray();


        string[] files = Array.Empty<string>();
        if (languages.Contains("all"))
        {
            files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".rsp", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        }
        else
        {
            foreach (var l in languages)
            {
                var fileExtensions = l switch
                {
                    "c#" => new[] { "cs" },
                    "java" => new[] { "*.java" },
                    "python" => new[] { "*.py" },
                    "javascript" => new[] { "*.js" },
                    "c++" => new[] { "*.cpp" },
                    "c" => new[] { "*.c" },
                    "html" => new[] { "*.html" },
                    _ => Array.Empty<string>()
                };

                foreach (var ext in fileExtensions)
                {
                    files = files.Concat(Directory.GetFiles(Directory.GetCurrentDirectory(), ext, SearchOption.AllDirectories)).ToArray();
                }
            }
        }

        //if (languages.ToLower() != "all")
        //{
        //    var selectedLanguages = languages.Split(',').Select(lang => lang.Trim().ToLower());
        //    files = files.Where(file => selectedLanguages.Contains(Path.GetExtension(file).TrimStart('.').ToLower())).ToArray();
        //}

        if (sortFiles)
            files = files.OrderBy(file => Path.GetExtension(file)).ThenBy(file => file).ToArray();

        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(author))
            lines.Add($"// Author: {author}");

        if (includeNote)
            lines.Add($"// Folder path: {Directory.GetCurrentDirectory()}");

        foreach (var file in files)
        {
            var fileLines = File.ReadAllLines(file).ToList();
            if (removeEmptyLines)
                fileLines = fileLines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

            lines.Add($"// File: {file}");
            lines.AddRange(fileLines);
            lines.Add(Environment.NewLine);
        }

        File.WriteAllLines(output.FullName, lines);
        Console.WriteLine($"Code files grouped successfully into '{output.FullName}'.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, bundleOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

createRspCommand.SetHandler(() =>
{
    Console.Write("Enter output file name (with .rsp extension): ");
    string filename = Console.ReadLine();
    Console.Write("Enter output file name (with .txt extension): ");
    string filenametxt = Console.ReadLine();

    Console.Write("Enter language (enter 'all' for all): ");
    string languages = Console.ReadLine();

    Console.Write("Include note? (true/false): ");
    bool includeNote = bool.Parse(Console.ReadLine());

    Console.Write("Sort files by file type? (true/false): ");
    bool sortFiles = bool.Parse(Console.ReadLine());

    Console.Write("Remove empty lines? (true/false): ");
    bool removeEmptyLines = bool.Parse(Console.ReadLine());

    Console.Write("Enter author name (optional): ");
    string author = Console.ReadLine();

    var command = $"bundle --output \"{filenametxt}\" --language {languages}";
    if (includeNote) command += " --note";
    if (sortFiles) command += " --sort";
    if (removeEmptyLines) command += " --removeempty";
    if (!string.IsNullOrWhiteSpace(author)) command += $" --author \"{author}\"";

    File.WriteAllText(filename, command);
    Console.WriteLine($"Response file '{filename}' created successfully.");
});

var rootCommand = new RootCommand("Root command for file bundle CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

await rootCommand.InvokeAsync(args);