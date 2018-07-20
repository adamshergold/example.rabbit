namespace Example.Rabbit.Lib

open Microsoft.Extensions.Logging 

open RabbitMQ.Client
open RabbitMQ.Client.Exceptions

open Polly 

open Example.Rabbit.Lib 

type Connection( logger:ILogger, factory: Factory ) =

    let tryConnection () =
    
        let onRetry (ex:System.Exception) retry = 
            logger.LogError( "Connection attempt failed {Message} will retry in {Retry}", ex.Message, retry )
            
        let action () =
            logger.LogInformation( "Attempting to CreateConnection()" )
            Some <| factory.CreateConnection()

        let retryDurationProvider (retryAttempt:int) = 
            System.TimeSpan.FromSeconds( (float)retryAttempt )
            
        let retry =                
            Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetry(
                    retryCount = 3,
                    sleepDurationProvider = retryDurationProvider,
                    onRetry = onRetry )

        let fallback =
            let fallbackAction () =
                logger.LogError( "Could not create RabbitMQ connection" ) 
                None 
            Policy<IConnection option>.Handle<BrokerUnreachableException>().Fallback(fallbackAction = fallbackAction)
           
        let policy = 
            fallback.Wrap(retry)
                        
        policy.Execute( fun () -> action() )             
                        
    let mutable connection : IConnection option = None 
    
    static member Make( host ) = 
        new Connection( host ) 
        
    member this.Dispose () =
    
        if connection.IsSome then 
            connection.Value.Close()
            connection.Value.Dispose()            

    member this.OnConnectionShutdown eventArgs = 
        logger.LogWarning( "Received ConnectionShutdown event from RabbitMQ" )
        ()
        
    member this.Value 
        with get () = 
            lock this ( fun () ->
            
                if connection.IsNone then 
                    connection <- tryConnection()
            
                if connection.IsNone then 
                    logger.LogError( "Could not establish RabbitMQ connection" )
                    failwithf "Could not establish RabbitMQ connection" 
                else   
                    connection.Value.ConnectionShutdown.Add( this.OnConnectionShutdown )
                    connection.Value )    
            
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()