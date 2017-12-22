# BotFW3-ParkingBotZH

## About
This is a sample using the Microsoft Bot Framework 3 to create a Bot using LUIS.ai for natural language intent detection.
The bot can tell you what parkings in Zürich have free parking available based on data by the Parkleitsystem Zürich http://pls-zh.ch

## Prepare Luis.ai
Log in to Luis.ai and create a Bot by uploading the included **ZurichParkingBotLuisEN.json** file.
Train and publish the LUIS.ai model and copy the URL presented to you by LUIS.ai on publishing. Note that **{APP_ID}**, **{SUBSCRIPTION_KEY}** and **{BING_KEY}** need to be replaced with the real values presented to you on the LUIS.ai publishing page:

    https://{AZURE_REGION}.api.cognitive.microsoft.com/luis/v2.0/apps/{APP_ID}?subscription-key={SUBSCRIPTION_KEY}&amp;spellCheck=true&amp;bing-spell-check-subscription-key={BING_KEY}&amp;verbose=true&amp;timezoneOffset=0&amp;q=

Copy it to the web.config file.

## Set up the bot
Log in to the [Azure Portal](http://portal.azure.com) and register a new **Bot Channels Registration**.
Copy down the **Micrsooft App ID** and **Microsoft App Password** and copy them to the **web.config** file of the main project.

    <add key="MicrosoftAppId" value="" />
    <add key="MicrosoftAppPassword" value="" />

## Set up CosmosDB for conversation state data storage

Create a new Azure Cosmos DB database in Azure. For the *API* field, select *SQL (DocumentDB)*. Click *Access keys* to find keys and connection strings. Your bot will use this information to call the storage service to save state data.

Replace the *CosmosDB URI* and *CosmosDB Key* in the Web.config file with the values displayed in the Azure Portal.

    <add key="DocumentDbUrl" value="https://{COSMOSDB_URL}.documents.azure.com:443/" />
    <add key="DocumentDbKey" value="" />

## Publish the bot

Deploy the bot project as an Azure Web App and note it's endpoint in the following format:

    https://[yourname].azurewebsites.net/api/messages

Copy this endpoint to the bot framework configuration page's "Messaging Endpoint" field.

Test your bot in the interactive chat window in the bot framework page.
