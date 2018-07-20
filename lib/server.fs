namespace Example.Rabbit.Lib

open Microsoft.Extensions.Logging 

open RabbitMQ.Client
open RabbitMQ.Client.Exceptions

open Polly 

type Server( logger:ILogger, factory: Factory ) =

    let connection = 
        Connection.Make( logger, factory )
    
    let channel = 
        Channel.Make( logger, connection )
    
    let mre = 
        new System.Threading.ManualResetEvent(false)
        
    static member Make( host ) = 
        new Server( host ) 
        
    member this.Dispose () =
        channel.Dispose()
        connection.Dispose()    
        
    member this.Subscribe (queue:string) (cb:byte[]->byte[]) = 
    
        let qd =
            channel.Value.QueueDeclare(
                queue = queue,
                durable = false,
                exclusive = false,
                autoDelete = false,
                arguments = null )

        logger.LogInformation( "Subscribe QueueDeclare {QueueDeclare}", qd )
                        
        let consumer = 
            Events.EventingBasicConsumer( channel.Value ) 
            
        let onReceived (args:Events.BasicDeliverEventArgs) =
        
            logger.LogInformation( "Subscribe::onReceived {Props}", args.BasicProperties )
                        
            let reply = 
                cb args.Body

            let props = 
                args.BasicProperties 
             
            let replyProps = 
                channel.Value.CreateBasicProperties()
                
            replyProps.CorrelationId <- props.CorrelationId
            
            logger.LogInformation( "Subscribe::onReceived ReplyTo {ReplyTo}", props.ReplyTo )
            
            channel.Value.BasicPublish(
                exchange = "", 
                routingKey = props.ReplyTo,
                basicProperties = replyProps, 
                body = reply )
                             
            channel.Value.BasicAck(
                deliveryTag = args.DeliveryTag,
                multiple = false )
                        
        consumer.Received.Add( onReceived )           
                    
        channel.Value.BasicConsume(
            queue = queue,
            autoAck = false,
            consumer = consumer ) |> ignore //why?
                 
    member this.Start () =
        logger.LogInformation( "Starting Server")
        Async.AwaitWaitHandle mre |> Async.RunSynchronously |> ignore
        
    member this.Stop () =
        logger.LogInformation( "Stopping Server")
        mre.Set() |> ignore
                                                                                                                                  
    interface System.IDisposable
        with 
            member this.Dispose () = 
                this.Dispose()