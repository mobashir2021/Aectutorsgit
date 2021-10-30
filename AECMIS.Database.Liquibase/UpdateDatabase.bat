@echo off
if not defined noclear (
	cls
)

:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: DEFAULT VALUES
:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
set defaultServerPath=s09.winhost.com
set defaultCommand=Update
set defaultAuthentication=Sql
set defaultToFileMode=0
set defaultOutputFile=output.sql
set defaultDbName=DB_67643_aecmis
set username=DB_67643_aecmis_user
set password=1Darulishaat

:: check if help requested
if .%1==./help goto :help
@echo Try "UpdateDatabase.bat /help" to display help.

:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: SET (COMBINE) PROPERTIES
:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
if not defined dbname (	
	set dbname=%defaultDbName%
)
if not defined serverPath (
	set serverPath=%defaultServerPath%
)
if not defined securityMode (
	set securityMode=%defaultAuthentication%	
)
if not defined command (
	set command=%defaultCommand%
)
if not defined toFile (
	set toFile=%defaultToFileMode%
)
if not defined outputFile (
	set outputFile=%defaultOutputFile%
)

:: combine properties
if %securityMode%==Windows (
	set AuthParams= 
	set SecurityString=integratedSecurity=true
) else (
	set AuthParams=--username=%username% --password=%password%
	set SecurityString=User Id=%username%; Password=%password%
)

:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: DISPLAY VALUES SET
:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

@echo dbName is: %dbName%	
@echo serverPath is:	%serverPath%	
@echo securityMode is: %securityMode%	
@echo command is: %command%	
@echo toFile is: %toFile%	
@echo outputFile is: %outputFile%	

:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: EXECUTE LIQUIBASE
:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
set fullCommand=tools\liquibase-2.0.5\liquibase.bat %AuthParams% --classpath=tools\sqljdbc4.jar --driver=com.microsoft.sqlserver.jdbc.SQLServerDriver --url="jdbc:sqlserver://%serverPath%;databaseName=%dbname%;%SecurityString%" --changeLogFile=changelogs\_master.xml %command% %*

if %toFile%==1 (
	@echo on
	%fullCommand% | findstr /V "Liquibase 'UpdateSql' Successful" > %outputFile% 
	@echo off
)	else (
	@echo on
	%fullCommand%
	@echo off
)	
goto:eof

:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: DISPLAY HELP
:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:help
@echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
@echo :: SCRIPT HELP
@echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
@echo The following parameters should be set for running this script:
@echo dbname - database name to run against
@echo command (default: %defaultCommand%):
@echo    Update - update the database
@echo    UpdateSQL - generate sql scripts (do not update db)
@echo serverPath - path to sql server (default: %defaultServerPath%)
@echo securityMode (default: %defaultAuthentication%):
@echo    Windows - for integrated security
@echo    SQL - for sql server authentication 
@echo          (requires setting username and password)
@echo username - username for sql server authentication
@echo password - password for server sql authentication
@echo toFile - 0 or 1 - whether tu push output to file (default %defaultToFileMode%)
@echo outputFile - path to the output file, if toFile set to 1. (Default: %defaultOutputFile%)