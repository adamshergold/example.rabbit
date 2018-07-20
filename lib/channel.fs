namespace Example.Rabbit.Lib

open Microsoft.Extensions.Logging 

open RabbitMQ.Client
open RabbitMQ.Client.Exceptions

open Polly 

open Example.Rabbit.Lib 

type Channel( logger:ILogger, connection: Connection ) =

    let mutable channel : IModel option = None
            
    static member Make( logger, connection ) = 
        new Channel( logger, connection ) 
        
    member this.Dispose () =
        if channel.IsSome then  
            channel.Value.Close()
            channel.Value.Dispose()
        
    member this.Value 
        with get () = 
            lock this ( fun () ->
                if channel.IsNone then
                    logger.LogDebug( "Creating channel" )
                    channel <- Some (connection.Value.CreateModel())
                    
                if channel.IsNone then 
                    logger.LogError( "Could not establish channel" )
                    failwithf "Could not establish channel"  
                else 
                    channel.Value )                
            
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()