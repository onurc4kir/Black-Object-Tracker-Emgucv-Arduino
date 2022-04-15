# Black-Object-Tracker-Emgucv-Arduino
-------
I designed Arduino circuit that takes data from computer and move camera according to coming data and track black object. It also display where the object is on 16*2 lcd display and store its position with time in the SD card. You can also track other colors. Change upper and lower mask from C# code see what happen.

### Arduino Components
-Arduino nano
-2x Servo Motor 9g
-Webcam 
-16x2 LCD Display
-SD Card Module
-RTC Module 1302
-Jumper Wires

If you only want to track object just use servo motors and webcam other modules are not mandatory.


Change these values from code and track other colors.

     Bgr lower = new Bgr(0, 0, 0);
     Bgr upper = new Bgr(30, 30, 30);
