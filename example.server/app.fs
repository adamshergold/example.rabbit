namespace Example.Rabbit.Server

open Microsoft.Extensions.Logging 

open CommandLine

open Example.Rabbit.Lib
open Example.Rabbit.Lib.Command

module App =

    [<EntryPoint>]
    let Main args =

        let result = 
            CommandLine.Parser.Default.ParseArguments<RunOptions,InfoOptions>(args)

        let status, logger =
            match result with
            | :? Parsed<obj> as parsed ->

                let commonOpts = 
                    match parsed.Value with
                    | :? CommonOptions as co -> co 
                    | _ -> failwithf "Unable to parse Common Options - fatal!" 
                 
                let loggerFactory =
                
                    let parameters = 
                        Logging.Parameters.Default
                        
                    let parameters = 
                        if commonOpts.LogLevel.Length > 0 then 
                            { parameters with Level = Some commonOpts.LogLevel } 
                        else 
                            parameters
                        
                    Logging.CreateFactory parameters 
                
                let logger = 
                    loggerFactory.CreateLogger("Server")
                    
                logger.LogInformation( "App Started" )
                    
                match parsed.Value with
                | :? InfoOptions as opts ->
                    (opts.Execute logger), logger
                | :? RunOptions as opts ->
                    (opts.Execute logger), logger
                | _ ->
                    logger.LogCritical( "Unable to parse command line {Parsed}", parsed )
                    -1, logger
            | _ ->
                failwithf( "Unable to parse command line!" )

        logger.LogInformation( "App Completed {Status}", status ) 
                    
        status 
