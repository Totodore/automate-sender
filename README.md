# Automate Sender

Heart of the automate bot.

This programm will precisely send all programed messages set on [the dashboard UI](https://github.com/totodore/automate) that need to be sent. 
In order to do that it waits a new minute at startup and then launch a routine that is executed every minute.

- All the frequential messages are pulled from database and the next send time & date is computed from the cron expression with the Cronos Lib
- All the ponctual messages that are programmed for the current minute are pulled from the database and sent.
- Every Discord Guild which don't have enough quota cannot send messages. Therefore all their messages are discarded until the quota is refreshed.
