See the comments at the top of Terminal.cs for information. REGARDING the SERIAL TERMINAL APP...
Original Serial Terminal App from: http://msmvps.com/blogs/coad/archive/2005/03/23/39466.aspx

for the project, see below



Mini Project for Computer Communication Networks
------------------------------------------------

Multi-Node Micro-Controller based Network for Implementation of Different MAC Protocols
=======================================================================================

Group Members:
* Ammad Afsar	(09003)
* Hafiz Maaz Ahmed	(09006)
* Hafiz Muhammad Zaid Pirwani	(09007) [Group Leader]
* Syeda Kulsoom Jafri	(09041)

Submitted to: Engr. Dr. Farah Haroon
------------------------------------
Course: Computer Communication Networks

8th Term, B.E. Industrial Electronics

Institute of Industrial Electronics Engineering, IIEE, PCSIR
------------------------------------------------------------
St-22/C, Block-6, Gulshan-e-Iqbal, Karachi-75300.
Ph #: +92-21-34982353, Fax #: +92-21-34966274
http://www.iiee.edu.pk





Mini Project Summary
--------------------
The project comprises of designing and making a network of multiple micro-controllers, we are putting in 3 AVR micro-controllers for now but more may be added. Once the network is created, different MAC Protocols may be implemented by simply changing the firmware in the micro-controllers. This will allow for better understanding and hands-on learning of the various MAC Protocols.
By MAC Protocols we mean to implement Channel Access Method or Multiple Access Methods are required in such a network where there are more than 2 terminals, to be connected to the same multi-point transmission medium to transmit over it and to share its capacity.

Scope of Project
----------------
The scope of this project is designing of the hardware which will allow a network of micro-controllers to communicate with one another and allow students to have a better learning experience of Channel Access Methods with the hands on approach of programming micro-controllers to implement different MAC Protocols and analyzing the data on the main communication channel in real-time.
We have hence designed a circuit which allows 3 micro-controllers to communicate on a single channel (using a tri-state output state when channel not in use) and a computer side C# application which shows the channel data in real-time and can also be programmed to detect and display collisions, packet information and packet data of each node.
The hardware also includes an on-board Ejaaduino which is an AVR/Arduino Development Board and is used in this project as a USB-to-Serial Converter and as a programmer to program the micro-controllers on the board.
