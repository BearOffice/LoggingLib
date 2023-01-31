# LoggingLib
 A simple thread-safe logging class library. This library can publish customized log event and output log files.  
  
# Memo
 Subscribe log's broadcast.  
 `Logging.Broadcast += args => Console.WriteLine(args.Log);`  
<br>

## Use root logger
 Publish a log(Level = error).  
 `Logging.Warn("An unexpected error occurred.");`  
>1       ERROR   root: An unexpected error occurred.  

<br>

 The default lowest importance level is Warn. The level lower than this will not be published to Broadcast event.  
`Logging.Info("I'm root.");`  
>(Nothing here)  

<br>  

 Lowest importance level or other setting can be changed.  
 ```
 Logging.Root.Level = LogLevel.Info
 Logging.Info("I'm root."); 
 ```
>1       INFO    root: I'm root.  

<br>

 Specify log file's path to save logs.
`Logging.Root.Path = "./logs/test.log";`

 Set log's format.
 ```
 Logging.Root.Format = "(level)|(name) (time:T) (message)";
 Logging.Info("A new style.");
 ```
>INFO|root 10:00:00 A new style.  

<br>

## Use customized logger
```
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
branch2.Format = "[(linenum)](level)\t(name):\t(message)";

Logging.Critical("branch2", "Not good");
```
>1       21:16:30        WARN    branch1:        Hello?

>[2]     CRIT    branch2:        Not good
