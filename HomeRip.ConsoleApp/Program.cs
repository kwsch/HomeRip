using HomeRip.Lib;

Console.WriteLine("Hello, World!");

var currentDirectory = Directory.GetCurrentDirectory();
var files = Directory.GetFiles(currentDirectory, "*.perbin");
RawRipper.RipFiles<PersonalTable, PersonalInfo>(files, x => x.Table);

PrettyRipper.RipFiles(files);
