# LoggingLib
 A python like logging class library. This lib can publish customized log event and output log file.  
  
# Memo
 Subscribe log broadcast.  
 `Logging.Broadcast += e => Console.WriteLine(e.Log);`  
  
 Publish a log(Level = error).  
 `Logging.Logger.Error("An unexpected error occurred.");`  
>An unexpected error occurred.  
  
 The default lowest importance level is Warn. The level lower than this will not be published to Broadcast event.  
`Logging.Logger.Info("I'm root.");`  
>(Nothing here)  
  
 Lowest importance level or other setting can be changed.  
 `Logging.Logger.BasicConfig(new LogBasicConfig { Level = LogLevel.Info });`  
 `Logging.Logger.Info("I'm root.");`  
>I'm root.  
  
```
Logging.Logger.BasicConfig(new LogBasicConfig  
{  
    Level = LogLevel.Info,  
    FileName = @"logs\root.log",  
    Format = "(level)|(name) (time:T) (message)",  
});  
Logging.Logger.Info("A new style.");  
```
>INFO|root 10:00:00 A new style.  
  
 The instance of logger can be created.  
```
var logger = Logging.GetLogger("Branch");  
logger.Error("I am branch. I'm angry.");  
```
>ERROR Branch: I am branch. I'm angry.  