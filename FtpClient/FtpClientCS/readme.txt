Notes

This is the C# version of FtpClient for you fans of 
curly brackets and semicolons.

Many thanks to Rasmus Kromann-Larsen for this version: his notes below!

--------------------------

Hey,

Here's the C# version of the FtpClient... I only made one change from the original... I changed:

_size = System.Convert.ToInt32(m.Groups["size"].Value);

to

Int64.TryParse(m.Groups ["size"].Value, out _size);

Since my ftp server doesn't actually send the size of directories.. 
So it failed with an exception.. The new code will just keep the initial 0 if it fails... 
And since it only fails for directories... :-D 


- Rasmus.
--------------------------
