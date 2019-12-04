# Startet Postman Test gegen Entwicklungsrechner
# Stand 23.9.2019

# Newman ist das Kommandozeilenwerkzeug für Postman
# es basiert auf NodeJS https://www.npmjs.com/package/newman

cls
cd $PSScriptRoot

# newman installieren
npm install -g newman

# newman starten mit Test-Collection und Umgebung, Ausgabe im JUnit-Format
newman run MiracleList-APITests.postman_collection.json `
-e MiracleList-APITests.postman_environment.staging.json --disable-unicode `
--reporters cli junit --reporter-junit-export TestResults/newman-run-report.xml