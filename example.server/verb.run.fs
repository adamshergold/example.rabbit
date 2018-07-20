namespace Example.Rabbit.Server

open Microsoft.Extensions.Logging 

open CommandLine

//open RabbitMQ.Client

open Example.Rabbit.Lib.Command
open Example.Rabbit.Lib

[<Verb("run")>]
type RunOptions() =
    inherit CommonOptions() 

    let mutable _port : string = ""
    let mutable _host : string = "" 
    let mutable _queue : string = "Queue.1"
with    
    static member Make () = 
        new RunOptions() 

    override this.ToString() = 
        sprintf "HttpOptions(Port=%s,Host=%s)" _port _host  
        
    [<Option("port", Required = false, HelpText = "")>]
    member this.Port with get () = _port and set newV = _port <- newV 

    [<Option("host", Required = false, HelpText = "")>]
    member this.Host with get () = _host and set newV = _host <- newV 

    [<Option("queue", Required = false, HelpText = "")>]
    member this.Queue with get () = _queue and set newV = _queue <- newV 

    member this.Execute (logger:ILogger) =
    
        logger.LogInformation( "Execute {Options}", this )

        let host = 
            sprintf "%s%s" this.Host (if this.Port.Length > 0 then sprintf ":%s" this.Port else "" )

        let options = 
            { FactoryOptions.Default with Hostname = host }
            
        let factory = 
            Factory.Make( logger, options )
            
        use server = 
            Server.Make( logger, factory )
            
        let nMessages = ref 0 
                    
        let cb (body:byte[]) =
            let text = System.Text.Encoding.UTF8.GetString(body)
            System.Threading.Interlocked.Increment(nMessages) |> ignore
            logger.LogInformation( "Message {Text} on {Queue}", text, this.Queue )
            let reply = sprintf "Hello, %s!" text 
            System.Text.Encoding.UTF8.GetBytes(reply) 
                                    
        server.Subscribe this.Queue cb 

        server.Start() 
        
        0