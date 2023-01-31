using LoggingLib;

Logging.Broadcast += args => Console.WriteLine(args.Log);


Logging.Warn("As the path to the root log file has not been set by default, this log will not be recorded to file.");

Logging.Root.Path = "./logs/test.log";

Logging.Debug("A debug message.");
Logging.Info("I'm root.");
Logging.Warn("An unexpected error occurred.");
Logging.PublishLog(LogLevel.Crit, "Critical error!");
Logging.PublishLog("root", LogLevel.Crit, "Another critical error!");


var branch1 = new Logger("branch1")
{
    Level = LogLevel.Debug,
    Format = "(linenum)\t(time:T)\t(level)\t(name):\t(message)",
    Path = "./logs/test_branch.log"
};
Logging.RegisterLogger(branch1);

Logging.Warn("branch1", "Hello?");


var branch2 = Logging.GetLogger("branch2");
branch2.Path = "./logs/test_branch.log";
branch2.Format = "[(linenum)]\t(level)\t(name):\t(message)";

Logging.Critical("branch2", "Not good");