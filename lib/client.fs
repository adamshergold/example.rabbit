namespace Example.Rabbit.Lib

open Microsoft.Extensions.Logging 

open RabbitMQ.Client
open RabbitMQ.Client.Exceptions

open Polly 

type Client( logger:ILogger, factory: Factory ) =

    let connection = 
        Connection.Make( logger, factory )
        
    let channel = 
        Channel.Make( logger, connection )
                
    static member Make( logger, factory ) = 
        new Client( logger, factory ) 
        
    member this.Dispose () =
        channel.Dispose()
        connection.Dispose()
        
    member this.Publish (queue:string) (text:string) = 
    
        let qd =
            channel.Value.QueueDeclare( 
                queue = queue, 
                durable = false, 
                exclusive = false, 
                autoDelete = false, 
                arguments = null ) 
    
        let body = 
            System.Text.Encoding.UTF8.GetBytes(text)
            
        channel.Value.BasicPublish( 
            exchange = "", 
            routingKey = queue, 
            basicProperties = null, 
            body = body )
                    
        logger.LogInformation( "Sent {Text} {Body} to {Queue}", text, body, queue )                    
                 
    member this.Call (queue:string) (text:string) (timeoutMilliseconds:int) = 
        
        let replyQueueName = 
            channel.Value.QueueDeclare().QueueName;
            
        let consumer = 
            new Events.EventingBasicConsumer(channel.Value);        

        let props = 
            channel.Value.CreateBasicProperties();
            
        let correlationId = 
            System.Guid.NewGuid().ToString();
        
        props.CorrelationId <- correlationId;
        props.ReplyTo <- replyQueueName;
        
        logger.LogInformation( "Call {Queue} {Text} {ReplyTo} {CorrelationId}", queue, text, props.ReplyTo, correlationId )
        
        let mre =
            new System.Threading.ManualResetEvent(false)
        
        let reply = ref Array.empty
        
        let onReceived (args:Events.BasicDeliverEventArgs) = 
            logger.LogInformation( "Call::onReceived {CorrelationId}", args.BasicProperties.CorrelationId )
            if args.BasicProperties.CorrelationId = correlationId then 
                reply := args.Body 
                logger.LogInformation( "Call::onReceived Signalling" )
                mre.Set() |> ignore
                
        consumer.Received.Add( onReceived )
        
        let body = 
            System.Text.Encoding.UTF8.GetBytes(text)
            
        logger.LogInformation( "Call Sending request" )
                    
        channel.Value.BasicPublish(
            exchange = "",
            routingKey = queue,
            basicProperties = props,
            body = body )

        let consume = 
            channel.Value.BasicConsume(
                consumer = consumer,
                queue = replyQueueName,
                autoAck = true);
        
        let goodResult = 
            Async.AwaitWaitHandle( mre, timeoutMilliseconds ) |> Async.RunSynchronously
            
        if goodResult then  
            System.Text.Encoding.UTF8.GetString( !reply ) 
        else 
            failwithf "Did not receive reply in time!"                         
        
                                                                                                        
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()