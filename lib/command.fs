namespace Example.Rabbit.Lib

open CommandLine

module Command =
 
    type CommonOptions() = 
        let mutable _logLevel = "info"
        let mutable _wait = "false"
    with
        [<Option("logLevel",Required=false, HelpText="Logging level: verbose, debug, info (default), warn or error")>]
        member this.LogLevel with get () = _logLevel and set newV = _logLevel <- newV  
    
        [<Option("wait",Required=false, HelpText="Wait for keypress on finish")>]
        member this.Wait with get () = _wait and set newV = _wait <- newV  
    