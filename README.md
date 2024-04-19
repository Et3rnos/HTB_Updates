# HTB Updates

A Discord bot that announces your members' Hack The Box solves.

There are 2 ways to use this bot: you can either invite it to your discord server or you can create your own instance of it.

## Invite the bot

To invite this bot to your server all you need to do is visit this link:

[https://discord.com/api/oauth2/authorize?client_id=806824180074938419&permissions=277025441856&scope=bot+applications.commands](https://discord.com/api/oauth2/authorize?client_id=806824180074938419&permissions=277025441856&scope=bot+applications.commands)

<br>

**Note:** Running your own bot instance won't work anymore as the code in this repo is outdated, please invite the bot to your server.

> ## Creating your own instance
> 
> To create your own instance you need to follow some certain steps.
> 
> This guide will assume you have a discord bot and a MySQL server already set up.
> 
> ### Clone this repository
> 
> Just run `git clone https://github.com/Et3rnos/HTB_Updates`.
> 
> ### Creating the configuration
> 
> There are 2 configuration files that you must create, one for the discord bot application and the other for the website backend (if you plan on using it).
> 
> Templates for these files can be found at `./HTB Updates Discord Bot/appsettings.json.sample` and `./htb_updates_backend/appsettings.json.sample`.
> 
> You can simply modify them and rename them to `appsettings.json` afterwards.
> 
> ### Launching the application
> 
> This step requires you to have docker installed.
> 
> Simply run `docker compose up -d`.

## Support Me

Want to support the development of this project? Consider supporting me at [https://patreon.com/et3rnos](https://patreon.com/et3rnos).
