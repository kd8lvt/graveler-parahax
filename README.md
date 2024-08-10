# GravelerParaHax
An implementation of [arhourigan/graveler](https://github.com/arhourigan/graveler) in C#  
This implementation has two possible improvement percentages, based on ehther or not you choose to enable Austin Mode.  
Austin Mode makes the script roll all 177 turns, regardless of whether or not your Graveler would have survived up to that point. 
This is the default behaviour of Austin's code, but it is NOT the default behaviour of mine. Austin Mode has a performance improvement of ~4700% over Austin's Python script.  

The default behaviour of GravelerParaHax is to roll until something *other* than a 1 is rolled, and therefore your Graveler going boom. It then stops that iteration, and moves on. 
This has the added benifit of a total performance increase of ~820,000%, running in a minute and 30-40 seconds on my machine.

To enable Austin Mode, pass the `--austinMode` commandline argument when running the program.

# How do I use it?
Clone the repository and build it. Releases are provided for your convenience, but I don't recommend running .exe files off GitHub. 
If you're on Unix, Wine/Proton probably work, or you can build it for your distro. Up to you.  
Then, open a command prompt of your choice, and run the executable. If you want to run it in Austin Mode, you can pass `--austinMode` or `-a` when running it.