namespace Example.Rabbit.Server

open Microsoft.Extensions.Logging 

open CommandLine

open Example.Rabbit.Lib.Command 

[<Verb("info")>]
type InfoOptions() =
    inherit CommonOptions() 
with    
    static member Make () = 
        new InfoOptions() 

    member this.Execute (logger:ILogger) =

        use operation = 
            logger.BeginScope( "execute {Options}", this ) 
            
        logger.LogInformation( "UserName {UserName}", System.Environment.UserName )
        
        0     
