# HTB_Updates
A Discord Bot that announces your members' Hack The Box solves.
To start own instance you need to configure it yourselve.

  1. Create own discord bot and set right permissions
  2. Make changes to appsettings.json
  3. You can use our dockerfile and docker-compose.

# Set up
# 1. Discord bot permissions
https://discord.com/developers/docs/intro -> https://discord.com/developers/applications

Create "New Application"
Navigate to /Bot section
Check all "Privileged Gateway Intents"
Check "Send messages" and "Embed links" in "Bot permissions"

# 2. Configure appsettings.json
Copy Bot token to appsettings.json file parameter "Token"
Add HTBUsername and HTBPassword
Retrieve "App Tokens" from HTB(https://app.hackthebox.com/profile/settings)
Set right "ConnectionString" (Make sure provided user has enough privileges)

# 3. Discord server
Under "Applications" section, click on your bot application, and open the OAuth2 page.
Navigate to URL Generator
Under scopes check bot
List of permissions will appear. Check "Send messages" and "Embed links".
Copy&Paste generated URL to the browser and invite bot to your server.

# 4. Extra tips
You might want to change dockerfile or docker-compose
Make sure Bot permissions are right
Make sure you have permissions on discord server where you want to add bot
Make sure Database "ConnectionString" is set right




