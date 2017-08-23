# ftpclient

 > ### NOTE
 > This is a transfer of an old Codeplex project [FtpClient](https://ftpclient.codeplex.com/). CodePlex is  
 > [shutting down](https://aka.ms/codeplex-announcement) in December 2017, so I have transferred the code here to GitHub.


FTPclient implements a high-level FTP client library using the System.NET.FTPrequest library in the .NET Framework 2.0 and above.

Although .NET now supports FTP it is a very basic protocol-level support, hence I wrote this library to give easy access 
to .NET functionality. 


## Source 
I have done a straight port for now. The project is *very* old and originally targeted only .NET Framework 2.0.

## Usage 

To find out more about this library in the first instance read my [CodeProject article](https://www.codeproject.com/Articles/11991/An-FTP-client-library-for-NET). Originally written in VB.NET it has been translated into C# as well and I hope to maintain both versions as people will find it easier to debug and add functionality if it remains in both versions.

## Future

I am just porting the code to make it available post-Codeplex. I considered a .NET Standard / .NET Core version but the FtpWebRequest class is not part of the .NET Standard libraries. If you need an FTP client for Core I'd check out https://github.com/sparkeh9/CoreFTP
