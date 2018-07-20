namespace Example.Rabbit.Lib

open Microsoft.Extensions.Logging 
open Serilog 

module Logging = 

    type Parameters = {
        Level : string option
        ToConsole : bool
    }
    with 
        static member Default = {
        
            Level = Some "debug"
            ToConsole = true 
        }
        
    let SerilogLevel (s:string) = 
        match s.ToLower() with
        | v when v = "verbose" -> Serilog.Events.LogEventLevel.Verbose
        | v when v = "debug" -> Serilog.Events.LogEventLevel.Debug
        | v when v.StartsWith( "info" ) -> Serilog.Events.LogEventLevel.Information
        | v when v.StartsWith( "warn" )  -> Serilog.Events.LogEventLevel.Warning
        | v when v = "error" -> Serilog.Events.LogEventLevel.Error
        | v when v = "fatal" -> Serilog.Events.LogEventLevel.Fatal
        | _ -> failwithf "Unsupported logging level [%s]" s                     
     
    let CreateFactory (parameters:Parameters) = 

        let levelSwitch = 
            Serilog.Core.LoggingLevelSwitch() 

        let loggerFactory =
        
            let logger =
            
                let config =
                    Serilog.LoggerConfiguration()
                        .MinimumLevel.ControlledBy(levelSwitch)
                        
                let config = 
                    if parameters.ToConsole then                        
                        config.WriteTo.Console( 
                            outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {Properties}{NewLine}{Exception}" )
                    else 
                        config
                                     
                config.CreateLogger()             
         
            
            let lf = 
                new LoggerFactory()
                
            lf.AddSerilog(logger)   
                
        if parameters.Level.IsSome then                
            levelSwitch.MinimumLevel <- (SerilogLevel parameters.Level.Value) 
                             
        loggerFactory

