# Enchanted Mirror
**Enchanted Mirror** is being written to run on anything UWP apps can run on, although the original intent it to run on Windows IoT on a Rasberry Pi 2 (at least until an upgrade to a 3 is possible)

The intention is to be moduler, simple speech recognistion, person recognition and interactions with assistants such as Alexa and Google Home

## Planned features
|**Feature**|**Description**|**Status**|
|---|---|---|
|Digital Clock|Display the time in 12 or 24 hour format|:white_check_mark:|
|Analog Clock|Display an analog clock with customisation options||
|Current Date|Allow date to be displayed based on user defined configuration, for example `Wednesday, 29th June 2016`|:white_check_mark:|
|Current Weather|Obtain current weather information for a given location and display temperature, conditions, wind speed and direction, sunrise and sunset|:white_check_mark:|
|Weather Forecast|Display the weather outlook for a given location||
|News Headlines|Display headlines from feed|:white_check_mark:|
|Twitter|Display latest *x* tweets from Twitter||
|Diary|Integrate with, for example, Google to retrieve users schedule||
|Internet Radio|Play a radio stream|:white_check_mark:|
|Music Player|Basic music player||

### Speech control
The idea that there is a monitor of some description sat behind a two-way mirror doesn't appeal if it remains on at all time.

Therefore using the GPIO pins on a Rasberry PI, along with a relay and voice control, the plan is to use commands such as `go to sleep` to turn the screen off and `wake up` to turn the screen on.

Speech can also be used to:
* Display the detail behind a news headline
* Switch to a different news feed
* Display the detail of a sheduled item in the users diary
* Expand the diary view, i.e. `this week`, `this month` or `this afternoon`
* Switch radio stations
* Control the music player, such as next song, select play list etc.


## Contributing
Please bear with me. I'm new to publishing anything out to the public, so may take me a while to figure this out. As a result closed for contributions right now.

## Author
* **Steve Wood**

## License
The project is licensed under the [MIT license](LICENSE.MD)

## Inspiration and external references
* Inspiration from [MagicMirror²](https://github.com/MichMich/MagicMirror) by [Michael Teeuw](https://github.com/MichMich)
* [Weather API](https://openweathermap.org/api)
* [Weather Icons](http://erikflowers.github.io/weather-icons/) by [Erik Flowers](https://github.com/erikflowers)
