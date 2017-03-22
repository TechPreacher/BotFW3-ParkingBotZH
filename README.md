# BotFW3-ParkingBotZH

## About
This is a sample using the Microsoft Bot Framework 3 to create a Bot using LUIS.ai for natural language intent detection.
The bot can tell you what parkings in Zürich have free parking available based on data by the Parkleitsystem Zürich http://pls-zh.ch
The bot uses the German LUIS.ai but can easily be edited to use English.

## Prepare Luis.ai
Log in to Luis.ai and create a Bot by uploading the included **ParkingBot.json** file.
Train and publish the LUIS.ai model and copy the URL presented to you by LUIS.ai on publishing. Note that **[your_luis_app_id]** and **[your_key]** need to be replaced with the real values presented to you on the LUIS.ai publishing page:

    https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/[your_luis_app_id]?subscription-key=[your_key]&verbose=true

Add "&q=" to the end of the URL and copy it to the Luis.cs class.

## Set up the bot
Log in at http://dev.botframework.com and register a new bot.
Copy down the **Micrsooft App ID** and **Microsoft App Password** and copy them to the **web.config** file of the main project.

    <add key="MicrosoftAppId" value="" />
    <add key="MicrosoftAppPassword" value="" />

Deploy the bot project as an Azure Web App and note it's endpoint in the following format:

    https://[yourname].azurewebsites.net/api/messages

Copy this endpoint to the bot framework configuration page's "Messaging Endpoint" field.

Test your bot in the interactive chat window in the bot framework page.
