namespace Example.Rabbit.Lib

open Microsoft.Extensions.Logging 

open RabbitMQ.Client
open RabbitMQ.Client.Exceptions

open Polly 

type FactoryOptions = {
    Hostname : string 
}
with 
    static member Default = {
        Hostname = "localhost"
    }
    
type Factory( logger:ILogger, options: FactoryOptions ) =

    let factory = 
        let f = new ConnectionFactory()
        f.HostName <- options.Hostname
        f
    
    member val Factory = factory 
    
    member this.CreateConnection () = 
        this.Factory.CreateConnection() 
        
    static member Make( logger, options ) =
        new Factory( logger, options ) 
    
    
