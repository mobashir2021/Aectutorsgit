:: these values have to be provided:
:: ------------------------------------------------------------------------------
:: set emptyDbName=AECLive
:: set lastReleaseDbName=INCR_DB_NAME
:: set fullScriptOutputFile=output-full.sql
:: set incrementalScriptOutputFile=output-incr.sql
:: set destinationDirectoryPath=C:\Source\Private\aectutors\AECMIS.Database.Liquibase\liquibase-dest\output
:: set incrementalScriptsRepository=:\Source\Private\aectutors\AECMIS.Database.Liquibase\liquibase-dest\incremental-repo
:: ------------------------------------------------------------------------------
cls

::
:: Tell the script that we'll be saving output to file
::
set toFile=1
set noclear=1

:: 
:: Create directory structure (and delete first if dir already exists)
:: 
@echo :: 
@echo :: Create directory structure (and delete first if dir already exists)
@echo :: 
if  exist "%destinationDirectoryPath%" del /F /S /Q "%destinationDirectoryPath%"
mkdir "%destinationDirectoryPath%"
mkdir "%destinationDirectoryPath%\incremental"
mkdir "%destinationDirectoryPath%\full"
mkdir "%destinationDirectoryPath%\backup"

::
:: Run scripts against empty db - FULL SCRIPT
::
@echo ::
@echo :: Run scripts against empty db - FULL SCRIPT
@echo ::
set outputFile=%destinationDirectoryPath%\full\%fullScriptOutputFile%
set dbName=%emptyDbName%
set command=UpdateSql
call UpdateDatabase.bat 

::
:: Check STATUS to see if there are any changeSets to be executed
::
@echo ::
@echo :: Check STATUS to see if there are any changeSets to be executed
@echo ::
set outputFile=db-status.txt
set dbName=%lastReleaseDbName%
set command=status
call UpdateDatabase.bat 

:: check if there were any changes found (errorlevel = 0 means text found)
@echo check if there were any changes found (errorlevel = 0 means text found)
call findstr /I "change sets have not been applied" db-status.txt
if %errorlevel% == 1 (
	@echo Database is up to date, no incremental script needed.
	goto :previousScriptsCopy
)
::
:: Run scripts against last db release - INCREMENTAL SCRIPT
::
@echo ::
@echo :: Run scripts against last db release - INCREMENTAL SCRIPT
@echo ::
set outputFile=%incrementalScriptsRepository%\%incrementalScriptOutputFile%
set dbName=%lastReleaseDbName%
set command=UpdateSql
call UpdateDatabase.bat 

::
:: Now actually update the last release db
:: 
@echo ::
@echo :: Now actually update the last release db
@echo :: 
set toFile=0
set dbName=%lastReleaseDbName%
set command=Update
call UpdateDatabase.bat 

::
:: Copy the previous incremental scripts
::
:previousScriptsCopy
@echo ::
@echo :: Copy the previous incremental scripts
@echo ::
xcopy /D /Y "%incrementalScriptsRepository%\*.*" "%destinationDirectoryPath%\incremental"
