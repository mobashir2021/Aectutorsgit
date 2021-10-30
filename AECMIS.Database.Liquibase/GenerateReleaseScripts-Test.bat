@echo off

:: these values have to be provided:
:: ------------------------------------------------------------------------------
set emptyDbName=lqbdemo-empty
set lastReleaseDbName=lqbdemo-inc
set fullScriptOutputFile=output-full.sql
set incrementalScriptOutputFile=output-incr.sql
set destinationDirectoryPath=d:\Temp\mic\liquibase-dest\output
set incrementalScriptsRepository=d:\Temp\mic\liquibase-dest\incremental-repo
:: ------------------------------------------------------------------------------
cls

call GenerateReleaseScripts.bat