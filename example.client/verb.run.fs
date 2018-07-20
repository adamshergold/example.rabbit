namespace Example.Rabbit.Client

open Microsoft.Extensions.Logging 

open CommandLine

open Example.Rabbit.Lib
open Example.Rabbit.Lib.Command 

[<Verb("run")>]
type RunOptions() =
    inherit CommonOptions() 

    let mutable _port : string = ""
    let mutable _host : string = "" 
with    
    static member Make () = 
        new RunOptions() 

    override this.ToString() = 
        sprintf "HttpOptions(Port=%s,Host=%s)" _port _host  
        
    [<Option("port", Required = false, HelpText = "")>]
    member this.Port with get () = _port and set newV = _port <- newV 

    [<Option("host", Required = false, HelpText = "")>]
    member this.Host with get () = _host and set newV = _host <- newV 

    member this.Execute (logger:ILogger) =
    
        logger.LogInformation( "Execute {Options}", this )

        let host = 
            sprintf "%s%s" this.Host (if this.Port.Length > 0 then sprintf ":%s" this.Port else "" )

        let options = 
            { FactoryOptions.Default with Hostname = host }
            
        let factory = 
            Factory.Make( logger, options )
            
        use client = 
            Client.Make( logger, factory )
            
        //client.Publish "Queue.1" "Hello, world!"
        let reply = 
            client.Call "Queue.1" "Adam" 1000
            
        logger.LogInformation( "Reply {Reply}", reply )
                       
        0